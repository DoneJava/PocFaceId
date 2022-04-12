﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly IUsuarioRepository _usuarioRepository;
        public FaceController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        [HttpPost]
        public async Task<string> ValidarUsuario([FromBody] CadastroDTO cadastroDTO)
        {
            cadastroDTO.img = cadastroDTO.img.Split(',').Last();
            var usuario = _usuarioRepository.buscarPessoaIdLogin(cadastroDTO.cpf, cadastroDTO.password);
            if (usuario == null)
               return "Não foi possível encontrar o usuário.";
            string recognitionModel03 = RecognitionModel.Recognition04;
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            List<DetectedFace> faceReferencia = await DetectFaceRecognize(client, cadastroDTO.img, recognitionModel03);
            if (faceReferencia.Count == 0)
                return "Nenhum rosto reconhecido.";
            if (faceReferencia.Count > 1)
                return  "Por favor, fique somente 1(uma) pessoa em frente a câmera.";
            Guid faceReferenciaIdentificada = faceReferencia[0].FaceId.Value;
            List<DetectedFace> faceComparacao = await DetectFaceRecognize(client, usuario.Pessoa.Foto, recognitionModel03);
            Guid faceComparacaoIdentificada = faceComparacao[0].FaceId.Value;
            VerifyResult resultadoVerificacao = await client.Face.VerifyFaceToFaceAsync(faceComparacaoIdentificada, faceReferenciaIdentificada);
            double valorMínConfianca = 0.8;
            if (resultadoVerificacao.Confidence >= valorMínConfianca)
                return $"Bem vindo(a) a validação da Prova de Conceito sr(a), {usuario.Pessoa.Nome}";
            else
                return  $"A pessoa que está em frente a câmera não é o(a) sr(a), {usuario.Pessoa.Nome}";
        }

        [HttpPut]
        public async Task<string> Login([FromBody] CadastroDTO cadastroDTO)
        {
            var aux = _usuarioRepository.Logar(cadastroDTO);
            if (aux == "0")
            {
                throw new Exception("Usuário ou senha inválido.");
            }
            else
            {
                return aux;
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
    }
}
