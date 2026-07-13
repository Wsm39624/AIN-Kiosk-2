using System;
using System.Threading;
using System.Threading.Tasks;
using AIN.Visitors.Mrz.Abstractions;
using AIN.Visitors.Mrz.Models;

namespace AIN.Visitors.Mrz.Scanners
{
    public class MockDocumentScanner : IDocumentScanner
    {
        public async Task<ScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[MockScanner] Hardware is scanning document...");
            
            // محاكاة وقت القراءة
            await Task.Delay(2000, cancellationToken);
            
            Console.WriteLine("[MockScanner] Scan completed successfully.");

            // إرجاع البيانات المطابقة تماماً لما تتوقعه واجهات وسام
            return new ScanResult
            {
                IsSuccess = true,
                NationalID = "L898902C3",
                FullNameEnglish = "ERIKSSON ANNA MARIA",
                FullNameArabic = "آنا ماريا إريكسون", // بيانات تجريبية لدعم الواجهة العربية
                RawMrzData = "P<UTOERIKSSON<<ANNA<MARIA<<<<<<<<<<<<<<<<<<<\nL898902C36UTO7408122F1204159ZE184226B<<<<<10"
            };
        }
    }
}