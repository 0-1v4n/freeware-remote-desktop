using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RemoteDesktopCore
{
    /// <summary>
    /// Handles image compression and encoding for efficient transmission
    /// </summary>
    public class ImageCompression
    {
        /// <summary>
        /// Compresses an image to JPEG format with specified quality
        /// </summary>
        public static byte[] CompressImageToJpeg(Bitmap bitmap, int quality = 75)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (quality < 1 || quality > 100)
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 1 and 100");

            using (MemoryStream ms = new MemoryStream())
            {
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                bitmap.Save(ms, jpegCodec, encoderParams);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Decompresses a JPEG image from byte array
        /// </summary>
        public static Bitmap DecompressImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data cannot be null or empty", nameof(imageData));

            using (MemoryStream ms = new MemoryStream(imageData))
            {
                return new Bitmap(ms);
            }
        }

        /// <summary>
        /// Gets the image codec info for a specified media type
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
