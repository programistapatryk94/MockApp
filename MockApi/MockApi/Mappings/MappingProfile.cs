using AutoMapper;
using MockApi.Dtos.Mock;
using MockApi.Dtos.Project;
using MockApi.Dtos.ProjectMember;
using MockApi.Dtos.SubscriptionPlan;
using MockApi.Models;
using MockApi.Services.Models;

namespace MockApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Mock, MockDto>();
            CreateMap<CreateOrUpdateMockInput, Mock>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.MembersEnabled, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    var currentUserId = (Guid?)context.Items["CurrentUserId"];

                    var isCreator = currentUserId == src.CreatorUserId;
                    var creatorEnabled = src.CreatorUser?.IsCollaborationEnabled ?? false;

                    return isCreator && creatorEnabled;
                }));
            CreateMap<CreateOrUpdateProjectInput, Project>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, ProjectMemberDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<SubscriptionPlan, SubscriptionPlanDto>();
            CreateMap<CurrentSubscriptionInfo, SubscriptionPlanDto>();
            CreateMap<SubscriptionPlanPrice, SubscriptionPlanPriceDto>();
            CreateMap<CurrentSubscriptionPlanPriceInfo, SubscriptionPlanPriceDto>();
        }
    }
}
