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
            try
            {
                var usuario = _context.Usuario.FirstOrDefault(x => x.Login == login);
                if (usuario.Senha == senha)
                {
                    return usuario;
                }
                else
                {
                    throw new Exception("A senha está incorreta.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string CadastrarUsuário(CadastroDTO cadastro)
        {
            try
            {
                Usuario usuarioCadastrado = new Usuario();
                Pessoa pessoaCadastrada = new Pessoa();
                if (cadastro != null)
                {
                    return "Cadastro inclompleto.";
                }
                var listaPessoas = _context.Usuario.Select(x => x.Login);
                if (listaPessoas.Contains(cadastro.cpf))
                {
                    return "Esse cpf já está cadastrado!";
                }
                pessoaCadastrada.Nome = cadastro.Nome;
                pessoaCadastrada.Foto = cadastro.Foto;
                var teste = _context.Add(pessoaCadastrada);
                _context.SaveChanges();
                var pessoa = _context.Pessoa.FirstOrDefault(x => x.Foto == cadastro.Foto && x.Nome == cadastro.Nome);
                usuarioCadastrado.Login = cadastro.cpf;
                usuarioCadastrado.Senha = cadastro.Senha;
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
