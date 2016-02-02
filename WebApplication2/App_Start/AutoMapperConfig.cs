using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.DBEntities;
using WebApplication2.Models;

namespace WebApplication2.App_Start
{
    public static class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            AutoMapper.Mapper.CreateMap<Tutor, TutorRegisterModel>()
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DOB, opts => opts.MapFrom(src => src.DateOfBirth));

            AutoMapper.Mapper.CreateMap<TutorRegisterModel, Tutor>()
               .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.FirstName))
               .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.LastName))
               .ForMember(dest => dest.DateOfBirth, opts => opts.MapFrom(src => src.DOB));

            AutoMapper.Mapper.CreateMap<Tutor, TutorUpdateModel>()
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DOB, opts => opts.MapFrom(src => src.DateOfBirth));

            AutoMapper.Mapper.CreateMap<TutorUpdateModel, Tutor>()
               .ForMember(dest => dest.FirstName, opts => opts.MapFrom(src => src.FirstName))
               .ForMember(dest => dest.LastName, opts => opts.MapFrom(src => src.LastName))
               .ForMember(dest => dest.DateOfBirth, opts => opts.MapFrom(src => src.DOB));



            AutoMapper.Mapper.CreateMap<Student, StudentRegisterModel>();
            AutoMapper.Mapper.CreateMap<StudentRegisterModel, Student>();


            AutoMapper.Mapper.CreateMap<Student, StudentUpdateModel>();
            AutoMapper.Mapper.CreateMap<StudentUpdateModel, Student>();

            AutoMapper.Mapper.CreateMap<QuestionViewModel, Question>();
            AutoMapper.Mapper.CreateMap<Question, QuestionViewModel>();
        }

    }
}