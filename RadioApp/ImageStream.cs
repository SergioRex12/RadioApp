using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RadioApp
{
    internal class ImageStream
    {

        public Image? LoadImageFromUrl(string imageUrl)
        {
            try
            {
                // Crear una solicitud web para obtener la imagen
                WebRequest request = WebRequest.Create(imageUrl);
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    // Cargar la imagen desde el stream
                    Image image = Image.FromStream(stream);

                    return image;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la imagen: " + ex.Message);
                return null;
            }
        }
    }
}
