namespace MockApi.Runtime.DataModels.Auditing
{
    public interface IAuditSerializer
    {
        string Serialize(object obj);
    }
}
