using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toec_Common.Dto;
using Toec_Common.Enum;
using Toec_Services.ApiCall;

namespace Toec_Services
{
    internal class ServiceDownloadConnectionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool CreateConnection(EnumPolicy.Trigger trigger)
        {
            //grab a download slot
            Logger.Debug("Obtaining A Download Connection.");
            var downloadConRequest = new DtoDownloadConRequest();
            downloadConRequest.ComputerGuid = DtoGobalSettings.ClientIdentity.Guid;
            downloadConRequest.ComputerName = DtoGobalSettings.ClientIdentity.Name;
            downloadConRequest.ComServer = DtoGobalSettings.ComServer;

            var downloadConnection = new DtoDownloadConnectionResult();
            if (trigger == EnumPolicy.Trigger.Login)
                downloadConnection = new APICall().LocalApi.CreateDownloadConnection(downloadConRequest);
            else
                downloadConnection = new APICall().PolicyApi.CreateDownloadConnection(downloadConRequest);
            var conAttempCounter = 0;
            while (downloadConnection.QueueIsFull || !downloadConnection.Success)
            {
                if (!downloadConnection.Success)
                {
                    Logger.Error("Could Not Obtain Download Connection. " + downloadConnection.ErrorMessage);
                    DtoGobalSettings.PolicyIsRunning = false;
                    return false;
                }
                if (downloadConnection.QueueIsFull && conAttempCounter == 0)
                    Logger.Debug("Download Connections Are Full.  Will Retry Continuously Every 1 Minute For The Next 10 Minutes.");

                Task.Delay(60 * 1000).Wait();
                conAttempCounter++;
                if (conAttempCounter == 10)
                {
                    Logger.Debug("Download Connections Remain Filled.  Giving Up.  Will Retry At Next Checkin.");
                    return false;
                }
                if (trigger == EnumPolicy.Trigger.Login)
                    downloadConnection = new APICall().LocalApi.CreateDownloadConnection(downloadConRequest);
                else
                    downloadConnection = new APICall().PolicyApi.CreateDownloadConnection(downloadConRequest);
            }
            return true;
        }

        public void RemoveConnection(EnumPolicy.Trigger trigger)
        {
            Logger.Debug("Releasing The Download Connection.");
            var downloadConRequest = new DtoDownloadConRequest();
            downloadConRequest.ComputerGuid = DtoGobalSettings.ClientIdentity.Guid;
            downloadConRequest.ComputerName = DtoGobalSettings.ClientIdentity.Name;
            downloadConRequest.ComServer = DtoGobalSettings.ComServer;
            if (trigger == EnumPolicy.Trigger.Login)
                new APICall().LocalApi.RemoveDownloadConnection(downloadConRequest);
            else
                new APICall().PolicyApi.RemoveDownloadConnection(downloadConRequest);
        }
    }
}
