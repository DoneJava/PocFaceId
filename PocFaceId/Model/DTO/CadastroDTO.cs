using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Model.DTO
{
    public class CadastroDTO
    {
        public string cpf { get; set; }
        public string password { get; set; }
        public string img { get; set; }
        public string name { get; set; }
    }
}
