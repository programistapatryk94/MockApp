namespace MockApi.Runtime.DataModels
{
    public interface IEntity : IEntity<Guid>
    {

    }

    public interface IEntity<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
        bool IsTransient();
    }
}
