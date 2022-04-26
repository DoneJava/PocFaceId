using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Model.DTO
{
    public class RespostaApiLoginDTO
    {
        public bool Validador { get; set; }
        public string MensagemResposta { get; set; }
    }
}
