using PocFaceId.Data;
using PocFaceId.Database.Interface;
using PocFaceId.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Database.Repository
{
    public class PessoaRepository : IPessoaRepository
    {
        private readonly PocFaceIdContext _context;
        public PessoaRepository(PocFaceIdContext context)
        {
            _context = context;
        }

        public Pessoa BuscarPessoa(int pessoaId)
        {
            try
            {
            return _context.Pessoa.FirstOrDefault(x => x.Id == pessoaId);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
