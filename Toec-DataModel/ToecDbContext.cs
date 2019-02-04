using System;
using System.Data.Entity;
using System.Data.SQLite;
using Toec_Common.Entity;

namespace Toec_DataModel
{
    internal class ToecDbContext : DbContext
    {
        public ToecDbContext()
            : base(
                new SQLiteConnection
                {
                    ConnectionString =
                        string.Format(@"data source={0}\Toec\Toec.db",
                            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
                }, true)
        {
        }

        public DbSet<EntityPolicyHistory> PolicyHistories { get; set; }
        public DbSet<EntitySetting> Settings { get; set; }
        public DbSet<EntityAppMonitor> AppMonitors { get; set; }
        public DbSet<EntityUserLogin> UserLogins { get; set; }
    }
}