using PocFaceId.Data;
using PocFaceId.Database.Interface;
using PocFaceId.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Database.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly PocFaceIdContext _context;

        public UsuarioRepository(PocFaceIdContext context)
        {
            _context = context;
        }

        public Usuario buscarPessoaIdLogin (string login, string senha)
        {
            try
            {
                var usuario = _context.Usuario.FirstOrDefault();
                if(usuario.Senha == senha)
                {
                    return usuario;
                }
                else
                {
                    throw new Exception("A senha está incorreta.");
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        } 

    }
}
