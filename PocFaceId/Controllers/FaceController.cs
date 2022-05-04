using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using PocFaceId.Database.Interface;
using PocFaceId.Model.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        const string SUBSCRIPTION_KEY = "";
        const string ENDPOINT = "";
        private readonly IUsuarioRepository _usuarioRepository;
        IFaceClient _client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
        string _recognitionModel03 = RecognitionModel.Recognition04;
        public FaceController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }
        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
        [HttpPost]
        [Route("Validar")]
        public async Task<dynamic> ValidarUsuario([FromBody] CadastroDTO cadastroDTO)
        {
            try
            {
                RespostaApiValidarDTO resposta = new RespostaApiValidarDTO();

                cadastroDTO.img = cadastroDTO.img.Split(',').Last();
                cadastroDTO.img2 = cadastroDTO.img2.Split(',').Last();

                List<DetectedFace> faceReferencia = await DetectFaceRecognize(_client, cadastroDTO.img, _recognitionModel03);
                List<DetectedFace> faceReferencia2 = await DetectFaceRecognize(_client, cadastroDTO.img2, _recognitionModel03);
                if (faceReferencia.Count == 0)
                {
                    return Ok(new
                    {
                        MensagemResposta = "Nenhum rosto reconhecido.",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.NenhumUsuario
                    });
                }

                if (faceReferencia.Count > 1)
                {
                    return Ok(new
                    {
                        MensagemResposta = $"Por favor, fique somente uma pessoa em frente a câmera.",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.DoisUsuarios
                    });
                }
                if (faceReferencia[0].FaceAttributes.HeadPose.Pitch == faceReferencia2[0].FaceAttributes.HeadPose.Pitch ||
                    faceReferencia[0].FaceAttributes.HeadPose.Roll == faceReferencia2[0].FaceAttributes.HeadPose.Roll ||
                    faceReferencia[0].FaceAttributes.HeadPose.Yaw == faceReferencia2[0].FaceAttributes.HeadPose.Yaw)
                {
                    return Ok(new
                    {
                        MensagemResposta = $"Não é uma pessoa em frente a câmera.",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.NenhumUsuario
                    });
                }
                var usuario = _usuarioRepository.buscarPessoaIdLogin(cadastroDTO.cpf, cadastroDTO.password);
                if (usuario == null)
                {
                    return Ok(new
                    {
                        MensagemResposta = "Não foi possível encontrar o usuário.",
                        success = false,
                        StatusCode = 200
                    });
                }
                Guid faceReferenciaIdentificada = faceReferencia[0].FaceId.Value;
                List<DetectedFace> faceComparacao = await DetectFaceRecognize(_client, usuario.Pessoa.Foto, _recognitionModel03);
                Guid faceComparacaoIdentificada = faceComparacao[0].FaceId.Value;
                VerifyResult resultadoVerificacao = await _client.Face.VerifyFaceToFaceAsync(faceComparacaoIdentificada, faceReferenciaIdentificada);
                double valorMínConfianca = 0.8;
                if (resultadoVerificacao.Confidence >= valorMínConfianca)
                {
                    return Ok(new
                    {
                        MensagemResposta = $"Olá sr(a) {usuario.Pessoa.Nome}",
                        success = true,
                        StatusCode = 200,
                        StatusMensagem = Validador.UsuarioCorreto,
                        Confidence = resultadoVerificacao.Confidence
                    });
                }
                else
                {
                    return Ok(new
                    {
                        MensagemResposta = $"A pessoa que está em frente a câmera não é o(a) sr(a), {usuario.Pessoa.Nome}",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.UsuarioIncorreto,
                        Confidence = resultadoVerificacao.Confidence
                    });
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("ValidarAtivo")]
        public async Task<dynamic> ValidarUsuarioAtivo([FromBody] CadastroDTO cadastroDTO)
        {
            try
            {
                RespostaApiValidarDTO resposta = new RespostaApiValidarDTO();

                cadastroDTO.img = cadastroDTO.img.Split(',').Last();
                cadastroDTO.img2 = cadastroDTO.img2.Split(',').Last();
                List<DetectedFace> faceReferencia = await DetectFaceRecognize(_client, cadastroDTO.img, _recognitionModel03);
                List<DetectedFace> faceReferencia2 = await DetectFaceRecognize(_client, cadastroDTO.img2, _recognitionModel03);
                if (faceReferencia.Count == 0)
                {
                    return Ok(new
                    {
                        MensagemResposta = "Nenhum rosto reconhecido.",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.NenhumUsuario
                    });
                }

                if (faceReferencia.Count > 1)
                {
                    return Ok(new
                    {
                        MensagemResposta = $"Por favor, fique somente uma pessoa em frente a câmera.",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.DoisUsuarios
                    });
                }
                if (faceReferencia[0].FaceAttributes.Smile >= 0.8 && faceReferencia2[0].FaceAttributes.Smile <= 0.8)
                {
                    return Ok(new
                    {
                        MensagemResposta = $"Não é uma pessoa em frente a câmera.",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.NenhumUsuario
                    });
                }
                var usuario = _usuarioRepository.buscarPessoaIdLogin(cadastroDTO.cpf, cadastroDTO.password);
                if (usuario == null)
                {
                    return Ok(new
                    {
                        MensagemResposta = "Não foi possível encontrar o usuário.",
                        success = false,
                        StatusCode = 200
                    });
                }
                Guid faceReferenciaIdentificada = faceReferencia[0].FaceId.Value;
                List<DetectedFace> faceComparacao = await DetectFaceRecognize(_client, cadastroDTO.img2, _recognitionModel03);
                Guid faceComparacaoIdentificada = faceComparacao[0].FaceId.Value;
                VerifyResult resultadoVerificacao = await _client.Face.VerifyFaceToFaceAsync(faceComparacaoIdentificada, faceReferenciaIdentificada);
                double valorMínConfianca = 0.8;
                if (resultadoVerificacao.Confidence >= valorMínConfianca)
                {
                    return Ok(new
                    {
                        MensagemResposta = $"Olá sr(a) {usuario.Pessoa.Nome}",
                        success = true,
                        StatusCode = 200,
                        StatusMensagem = Validador.UsuarioCorreto,
                        Confidence = resultadoVerificacao.Confidence
                    });
                }
                else
                {
                    return Ok(new
                    {
                        MensagemResposta = $"A pessoa que está em frente a câmera não é o(a) sr(a), {usuario.Pessoa.Nome}",
                        success = false,
                        StatusCode = 200,
                        StatusMensagem = Validador.UsuarioIncorreto,
                        Confidence = resultadoVerificacao.Confidence
                    });
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPut]
        [Route("Login")]
        public async Task<dynamic> Login([FromBody] CadastroDTO cadastroDTO)
        {
            try
            {
                RespostaApiLoginDTO resposta = new RespostaApiLoginDTO();
                var aux = _usuarioRepository.Logar(cadastroDTO);
                if (string.IsNullOrEmpty(aux.img))
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok(new
                    {
                        MensagemResposta = aux.img,
                        success = true,
                        StatusCode = 200
                    });
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        private async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string image64, string recognition_model)
        {
            byte[] imageBytes = Convert.FromBase64String(image64);
            MemoryStream streamImage = new MemoryStream(imageBytes);
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(streamImage, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03, returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.QualityForRecognition, FaceAttributeType.HeadPose, FaceAttributeType.Smile });
            List<DetectedFace> sufficientQualityFaces = new List<DetectedFace>();
            foreach (DetectedFace detectedFace in detectedFaces)
            {
                var faceQualityForRecognition = detectedFace.FaceAttributes.QualityForRecognition;
                if (faceQualityForRecognition.HasValue && (faceQualityForRecognition.Value >= QualityForRecognition.Medium))
                    sufficientQualityFaces.Add(detectedFace);
            }
            return sufficientQualityFaces.ToList();
        }
        [HttpPut("Cadastrar")]
        public async Task<dynamic> Cadastrar(CadastroDTO cadastro)
        {

            var aux = _usuarioRepository.CadastrarUsuário(cadastro);
            if (aux == true)
            {
                return Ok(new
                {
                    MensagemResposta = "Cadastro feito com sucesso!",
                    success = true,
                    StatusCode = 200
                });
            }
            else
            {
                return Conflict();
            }
        }
        public enum QtdRostos
        {
            UmRosto = 1,
            NenhumRosto = 0,
            MaisDeUmRosto = 2
        }
        public enum Validador
        {
            NenhumUsuario = 1,
            DoisUsuarios = 2,
            UsuarioCorreto = 3,
            UsuarioIncorreto = 4
        }
        [HttpPut]
        [Route("VerificarImg")]
        public async Task<dynamic> VerificarImg(string img)
        {
            try
            {
                RespostaApiDTO resposta = new RespostaApiDTO();
                List<DetectedFace> temRosto = await DetectFaceRecognize(_client, img, _recognitionModel03);
                if (temRosto.Count() == (int)QtdRostos.UmRosto)
                {
                    return Ok(new
                    {
                        MensagemResposta = "Sucesso!",
                        Validador = (int)QtdRostos.UmRosto,
                        success = true,
                        StatusCode = 200
                    });
                }
                else if (temRosto.Count() == (int)QtdRostos.NenhumRosto)
                {
                    return Ok(new
                    {
                        MensagemResposta = "Nenhum rosto encontrado. Por favor, tire outra foto.",
                        Validador = (int)QtdRostos.NenhumRosto,
                        success = false,
                        StatusCode = 200
                    });
                }
                else
                {
                    return Ok(new
                    {
                        MensagemResposta = "Foram encontrados mais de um rosto na imagem. Por favor, tire outra foto.",
                        Validador = (int)QtdRostos.MaisDeUmRosto,
                        success = false,
                        StatusCode = 200
                    });
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
