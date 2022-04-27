using PocFaceId.Model;
using PocFaceId.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Database.Interface
{
    public interface IUsuarioRepository
    {
        Usuario buscarPessoaIdLogin(string img, string senha);
        bool CadastrarUsuário(CadastroDTO cadastro);
        CadastroDTO Logar(CadastroDTO cadastroDTO);
    }
}
