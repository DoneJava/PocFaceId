using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Model.DTO
{
    public class CadastroDTO
    {
        public string cpf { get; set; }
        public string Senha { get; set; }
        public string Foto { get; set; }
        public string Nome { get; set; }
    }
}
