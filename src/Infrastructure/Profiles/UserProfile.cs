using AutoMapper;
using Domain.Models;
using Infrastructure.DTOs.Response;

namespace Infrastructure.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponse>();
        }
    }
}