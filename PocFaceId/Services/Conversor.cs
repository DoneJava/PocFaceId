using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocFaceId.Services
{
    public class Conversor
    {
        public string ConvertImgToBase64(string path)
        {
            try
            {
                byte[] imageArray = System.IO.File.ReadAllBytes(path);
                var base64ImageRepresentation = Convert.ToBase64String(imageArray);
                return base64ImageRepresentation;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
