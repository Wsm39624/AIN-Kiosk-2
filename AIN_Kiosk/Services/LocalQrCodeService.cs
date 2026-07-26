using System;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace AIN_Kiosk;

public sealed class LocalQrCodeService
{
    public BitmapImage GenerateBitmap(string payload, int pixelsPerModule = 12)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        // Generate raw PNG bytes locally without external service dependency
        byte[] pngBytes = PngByteQRCodeHelper.GetQRCode(
            payload,
            QRCodeGenerator.ECCLevel.Q,
            pixelsPerModule);

        using MemoryStream stream = new(pngBytes);

        BitmapImage image = new();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();

        // Clear sensitive buffer from memory
        Array.Clear(pngBytes, 0, pngBytes.Length);

        return image;
    }

    // Backward compatibility wrapper for existing tests
    public BitmapImage GenerateQrCodeImage(string payload, int pixelsPerModule = 12)
    {
        return GenerateBitmap(payload, pixelsPerModule);
    }
}