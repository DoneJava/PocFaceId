using PocFaceId.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Database.Interface
{
    public interface IUsuarioRepository
    {
        Usuario buscarPessoaIdLogin(string login, string senha);
    }
}
