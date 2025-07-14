namespace MockApi.Runtime.DataModels.Auditing
{
    public interface ICreatedAudited : IHasCreatedAt
    {
        Guid? CreatorUserId { get; set; }
    }

    public interface ICreatedAudited<TUser> : ICreatedAudited
        where TUser : IEntity<Guid>
    {
        TUser CreatorUser { get; set; }
    }
}
