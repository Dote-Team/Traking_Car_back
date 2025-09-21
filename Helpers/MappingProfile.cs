using AutoMapper;
using TrakingCar.Models;
using TrakingCar.Dto.attachment;
using TrakingCar.Dto.location;
using TrakingCar.Dto.ownership;
using TrackingCar.Dto.user;
using TrakingCar.Data;
using TrakingCar.Dto.Car;
using TrakingCar.Dtos;

namespace Tracking.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 🧑‍💻 User mappings
            CreateMap<User, UserReadDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserUpdateDto>().ReverseMap();
            CreateMap<User, UpdateUserStatusDto>().ReverseMap();
            CreateMap<User, LoginResponseDto>().ReverseMap();

            // 🚗 Car mappings
            CreateMap<Car, CarDto>().ReverseMap();
            CreateMap<Car, CreateCarDto>().ReverseMap();
            CreateMap<Car, UpdateCarDto>().ReverseMap();

            // 📎 Attachment mappings
            CreateMap<Attachment, AttachmentDetailsDto>().ReverseMap();
            CreateMap<Attachment, CreateAttachmentDto>().ReverseMap();
            CreateMap<Attachment, UpdateAttachmentDto>().ReverseMap();

            // 📍 Location mappings
            CreateMap<CreateLocationDto, Location>().ReverseMap();
            CreateMap<UpdateLocationDto, Location>().ReverseMap();
            CreateMap<LocationDetailsDto, Location>().ReverseMap();


            // 🏢 Ownership mappings
            CreateMap<Ownership, OwnershipDetailsDto>().ReverseMap();
            CreateMap<Ownership, CreateOwnershipDto>().ReverseMap();
            CreateMap<Ownership, UpdateOwnershipDto>().ReverseMap();
        }
    }
}
