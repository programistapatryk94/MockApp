using System.Transactions;

namespace MockApi.Runtime.DataModels.UoW
{
    public class UnitOfWorkConfiguration : IUnitOfWorkConfiguration
    {
        public TransactionScopeOption Scope { get; set; }
        public TimeSpan? Timeout { get; set; }
        public IsolationLevel? IsolationLevel { get; set; }

        public UnitOfWorkConfiguration()
        {
            Scope = TransactionScopeOption.Required;
        }
    }
}
