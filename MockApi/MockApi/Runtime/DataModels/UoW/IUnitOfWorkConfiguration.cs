using System.Transactions;

namespace MockApi.Runtime.DataModels.UoW
{
    public interface IUnitOfWorkConfiguration
    {
        TransactionScopeOption Scope { get; set; }
        TimeSpan? Timeout { get; set; }
        IsolationLevel? IsolationLevel { get; set; }
    }
}
