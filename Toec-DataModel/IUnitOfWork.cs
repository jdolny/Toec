using System;
using Toec_Common.Entity;

namespace Toec_DataModel
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<EntityPolicyHistory> PolicyHistoryRepository { get; }
        IGenericRepository<EntitySetting> SettingRepository { get; }
        IGenericRepository<EntityAppMonitor> AppMonitorRepository { get; } 
        IGenericRepository<EntityUserLogin> UserLoginRepository { get; }
        void Save();
    }
}