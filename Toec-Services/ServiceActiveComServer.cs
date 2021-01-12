using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using Toec_Common.Dto;
using Toec_Services.ApiCall;
using Toec_Services.Entity;

namespace Toec_Services
{
    public class ServiceActiveComServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ServiceSetting _serviceSetting;
        private List<string> _comServers;


        public ServiceActiveComServer()
        {
            _comServers = new List<string>();
            _serviceSetting = new ServiceSetting();
        }

        private bool IsValidURI(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return false;
            Uri result;
            return Uri.TryCreate(uri, UriKind.Absolute, out result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private void RemoveInvalidUris()
        {
            if (_comServers.Count == 0) return;
            var toRemove = new List<string>();
            foreach (var com in _comServers.Where(com => !IsValidURI(com)))
            {
                Logger.Debug("Com Server: " + com + " Is Invalid.  Removing From List.");
                toRemove.Add(com);
            }

            foreach (var s in toRemove)
                _comServers.Remove(s);
        }

        public bool Set()
        {
            while (true)
            {
                Logger.Debug("Trying To Establish Client Com Server");

                //Check if active com servers have been defined

                var activeComServers = _serviceSetting.GetSetting("active_com_servers").Value;
                var initialComServers = _serviceSetting.GetSetting("initial_com_servers").Value;
                var passiveComServers = _serviceSetting.GetSetting("passive_com_servers").Value;
                if (!string.IsNullOrEmpty(activeComServers))
                {
                    _comServers = activeComServers.Split(',').ToList();
                }

                RemoveInvalidUris();

                if (!TestConnectionForActive())
                {
                    Logger.Debug("Could Not Connect To Any Active Com Servers, Falling Back To Passive Com Servers");

                    if (!string.IsNullOrEmpty(passiveComServers))
                    {
                        _comServers = passiveComServers.Split(',').ToList();
                    }

                    RemoveInvalidUris();

                    if (!TestConnection())
                    {
                        Logger.Debug("Could Not Connect To Any Passive Com Servers, Falling Back To Initial Com Servers");

                        if (!string.IsNullOrEmpty(initialComServers))
                        {
                            _comServers = initialComServers.Split(',').ToList();
                        }

                        RemoveInvalidUris();

                        if (!TestConnection())
                        {
                            Logger.Debug("Could Not Connect To Any Initial Com Servers");
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                Logger.Error("Could Not Connect To Any Client Com Servers.  Delaying 30 Seconds Before Next Retry.");
                Task.Delay(30*1000).Wait();
            }

            Logger.Debug("Com Server Set To: " + DtoGobalSettings.ComServer);
            return true;
        }

      

        private bool TestConnectionForActive()
        {
            Logger.Debug("Looking For Best Active Com Server.");
            //When looking for an active com server, a connection is made to the one with the least number of download connections
            if (_comServers.Count <= 1) return TestConnection();

            var comConnections = new List<DtoComServerConnection>();
            foreach (var active in _comServers)
            {
                //try to get the active download connections from any active com server
                DtoGobalSettings.ComServer = active;
                if (!DtoGobalSettings.ComServer.EndsWith("/"))
                    DtoGobalSettings.ComServer += "/";

                comConnections =
                    new APICall().ProvisionApi.ComConnections(DtoGobalSettings.ClientIdentity.Name);
                if (comConnections == null) continue;

                //the connections have been established, break out and try to set one as the com server
                break;
            }

            if (comConnections == null) return TestConnection();
            if (comConnections.Count == 0) return TestConnection();

            //remove com servers from orderedconnections that this client is not assigned to use.
            var toRemove = new List<DtoComServerConnection>();
            foreach (var con in comConnections)
            {
                if(!_comServers.Contains(con.ComUrl))
                    toRemove.Add(con);
            }
            foreach (var con in toRemove)
            {
                comConnections.Remove(con);
            }

            //try to set com server
            var connectionGrouping = comConnections.GroupBy(x => new {x.Connections}).Select(g => g.First()).OrderBy(x => x.Connections).ToList();
            foreach (var group in connectionGrouping)
            {
                var groupServers = comConnections.Where(x => x.Connections == group.Connections).Select(x => x.ComUrl).ToList();

                var index = DtoGobalSettings.Rnd.Next(groupServers.Count);
                DtoGobalSettings.ComServer = groupServers[index];
                if (!DtoGobalSettings.ComServer.EndsWith("/"))
                    DtoGobalSettings.ComServer += "/";

                Logger.Debug("Verifying Connection To: " + DtoGobalSettings.ComServer);
                var success = new APICall().ProvisionApi.ComConnectionTest(DtoGobalSettings.ClientIdentity.Name);
                while (!success)
                {
                    Logger.Debug("Failed To Connect To: " + DtoGobalSettings.ComServer);
                    groupServers.Remove(DtoGobalSettings.ComServer);
                    if (groupServers.Count == 0)
                        break;
                    index = DtoGobalSettings.Rnd.Next(groupServers.Count);
                    DtoGobalSettings.ComServer = groupServers[index];
                    if (!DtoGobalSettings.ComServer.EndsWith("/"))
                        DtoGobalSettings.ComServer += "/";
                    Task.Delay(15 * 1000).Wait();
                    Logger.Debug("Verifying Connection To: " + DtoGobalSettings.ComServer);
                    success = new APICall().ProvisionApi.ComConnectionTest(DtoGobalSettings.ClientIdentity.Name);
                }

                if (success) return true;
            }
            return false;
        }

        private bool TestConnection()
        {
            //when looking for a fallback or initial com server, a random one is selected, not dependent on current number of download connections
            if (_comServers.Count == 0) return false;
            var index = DtoGobalSettings.Rnd.Next(_comServers.Count);
            DtoGobalSettings.ComServer = _comServers[index];
            if (!DtoGobalSettings.ComServer.EndsWith("/"))
                DtoGobalSettings.ComServer += "/";

            Logger.Debug("Verifying Connection To: " + DtoGobalSettings.ComServer);
            while (!new APICall().ProvisionApi.ComConnectionTest(DtoGobalSettings.ClientIdentity.Name))
            {
                Logger.Debug("Failed To Connect To: " + DtoGobalSettings.ComServer);
                _comServers.Remove(DtoGobalSettings.ComServer);
                if (_comServers.Count == 0)
                    return false;
                index = DtoGobalSettings.Rnd.Next(_comServers.Count);
                DtoGobalSettings.ComServer = _comServers[index];
                if (!DtoGobalSettings.ComServer.EndsWith("/"))
                    DtoGobalSettings.ComServer += "/";
                Task.Delay(15*1000).Wait();
                Logger.Debug("Verifying Connection To: " + DtoGobalSettings.ComServer);
            }

            return true;
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
                    if(_serviceSetting != null)
                    _serviceSetting.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}