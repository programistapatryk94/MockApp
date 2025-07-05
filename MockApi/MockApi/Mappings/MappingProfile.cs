using AutoMapper;
using MockApi.Dtos.Mock;
using MockApi.Dtos.Project;
using MockApi.Models;

namespace MockApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Mock, MockDto>();
            CreateMap<CreateMockInput, Mock>();

            CreateMap<Project, ProjectDto>();
            CreateMap<CreateProjectInput, Project>();
        }
    }
}
