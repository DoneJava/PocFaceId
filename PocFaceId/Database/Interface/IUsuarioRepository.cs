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
        Usuario buscarPessoaIdLogin(string login, string senha);
        string CadastrarUsuário(CadastroDTO cadastro);
        bool Logar(CadastroDTO cadastroDTO);
    }
}
