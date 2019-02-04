using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_DataModel;

namespace Toec_Services.Entity
{
    public class ServiceAppMonitor : IDisposable
    {
        private readonly UnitOfWork _uow;

        public ServiceAppMonitor()
        {
            _uow = new UnitOfWork();
        }

        public DtoActionResult AddAppEvent(EntityAppMonitor appMonitor)
        {
            if(string.IsNullOrEmpty(appMonitor.UserName) || string.IsNullOrEmpty(appMonitor.Name) || string.IsNullOrEmpty(appMonitor.Path) || string.IsNullOrEmpty(appMonitor.StartDateTime))
            return new DtoActionResult() {Success = true}; //ignored

            var actionResult = new DtoActionResult();
            _uow.AppMonitorRepository.Insert(appMonitor);
            _uow.Save();
            actionResult.Success = true;
            actionResult.Id = appMonitor.Id;
            return actionResult;
        }

        public bool DeleteAll()
        {
            _uow.AppMonitorRepository.DeleteRange(x => x.Id > 0);
            _uow.Save();
            return true;
        }

        public bool DeleteEvents(List<EntityAppMonitor> events)
        {
            foreach (var e in events)
            {
                var exists = _uow.AppMonitorRepository.GetById(e.Id);
                if (exists == null) continue;
                _uow.AppMonitorRepository.Delete(e.Id);
            }
            _uow.Save();
            return true;
        }

        public List<EntityAppMonitor> GetCompletedEvents()
        {
            return
                _uow.AppMonitorRepository.Get(
                    x => !string.IsNullOrEmpty(x.StartDateTime) && !string.IsNullOrEmpty(x.EndDateTime));
        }

        public DtoActionResult CleanupOldEvents()
        {
            //cleanup old evens that never closed
            var events = _uow.AppMonitorRepository.Get(x => string.IsNullOrEmpty(x.EndDateTime));
            foreach (var e in events)
            {
                if (string.IsNullOrEmpty(e.StartDateTime)) continue;
                var dateTime = Convert.ToDateTime(e.StartDateTime);
                var deleteThreshold = DateTime.UtcNow - TimeSpan.FromDays(7);
                if (dateTime < deleteThreshold)
                {
                    _uow.AppMonitorRepository.Delete(e.Id);
                }
            }

            _uow.Save();
            return new DtoActionResult(){Success = true};
        }

        public DtoActionResult RemoveDuplicates()
        {
            //some applications open many instances of itself, skewing the total time numbers, try to remove those duplicates
            var completedEvents = GetCompletedEvents();
            var distinctStartEvents = completedEvents.GroupBy(x => new {x.Path, x.StartDateTime, x.UserName});

            var remainingEvents = new List<EntityAppMonitor>();
            foreach (var g in distinctStartEvents)
            {

                var lastEvent = g.Select(x => x.EndDateTime).OrderByDescending(x => x).FirstOrDefault();
                if (lastEvent == null) continue;

                var counter = 0;
                foreach (var p in g)
                {
                    //verify object still exists
                    var exists = _uow.AppMonitorRepository.GetById(p.Id);
                    if (exists == null) continue;

                    if (counter == 0)
                    {
                        p.EndDateTime = lastEvent;
                        _uow.AppMonitorRepository.Update(p,p.Id);
                        remainingEvents.Add(p);
                    }
                    else
                    {
                        _uow.AppMonitorRepository.Delete(p.Id);
                    }
                    _uow.Save();
                    counter++;
                }
            }

            var distinctEndEvents = remainingEvents.GroupBy(x => new { x.Path, x.EndDateTime, x.UserName });
            foreach (var e in distinctEndEvents)
            {
                var firstEvent = e.Select(x => x.StartDateTime).OrderBy(x => x).FirstOrDefault();
                if (firstEvent == null) continue;

                var counter = 0;
                foreach (var p in e)
                {
                    //verify object still exists
                    var exists = _uow.AppMonitorRepository.GetById(p.Id);
                    if (exists == null) continue;

                    if (counter == 0)
                    {
                        p.StartDateTime = firstEvent;
                        _uow.AppMonitorRepository.Update(p, p.Id);
                    }
                    else
                    {
                        _uow.AppMonitorRepository.Delete(p.Id);
                    }
                    _uow.Save();
                    counter++;
                }
            }

            //One last round
            var allEvents = GetCompletedEvents();

            foreach (var g in allEvents)
            {
                var startTime = g.StartDateTime;
                var endTime = g.EndDateTime;
                var userName = g.UserName;
                var path = g.Path;
                if (startTime == null || userName == null || path == null || endTime == null) continue;

                var dateStartTime = Convert.ToDateTime(startTime);
                var dateEndTime = Convert.ToDateTime(endTime);
                foreach (var p in allEvents.Where(x => x.Path.Equals(path) && x.UserName.Equals(userName)))
                {

                    if (dateStartTime > Convert.ToDateTime(p.StartDateTime) && dateEndTime < Convert.ToDateTime(p.EndDateTime))
                    {
                        _uow.AppMonitorRepository.Delete(g.Id);
                        _uow.Save();
                        break;
                    }
                }
            }

            return new DtoActionResult();
        }

        public DtoActionResult CloseSinceServiceStart()
        {
            var serviceStartTime = DtoGobalSettings.ServiceStartTime;
            var openProcess = _uow.AppMonitorRepository.Get(x => string.IsNullOrEmpty(x.EndDateTime));
            foreach (var p in openProcess)
            {
                try
                {
                    var appStartTime = Convert.ToDateTime(p.StartDateTime);
                    if (appStartTime > serviceStartTime)
                    {
                        p.EndDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                        _uow.AppMonitorRepository.Update(p, p.Id);
                        _uow.Save();
                    }
                }
                catch
                {
                    //ignored
                }
               
            }

            return new DtoActionResult() {Success = true};
        }

        public DtoActionResult CloseAllOpen()
        {
            var openProcess = _uow.AppMonitorRepository.Get(x => string.IsNullOrEmpty(x.EndDateTime));
            foreach (var p in openProcess)
            {
                try
                {
                    var runningProcess = Process.GetProcessById(p.Pid);
                    var fileName = runningProcess.MainModule.FileName;
                    if(fileName.Equals(p.Path))
                        p.EndDateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                    _uow.AppMonitorRepository.Update(p, p.Id);
                    _uow.Save();
                }
                catch
                {
                    //ignored
                    continue;
                }
                
            }
            return new DtoActionResult() {Success = true};
        }

        public DtoActionResult CloseAppEvent(EntityAppMonitor appMonitor)
        {
            var closingApp = _uow.AppMonitorRepository.GetFirstOrDefault(x => x.Pid.Equals(appMonitor.Pid) && x.Name.Equals(appMonitor.Name) && x.UserName.Equals(appMonitor.UserName));
            if (closingApp == null) return new DtoActionResult() {Success = true};
            closingApp.EndDateTime = appMonitor.EndDateTime;
            
            var actionResult = new DtoActionResult();
            _uow.AppMonitorRepository.Update(closingApp, closingApp.Id);
            _uow.Save();
            actionResult.Success = true;
            actionResult.Id = appMonitor.Id;
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
                    _uow.Dispose();
                }
            }
            this.disposed = true;
        }

       
    }
}