namespace MockApi.Runtime.DataModels.Auditing
{
    public interface IModifiedAudited : IHasModifiedAt
    {
        Guid? LastModifierUserId { get; set; }
    }

    public interface IModifiedAudited<TUser> : IModifiedAudited
        where TUser : IEntity<Guid>
    {
        TUser LastModifierUser { get; set; }
    }
}
