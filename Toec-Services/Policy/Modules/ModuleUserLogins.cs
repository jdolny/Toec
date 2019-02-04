using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using Toec_Common.Entity;
using Toec_Services.Entity;

namespace Toec_Services.Policy.Modules
{
    public class ModuleUserLogins
    {
        public static Timer _timer;
        private readonly object ObjectLock = new object();
        private List<string> _loggedInUsers;

        public ModuleUserLogins()
        {
            _loggedInUsers = new List<string>();
        }

        private static bool LoginPolicyRunning { get; set; }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            lock (ObjectLock)
            {
                var users = new ServiceUserLogins().GetUsersLoggedIn();
                foreach (var user in users)
                {
                    if (!_loggedInUsers.Contains(user))
                    {
                        //user logged in
                        var en = new EntityUserLogin();
                        en.UserName = user;
                        en.LoginDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                        new ServiceUserTracker().AddTrackerEvent(en);
                    }
                    else
                    {
                        _loggedInUsers.Remove(user);
                    }
                }
                foreach (var user in _loggedInUsers)
                {
                    //user logged out
                    var loginEntity = new ServiceUserTracker().GetUserLastLogin(user);
                    if (loginEntity != null)
                    {
                        loginEntity.LogoutDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                        new ServiceUserTracker().UpdateTrackerEvent(loginEntity);
                    }
                }

                _loggedInUsers = users;
            }
        }

        public void Run()
        {
            if (LoginPolicyRunning)
                return;
            LoginPolicyRunning = true;
            _timer = new Timer();
            _timer.Elapsed += OnTimedEvent;
            _timer.Interval = 10000;
            _timer.Enabled = true;
            OnTimedEvent(null, null);
        }
    }
}