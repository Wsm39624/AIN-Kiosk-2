using QRCoder;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace AIN_Kiosk.Services
{
    public class LocalQrCodeService
    {
        public BitmapImage GenerateQrCodeImage(string content)
        {
            // 🔒 توليد الـ QR محلياً بالكامل بداخل الذاكرة العشوائية لحظر أي تسريب شبكي
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

                var bitmapImage = new BitmapImage();
                using (var stream = new MemoryStream(qrCodeAsPngByteArr))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }

                bitmapImage.Freeze(); // تجميد الصورة في الذاكرة لتسريع الأداء

                // 🔒 الإجراء الأمني المطلوب: مسح مصفوفة البايتات فوراً من الـ RAM لحماية الخصوصية
                Array.Clear(qrCodeAsPngByteArr, 0, qrCodeAsPngByteArr.Length);

                return bitmapImage;
            }
        }
    }
}