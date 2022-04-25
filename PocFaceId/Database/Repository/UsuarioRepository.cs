using Microsoft.EntityFrameworkCore;
using PocFaceId.Data;
using PocFaceId.Database.Interface;
using PocFaceId.Model;
using PocFaceId.Model.DTO;
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


        public Usuario buscarPessoaIdLogin(string login, string senha)
        {
            var usuario = _context.Usuario.Include(x => x.Pessoa).First(x => x.Login == login);
            if (usuario.Senha == senha) return usuario;
            else throw new Exception("A senha está incorreta.");
        }
        public string Logar(CadastroDTO cadastroDTO)
        {
            try
            {
                var usuarioLogado = _context.Usuario.FirstOrDefault(x => x.Login == cadastroDTO.cpf && x.Senha == cadastroDTO.password);
                var pessoaFoto = _context.Pessoa.FirstOrDefault(x => x.Id == usuarioLogado.PessoaId).Foto;
                if (usuarioLogado != null)
                {
                    return pessoaFoto;
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
               return "0";
            }
        }
        public bool CadastrarUsuário(CadastroDTO cadastro)
        {
            try
            {
                if (cadastro == null) 
                {
                    return false;
                }
                if (_context.Usuario.Any(x => x.Login == cadastro.cpf)) 
                {
                    return false;
                }             
                Usuario usuarioCadastrado = new Usuario()
                { 
                    Login = cadastro.cpf,
                    Senha = cadastro.password,
                    Pessoa = new Pessoa() { 
                        Nome = cadastro.name,
                        Foto = cadastro.img.Split(",").Last()
                    }
                };
                _context.Usuario.Add(usuarioCadastrado);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
