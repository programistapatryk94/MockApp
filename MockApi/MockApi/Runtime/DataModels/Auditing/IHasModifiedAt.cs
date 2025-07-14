namespace MockApi.Runtime.DataModels.Auditing
{
    public interface IHasModifiedAt
    {
        DateTime? ModifiedAt { get; set; }
    }
}
