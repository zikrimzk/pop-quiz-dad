using com.google.zxing.common;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Printing;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.SkiaSharp;
using ZXing.Windows.Compatibility;
namespace PopQuizApi.Services
{
    public class QrService
    {
        public Image<Rgba32> GenerateQRCode(string data)
        {
            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    Margin = 2
                }
            };

            var pixelData = qrCodeWriter.Write(data);

            var image = new Image<Rgba32>(pixelData.Width, pixelData.Height);

            // Copy raw pixels into ImageSharp
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < pixelData.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < pixelData.Width; x++)
                    {
                        int i = (y * pixelData.Width + x) * 4;
                        byte r = pixelData.Pixels[i + 2]; // BGR → RGB
                        byte g = pixelData.Pixels[i + 1];
                        byte b = pixelData.Pixels[i];
                        row[x] = new Rgba32(r, g, b);
                    }
                }
            });

            return image;

        }
    }
}
