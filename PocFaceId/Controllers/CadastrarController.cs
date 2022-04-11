using Microsoft.AspNetCore.Mvc;
using PocFaceId.Data;
using PocFaceId.Database.Interface;
using PocFaceId.Database.Repository;
using PocFaceId.Model.DTO;
using PocFaceId.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Cors;

namespace PocFaceId.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CadastrarController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public CadastrarController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }
        [HttpPut]
        public async Task<string> Cadastrar(CadastroDTO cadastro)
        {
            return _usuarioRepository.CadastrarUsuário(cadastro);
        }
    }
}
