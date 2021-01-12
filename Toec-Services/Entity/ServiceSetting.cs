using System;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_DataModel;

namespace Toec_Services.Entity
{
    public class ServiceSetting : IDisposable
    {
        private readonly UnitOfWork _uow;

        public ServiceSetting()
        {
            _uow = new UnitOfWork();
        }

        public EntitySetting GetSetting(string settingName)
        {
            return _uow.SettingRepository.GetFirstOrDefault(x => x.Name.Equals(settingName));
        }

        public DtoActionResult UpdateSettingValue(EntitySetting setting)
        {
            var actionResult = new DtoActionResult();

            _uow.SettingRepository.Update(setting, setting.Id);
            _uow.Save();
            actionResult.Success = true;
            actionResult.Id = setting.Id;
            return actionResult;
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
                    if(_uow != null)
                    _uow.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}