using System.Text;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Service
{
    // Comprueba que los primeros bytes del archivo correspondan a su extensión
    public static class FirmasArchivo
    {
        public static async Task<bool> CoincideConExtensionAsync( IFormFile archivo, string extension)
        {
            var c = new byte[12];
            int leidos;

            using (var stream = archivo.OpenReadStream())
            {
                leidos = await stream.ReadAtLeastAsync(
                    c, c.Length, throwOnEndOfStream: false);
            }

            string Ascii(int desde, int largo) =>
                Encoding.ASCII.GetString(c, desde, largo);

            return extension switch
            {
                ".jpg" or ".jpeg" =>
                    leidos >= 3 && c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF,

                ".png" =>
                    leidos >= 8 &&
                    c.AsSpan(0, 8).SequenceEqual(
                        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),

                ".gif" =>
                    leidos >= 6 &&
                    (Ascii(0, 6) == "GIF87a" || Ascii(0, 6) == "GIF89a"),

                ".webp" =>
                    leidos >= 12 && Ascii(0, 4) == "RIFF" && Ascii(8, 4) == "WEBP",

                ".mp4" =>
                    leidos >= 12 && Ascii(4, 4) == "ftyp",

                ".webm" =>
                    leidos >= 4 &&
                    c[0] == 0x1A && c[1] == 0x45 && c[2] == 0xDF && c[3] == 0xA3,

                _ => false
            };
        }
    }
}