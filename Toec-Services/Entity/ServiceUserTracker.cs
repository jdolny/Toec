using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_DataModel;
using Toec_Services.Policy.Modules;

namespace Toec_Services.Entity
{
    public class ServiceUserTracker : IDisposable
    {
        private readonly UnitOfWork _uow;

        public ServiceUserTracker()
        {
            _uow = new UnitOfWork();
        }

        public DtoActionResult AddTrackerEvent(EntityUserLogin userLogin)
        {
            var actionResult = new DtoActionResult();
            _uow.UserLoginRepository.Insert(userLogin);
            _uow.Save();
            actionResult.Success = true;
            actionResult.Id = userLogin.Id;
            return actionResult;
        }

        public bool DeleteAll()
        {
            _uow.UserLoginRepository.DeleteRange(x => x.Id > 0);
            _uow.Save();
            return true;
        }

        public bool DeleteEvents(List<EntityUserLogin> events)
        {
            foreach (var e in events)
            {
                var exists = _uow.UserLoginRepository.GetById(e.Id);
                if (exists == null) continue;
                _uow.UserLoginRepository.Delete(e.Id);
            }
            _uow.Save();
            return true;
        }

        public List<EntityUserLogin> GetUserCompletedEvents()
        {
            return
                _uow.UserLoginRepository.Get(
                    x => !string.IsNullOrEmpty(x.LoginDateTime) && !string.IsNullOrEmpty(x.LogoutDateTime));
        }

        public EntityUserLogin GetUserLastLogin(string userName)
        {
            var login = _uow.UserLoginRepository.Get(x => x.UserName.Equals(userName)).OrderByDescending(x => x.Id);
            return login.Any() ? login.FirstOrDefault() : null;
        }

        public DtoActionResult UpdateTrackerEvent(EntityUserLogin userLogin)
        {
            var u = _uow.UserLoginRepository.GetById(userLogin.Id);
            if (u == null) return new DtoActionResult {ErrorMessage = "Login Id Not Found", Id = 0};

            var actionResult = new DtoActionResult();

            _uow.UserLoginRepository.Update(userLogin, userLogin.Id);
            _uow.Save();
            actionResult.Success = true;
            actionResult.Id = userLogin.Id;
            return actionResult;
        }

        public DtoActionResult CleanupOldEvents()
        {
            //cleanup old events that never closed
            var events = _uow.UserLoginRepository.Get(x => string.IsNullOrEmpty(x.LogoutDateTime));
            foreach (var e in events)
            {
                if (string.IsNullOrEmpty(e.LoginDateTime)) continue;
                var dateTime = Convert.ToDateTime(e.LoginDateTime,CultureInfo.InvariantCulture);
                var deleteThreshold = DateTime.UtcNow - TimeSpan.FromDays(14);
                if (dateTime < deleteThreshold)
                {
                    _uow.UserLoginRepository.Delete(e.Id);
                }
            }

            _uow.Save();
            return new DtoActionResult() { Success = true };
        }

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _uow.Dispose();
                }
            }
            this.disposed = true;
        }

        public void LogoutAllUsers()
        {
            if (ModuleUserLogins._timer != null)
            {
                if (ModuleUserLogins._timer.Enabled)
                {
                    ModuleUserLogins._timer.Stop();
                }
            }

            var users = new ServiceUserLogins().GetUsersLoggedIn();
            foreach (var user in users)
            {
                //log them all out
                var loginEntity = new ServiceUserTracker().GetUserLastLogin(user);
                if (loginEntity != null)
                {
                    loginEntity.LogoutDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                    new ServiceUserTracker().UpdateTrackerEvent(loginEntity);
                }
            }

        }
    }
}