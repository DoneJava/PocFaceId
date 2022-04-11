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
            var usuario = _context.Usuario.First(x => x.Login == login);
            if (usuario.Senha == senha) return usuario;
            else throw new Exception("A senha está incorreta.");
        }
        public bool Logar(CadastroDTO cadastroDTO)
        {
            try
            {
                var usuarioLogado = _context.Usuario.FirstOrDefault(x => x.Login == cadastroDTO.cpf && x.Senha == cadastroDTO.password);
                if (usuarioLogado != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public string CadastrarUsuário(CadastroDTO cadastro)
        {
            try
            {
                Usuario usuarioCadastrado = new Usuario();
                Pessoa pessoaCadastrada = new Pessoa();
                if (cadastro == null)
                    return "Cadastro inclompleto.";
                if (_context.Usuario.Any(x => x.Login == cadastro.cpf))
                    return "Esse cpf já está cadastrado!";
                pessoaCadastrada.Nome = cadastro.name;
                pessoaCadastrada.Foto = cadastro.img;
                var teste = _context.Add(pessoaCadastrada);
                _context.SaveChanges();
                var pessoa = _context.Pessoa.FirstOrDefault(x => x.Foto == cadastro.img && x.Nome == cadastro.name);
                usuarioCadastrado.Login = cadastro.cpf;
                usuarioCadastrado.Senha = cadastro.password;
                usuarioCadastrado.PessoaId = pessoa.Id;
                _context.Usuario.Add(usuarioCadastrado);
                _context.SaveChanges();
                return "Cadastrado com sucesso!";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
