using System;
using System.Collections.Generic;
using System.Management;
using System.Reflection;
using log4net;

namespace Toec_Services
{
    public class ServiceWmi<T> : IDisposable where T : new()
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly T _instance;
        private readonly string _query;
        private readonly string _scope;
        private bool _disposed;

        private ManagementObjectCollection moc;

        public ServiceWmi(T instance, string scope = "\\")
        {
            _instance = instance;
            _scope = scope;
            _query = instance.GetType().GetField("Query").GetValue(instance).ToString();
            SetObjectCollection();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if(moc != null)
                moc.Dispose();
            }

            _disposed = true;
        }

        public T Execute()
        {
            try
            {
                var type = _instance.GetType();

                foreach (var o in moc)
                {
                    foreach (var a in type.GetProperties())
                    {

                        if (o[a.Name] != null)
                            a.SetValue(_instance, o[a.Name], null);


                    }
                    //Can only handle one object in the collection, if more than 1 expected use GetObjectList
                    break;
                }

                
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
             
            }

            return _instance;
           
        }

        ~ServiceWmi()
        {
            Dispose(false);
        }

        public List<T> GetObjectList()
        {
            var list = new List<T>();

            try
            {
                var inst = (T)Activator.CreateInstance(typeof(T));
                var type = inst.GetType();

                foreach (var o in moc)
                {
                    var objCounter = 0;
                    foreach (var a in type.GetProperties())
                    {
                        objCounter++;
                      
                            if (o[a.Name] != null)
                            {
                                a.SetValue(inst, o[a.Name], null);

                                if (type.GetProperties().Length == objCounter)
                                {
                                    list.Add(inst);
                                    inst = (T)Activator.CreateInstance(typeof(T));
                                }
                            }
                      
                      
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
               
            }
          

            return list;
        }

        public void SetObjectCollection()
        {
            var mScope = new ManagementScope(_scope);
            var oQuery = new ObjectQuery(_query);

            try
            {
                using (var objectSearcher = new ManagementObjectSearcher(mScope, oQuery))
                {
                    objectSearcher.Options.BlockSize = 10;
                    objectSearcher.Options.ReturnImmediately = true;
                    objectSearcher.Options.Timeout = TimeSpan.FromSeconds(30);
                    moc = objectSearcher.Get();
                }
            }
            catch { }
        }
    }
}