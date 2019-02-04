using System;
using System.Linq;
using Toec_Common.Dto;
using Toec_Common.Entity;
using Toec_DataModel;

namespace Toec_Services.Entity
{
    public class PolicyHistoryServices : IDisposable
    {
        private readonly UnitOfWork _uow;

        public PolicyHistoryServices()
        {
            _uow = new UnitOfWork();
        }

        public DtoActionResult AddHistory(EntityPolicyHistory history)
        {
            var actionResult = new DtoActionResult();
            _uow.PolicyHistoryRepository.Insert(history);
            _uow.Save();
            actionResult.Success = true;
            actionResult.Id = history.Id;
            return actionResult;
        }

        public bool DeleteAll()
        {
            _uow.PolicyHistoryRepository.DeleteRange(x => x.Id > 0);
            _uow.Save();
            return true;
        }

        public EntityPolicyHistory GetLastPolicyRunForUserFromGuid(string policyGuid, string user)
        {
            return
                _uow.PolicyHistoryRepository.Get(x => x.PolicyGUID.Equals(policyGuid) && x.Username.Equals(user),
                    q => q.OrderByDescending(t => t.Id)).FirstOrDefault();
        }

        //Gets the last instance that a specific policy ran.  Not the last policy to run.
        public EntityPolicyHistory GetLastPolicyRunForUserFromHash(string policyHash, string user)
        {
            return
                _uow.PolicyHistoryRepository.Get(x => x.PolicyHash.Equals(policyHash) && x.Username.Equals(user),
                    q => q.OrderByDescending(t => t.Id)).FirstOrDefault();
        }

        public EntityPolicyHistory GetLastPolicyRunFromGuid(string policyGuid)
        {
            return
                _uow.PolicyHistoryRepository.Get(x => x.PolicyGUID.Equals(policyGuid), q => q.OrderByDescending(t => t.Id))
                    .FirstOrDefault();
        }

        //Gets the last instance that a specific policy ran.  Not the last policy to run.
        public EntityPolicyHistory GetLastPolicyRunFromHash(string policyHash)
        {
            return
                _uow.PolicyHistoryRepository.Get(x => x.PolicyHash.Equals(policyHash), q => q.OrderByDescending(t => t.Id))
                    .FirstOrDefault();
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