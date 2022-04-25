using AutoMapper;
using PocFaceId.Model;
using PocFaceId.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Profiles
{
    public class LoginProfile : Profile
    {
        public LoginProfile()
        {
            CreateMap<CadastroDTO, Usuario>();
            CreateMap<CadastroDTO, Pessoa>();
        }
    }
}
