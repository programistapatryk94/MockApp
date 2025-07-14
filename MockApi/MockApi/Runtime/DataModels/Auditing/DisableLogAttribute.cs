namespace MockApi.Runtime.DataModels.Auditing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class DisableLogAttribute : Attribute
    {
    }
}
