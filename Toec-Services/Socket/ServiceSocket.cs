using log4net;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Toec_Common.Dto;
using Toec_Services.Entity;
using System.Runtime.Caching;
using System.Timers;

namespace Toec_Services.Socket
{
    public class ServiceSocket
    {
        private HubConnection _hubConnection;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string _logId;
        private readonly ulong requestMaxAgeInSeconds = 600; //5 mins
        public static Timer _timer;
        private bool _socketConnecting = false;

        public ServiceSocket(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;
            _logId = Guid.NewGuid().ToString("n").Substring(0, 8);
        }
        
        public HubConnection ConnectSocket()
        {
            if (_hubConnection == null)
            {
                Logger.Debug("Establishing Connection To Com Server Web Socket.");
                StartWebSocket();
            }
            else
            {
                //reconnect socket on checkin if not already connected
                if (_hubConnection.State == ConnectionState.Disconnected)
                {
                    Logger.Debug("Re-establishing Connection To Com Server Web Socket.");
                    if (!_socketConnecting)
                        StartWebSocket();
                }
            }

            return _hubConnection;
        }

        private void StartWebSocket()
        {
            _socketConnecting = true;

            if(_timer == null)
            {
                _timer = new Timer();
                _timer.Elapsed += OnTimedEvent;
                _timer.Interval = 30000;
                _timer.Enabled = true;
                OnTimedEvent(null, null);
            }         

            var deviceThumbprint = new ServiceSetting().GetSetting("device_thumbprint");
            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null)
            {
                Logger.Error("Could Not Find The Device Certificate For Web Socket Connection.");
                Logger.Info("Server Push Events Will Not Be Available");
                return;
            }

            if (DtoGobalSettings.ComServer == null || DtoGobalSettings.ClientIdentity == null)
            {
                Logger.Info("Cannot Connect To Web Socket.  The Com Server Has Not Yet Been Set.");
                Logger.Info("Server Push Events Will Not Be Available");
                return;
            }

            if(_hubConnection != null)
            {
                _hubConnection.Stop();
                _hubConnection.Dispose();
            }

            _hubConnection = new HubConnection(DtoGobalSettings.ComServer);
            _hubConnection.Headers.Add("certificate", Convert.ToBase64String(deviceCert.GetRawCertData()));
            _hubConnection.Headers.Add("computerGuid", DtoGobalSettings.ClientIdentity.Guid);
            _hubConnection.Headers.Add("comServer", DtoGobalSettings.ComServer);

            var hubProxy = _hubConnection.CreateHubProxy("ActionHub");
            _hubConnection.Error += HubConnection_Error;
            _hubConnection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.Info("Could Not Connect To Web Socket");
                    Logger.Error(task.Exception.GetBaseException());
                    Logger.Info("Server Push Events Will Not Be Available");
                    _socketConnecting = false;
                    return;
                }
                else
                {
                    Logger.Debug("Web Socket Connected.  Connection ID: " + _hubConnection.ConnectionId);
                    var v = hubProxy.Invoke<DtoSocketServerVerify>("VerifyServer").Result;
                    if (isValidRequest(v))
                    {
                        hubProxy.On<DtoHubAction>("ClientAction", hubAction => new ServiceHubAction().Process(hubAction));
                        _socketConnecting = false;
                    }
                    else
                    {
                        Logger.Debug("Socket Server Verification Failed.  Disconnecting.");
                        _hubConnection.Stop();
                        _socketConnecting = false;
                    }

                }

            }).Wait();

        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if(_hubConnection != null)
            {
                if (_hubConnection.State == ConnectionState.Disconnected)
                    if(!_socketConnecting)
                        StartWebSocket();
            }
        }

        private bool VerifyServer(DtoSocketServerVerify verification)
        {

            return true;
        }

        private void HubConnection_Error(Exception obj)
        {
            Logger.Debug($"SignalR error: {obj.Message}");
        }



        private bool isValidRequest(DtoSocketServerVerify verification)
        {
            if (isReplayRequest(verification.nOnce, verification.Timestamp))
            {
                Logger.Debug($"ID: {_logId} - Request appears to be a replay, denying {verification.nOnce} {verification.Timestamp}");
                return false;
            }

            var deviceThumbprint = new ServiceSetting().GetSetting("device_thumbprint");
            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null)
            {
                Logger.Error("Could Not Find The Device Certificate For Signature Verification.");
                return false;
            }

            if (!ServiceCertificate.VerifySignature(deviceCert, Convert.FromBase64String(verification.signature), verification.Timestamp + verification.nOnce))
            {
                return false;
            }
            return true;
        }

        private bool isReplayRequest(string nonce, string requestTimeStamp)
        {
            if (MemoryCache.Default.Contains(nonce))
            {
                Logger.Debug($"ID: {_logId} - This nonce has already been used");
                return true;
            }

            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var currentTs = DateTime.UtcNow - epochStart;

            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            Logger.Debug($"ID: {_logId} - Server Timestamp Seconds " + serverTotalSeconds);
            Logger.Debug($"ID: {_logId} - Request Timestamp Seconds " + requestTimeStamp);

            if (requestTotalSeconds > serverTotalSeconds)
            {
                Logger.Debug($"ID: {_logId} - Server time is behind client, allowing 5 minute discrepancy");
                //server time is behind client, give it a 5 min window
                serverTotalSeconds += 300;
            }

            var timeStampDifference = serverTotalSeconds - requestTotalSeconds;
            Logger.Debug($"ID: {_logId} - Timestamp difference: " + timeStampDifference);
            if (timeStampDifference > requestMaxAgeInSeconds)
            {
                Logger.Debug($"ID: {_logId} - Request has exceeded the maximum request age of 600 seconds");
                return true;
            }

            MemoryCache.Default.Add(nonce, requestTimeStamp,
                DateTimeOffset.UtcNow.AddSeconds(requestMaxAgeInSeconds));

            return false;
        }
    }
}
