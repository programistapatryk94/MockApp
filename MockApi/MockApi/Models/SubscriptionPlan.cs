using MockApi.Runtime.DataModels;

namespace MockApi.Models
{
    public class SubscriptionPlan : Entity
    {
        public static class DefaultNames
        {
            public const string DefaultPlan = "DefaultPlan";
        }

        public string Name { get; set; } = default!;
        public int MaxProjects { get; set; }
        public int MaxResources { get; set; }
        public bool HasCustomResponse { get; set; }
        public bool HasCollaboration { get; set; }

        public ICollection<SubscriptionPlanPrice> Prices { get; set; } = new List<SubscriptionPlanPrice>();
    }
}
