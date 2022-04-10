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
using System.Threading.Tasks;

namespace PocFaceId.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        const string SUBSCRIPTION_KEY = "726b5b918c3f4b638988a083dda31985";
        const string ENDPOINT = "https://eafcgfaceid01.cognitiveservices.azure.com/";
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        public FaceController(IPessoaRepository pessoaRepository, IUsuarioRepository usuarioRepository)
        {
            _pessoaRepository = pessoaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        [HttpPost]
        public async Task<string> Login([FromBody]string cpf, string senha, string fotoAtual)
        {
            try
            {
                var usuario = _usuarioRepository.buscarPessoaIdLogin(cpf, senha);
                if (usuario != null)
                {
                    var pessoa = _pessoaRepository.BuscarPessoa(usuario.PessoaId);  
                    string recognitionModel03 = RecognitionModel.Recognition04;
                    IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);     
                    List<DetectedFace> faceReferencia = await DetectFaceRecognize(client, fotoAtual, recognitionModel03);
                    if (faceReferencia.Count > 1)
                    {
                        return "Por favor, fique somente 1 pessoa em frente a câmera.";
                    }
                    Guid faceReferenciaIdentificada = faceReferencia[0].FaceId.Value;
                    List<DetectedFace> faceComparacao = await DetectFaceRecognize(client, pessoa.Foto, recognitionModel03);
                    Guid faceComparacaoIdentificada = faceComparacao[0].FaceId.Value;
                    VerifyResult resultadoVerificacao = await client.Face.VerifyFaceToFaceAsync(faceComparacaoIdentificada, faceReferenciaIdentificada);
                    double valorMínConfianca = 0.8;
                    if (resultadoVerificacao.Confidence >= valorMínConfianca)
                    {
                        return $"Bem vindo a validação da Prova de Conceito sr(a), {pessoa.Nome}";
                    }
                    else
                    {
                        return $"A pessoa que está em frente a câmera não é o(a) sr(a), {pessoa.Nome}";
                    }                  
                }
                else
                {
                    throw new Exception("Não foi possível encontrar o usuário.");
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        [HttpPut]
        public async Task<bool> Cadastrar([FromBody]CadastroDTO cadastro)
        {
            if (_usuarioRepository.CadastrarUsuário(cadastro))
            {
                return true;
            }
            else
            {
                return false;
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
                {
                    sufficientQualityFaces.Add(detectedFace);
                }
            }
            return sufficientQualityFaces.ToList();
        }
    }
}
