using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Domain;
using WebApplicationAPI.Dto;

namespace WebApplicationAPI.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserLoginDto>().ReverseMap();

        }
    }
}
