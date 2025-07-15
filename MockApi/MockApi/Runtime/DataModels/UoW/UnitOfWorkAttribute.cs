using System.Transactions;

namespace MockApi.Runtime.DataModels.UoW
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class UnitOfWorkAttribute : Attribute
    {
        public TransactionScopeOption? Scope { get; set; }

        public TimeSpan? Timeout { get; set; }

        public IsolationLevel? IsolationLevel { get; set; }
    }
}
