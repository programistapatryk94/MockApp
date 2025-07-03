using AutoMapper;
using MockApi.Dtos;
using MockApi.Models;

namespace MockApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Mock, MockDto>();
            CreateMap<CreateMockInput, Mock>();
        }
    }
}
