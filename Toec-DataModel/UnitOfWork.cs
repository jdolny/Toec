using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using log4net;
using Toec_Common.Entity;

namespace Toec_DataModel
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ToecDbContext _context = new ToecDbContext();

        private readonly ILog log = LogManager.GetLogger(typeof (UnitOfWork));
        private IGenericRepository<EntityPolicyHistory> _policyHistoryRepository;
        private IGenericRepository<EntitySetting> _settingRepository;
        private IGenericRepository<EntityUserLogin> _userLoginRepository;
        private IGenericRepository<EntityAppMonitor> _appMonitorRepository; 

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IGenericRepository<EntityAppMonitor> AppMonitorRepository
        {
            get
            {
                return _appMonitorRepository ??
                       (_appMonitorRepository = new GenericRepository<EntityAppMonitor>(_context));
            }
        }

        public IGenericRepository<EntityPolicyHistory> PolicyHistoryRepository
        {
            get
            {
                return _policyHistoryRepository ??
                       (_policyHistoryRepository =
                           new GenericRepository<EntityPolicyHistory>(_context));
            }
        }

        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    log.Error(
                        string.Format(
                            "{0}: Entity of type \"{1}\" in state \"{2}\" has the following validation errors:",
                            DateTime.Now, eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        log.Error(string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (DbUpdateException ex)
            {
                log.Error(ex.Message);
                log.Error(ex.InnerException);
                throw;
            }
        }

        public IGenericRepository<EntitySetting> SettingRepository
        {
            get
            {
                return _settingRepository ??
                       (_settingRepository =
                           new GenericRepository<EntitySetting>(_context));
            }
        }

        public IGenericRepository<EntityUserLogin> UserLoginRepository
        {
            get
            {
                return _userLoginRepository ??
                       (_userLoginRepository =
                           new GenericRepository<EntityUserLogin>(_context));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}