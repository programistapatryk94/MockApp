namespace MockApi.Runtime.DataModels.Auditing
{
    public interface IAudited : ICreatedAudited, IModifiedAudited
    {

    }

    public interface IAudited<TUser> : IAudited, ICreatedAudited<TUser>, IModifiedAudited<TUser>
        where TUser : IEntity<Guid>
    {

    }
}
