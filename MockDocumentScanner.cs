using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIN_Kiosk
{
    // محاكي القارئ الرسمي المتوافق مع مواصفات الأسبوع الأول
    public sealed class MockDocumentScanner : IDocumentScanner
    {
        public async Task<ScanResult> ScanAsync(CancellationToken cancellationToken)
        {
            // محاكاة وقت المعالجة الضوئية الفعلي (3 ثوانٍ)
            await Task.Delay(3000, cancellationToken);

            // إرجاع النتيجة المصممة مسبقاً (Canned Scan) بدون تسريب بيانات حساسة للذاكرة
            return new ScanResult
            {
                IsSuccess = true,
                Metadata = "Document Type: National ID | Issuer: KSA | Verification: Verified",
                RawPayload = System.Text.Encoding.UTF8.GetBytes("MOCKED_SECURE_MRZ_DATA_PAYLOAD")
            };
        }
    }
}