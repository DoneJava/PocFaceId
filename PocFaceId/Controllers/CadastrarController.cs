using Microsoft.AspNetCore.Mvc;
using PocFaceId.Database.Interface;
using PocFaceId.Model.DTO;
using System.Threading.Tasks;

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
