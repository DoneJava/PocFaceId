using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using PocFaceId.Database.Interface;
using PocFaceId.Model.DTO;
using PocFaceId.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PocFaceId.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        const string SUBSCRIPTION_KEY = "726b5b918c3f4b638988a083dda31985";
        const string ENDPOINT = "https://eafcgfaceid01.cognitiveservices.azure.com/";
        private readonly IUsuarioRepository _usuarioRepository;
        private IMapper _mapper;
        IFaceClient _client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
        string _recognitionModel03 = RecognitionModel.Recognition04;
        public FaceController(IUsuarioRepository usuarioRepository, IMapper mapper)
        {
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        [HttpPost]
        public async Task<dynamic> ValidarUsuario([FromBody] CadastroDTO cadastroDTO)
        {
            RespostaApiValidarDTO resposta = new RespostaApiValidarDTO();
            cadastroDTO.img = cadastroDTO.img.Split(',').Last();
            var usuario = _usuarioRepository.buscarPessoaIdLogin(cadastroDTO.cpf, cadastroDTO.password);
            if (usuario == null)
            {
                resposta.Validador = false;
                resposta.MensagemRsposta = "Não foi possível encontrar o usuário.";
                return  resposta;
            }

            List<DetectedFace> faceReferencia = await DetectFaceRecognize(_client, cadastroDTO.img, _recognitionModel03);
            if (faceReferencia.Count == 0)
            {
                resposta.Validador = false;
                resposta.MensagemRsposta = "Nenhum rosto reconhecido.";
                return resposta;
            }
                 
            if (faceReferencia.Count > 1)
            {
                resposta.Validador = false;
                resposta.MensagemRsposta = "Por favor, fique somente 1(uma) pessoa em frente a câmera.";
                return resposta;
            }
                 
            Guid faceReferenciaIdentificada = faceReferencia[0].FaceId.Value;
            List<DetectedFace> faceComparacao = await DetectFaceRecognize(_client, usuario.Pessoa.Foto, _recognitionModel03);
            Guid faceComparacaoIdentificada = faceComparacao[0].FaceId.Value;
            VerifyResult resultadoVerificacao = await _client.Face.VerifyFaceToFaceAsync(faceComparacaoIdentificada, faceReferenciaIdentificada);
            double valorMínConfianca = 0.8;
            if (resultadoVerificacao.Confidence >= valorMínConfianca)
            {
                resposta.Validador = true;
                resposta.MensagemRsposta = $"Bem vindo(a) a validação da Prova de Conceito sr(a), {usuario.Pessoa.Nome}";
                return resposta;
            }
            else
            {
                resposta.Validador = true;
                resposta.MensagemRsposta = $"A pessoa que está em frente a câmera não é o(a) sr(a), {usuario.Pessoa.Nome}";
                return resposta;
            }                
        }

        [HttpPut]
        public async Task<dynamic> Login([FromBody] CadastroDTO cadastroDTO)
        {
            RespostaApiLoginDTO resposta = new RespostaApiLoginDTO();
            var aux = _usuarioRepository.Logar(cadastroDTO);
            if (aux == "0")
            {
                return Unauthorized();
            }
            else
            {
                resposta.MensagemResposta = aux;
                resposta.Validador = true;
                return resposta;
            }
        }

        private async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string image64, string recognition_model)
        {
            byte[] imageBytes = Convert.FromBase64String(image64);
            MemoryStream streamImage = new MemoryStream(imageBytes);
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(streamImage, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03, returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.QualityForRecognition });
            List<DetectedFace> sufficientQualityFaces = new List<DetectedFace>();
            foreach (DetectedFace detectedFace in detectedFaces)
            {
                var faceQualityForRecognition = detectedFace.FaceAttributes.QualityForRecognition;
                if (faceQualityForRecognition.HasValue && (faceQualityForRecognition.Value >= QualityForRecognition.Medium))
                    sufficientQualityFaces.Add(detectedFace);
            }
            return sufficientQualityFaces.ToList();
        }
        [HttpPut]
        [Route("Cadastrar")]
        public async Task<dynamic> Cadastrar(CadastroDTO cadastro)
        {

            var aux = _usuarioRepository.CadastrarUsuário(cadastro);
            if (aux == true)
            {
                return Ok(new
                {
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
        [HttpPut]
        [Route("VerificarImg")]
        public async Task<RespostaApiDTO> VerificarImg(string img)
        {
            RespostaApiDTO resposta = new RespostaApiDTO();
            List<DetectedFace> temRosto = await DetectFaceRecognize(_client, img, _recognitionModel03);
            if (temRosto.Count() == (int)QtdRostos.UmRosto)
            {
                resposta.MensagemRetorno = "Sucesso!";
                resposta.Validador = (int)QtdRostos.UmRosto;
                return resposta;
            }
            else if (temRosto.Count() == (int)QtdRostos.NenhumRosto)
            {
                resposta.MensagemRetorno = "Nenhum rosto encontrado. Por favor, tire outra foto.";
                resposta.Validador = (int)QtdRostos.NenhumRosto;
                return resposta;
            }
            else
            {
                resposta.MensagemRetorno = "Foram encontrados mais de um rosto na imagem. Por favor, tire outra foto.";
                resposta.Validador = (int)QtdRostos.MaisDeUmRosto;
                return resposta;
            }
        }
    }
}
