using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Programowanie_Projekt_Web.Model
{
    public static class Utility
    {
        // Konwersja IFormFile (upload z formularza) na tablicę bajtów
        public static async Task<byte[]> ImageToByteArrayAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Array.Empty<byte>();

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        // Konwersja tablicy bajtów na string base64 do <img src="data:image/png;base64,..." />
        public static string ByteArrayToBase64Image(byte[] byteArray, string mimeType = "image/png")
        {
            if (byteArray == null || byteArray.Length == 0)
                return string.Empty;

            string base64 = Convert.ToBase64String(byteArray);
            return $"data:{mimeType};base64,{base64}";
        }
    }
}
