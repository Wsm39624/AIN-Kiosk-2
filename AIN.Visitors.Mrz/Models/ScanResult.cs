using System;

namespace AIN.Visitors.Mrz.Models
{
    public class ScanResult : IDisposable
    {
        public bool IsSuccess { get; set; }
        public string NationalID { get; set; } = string.Empty;
        public string FullNameEnglish { get; set; } = string.Empty;
        public string FullNameArabic { get; set; } = string.Empty;
        public string RawMrzData { get; set; } = string.Empty;

        // تطبيق التخلص الآمن لحماية خصوصية البيانات وتطهير الـ RAM فوراً
        public void Dispose()
        {
            NationalID = string.Empty;
            FullNameEnglish = string.Empty;
            FullNameArabic = string.Empty;
            RawMrzData = string.Empty;
            GC.SuppressFinalize(this);
        }
    }
}