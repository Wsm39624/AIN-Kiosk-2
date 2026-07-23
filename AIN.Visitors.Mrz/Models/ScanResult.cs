using System;

namespace AIN.Visitors.Mrz.Models
{
    /// <summary>
    /// عقد نتيجة المسح العادية (المشارك مع وسام ومحمد).
    /// </summary>
    public class ScanResult : IDisposable
    {
        public NormalizedDocumentData Data { get; set; } = new NormalizedDocumentData();
        public ValidationEvidence Evidence { get; set; } = new ValidationEvidence();
        public bool IsSuccess => Evidence.ValidationStatus == "Valid" || Evidence.ValidationStatus == "Warning";

        // خصائص تباين وتوافقية لتسهيل ربط Backend الخاص بمحمد
        public string FullNameEnglish => $"{Data.GivenNames} {Data.Surname}".Trim();
        public string? FullNameArabic { get; set; } = null;
        public string NationalID => Data.DocumentNumber;

        // مراجع البيانات الخام (توجيه آمن حسب المعيار)
        private RawMrzArtifact? _rawArtifact;

        public RawMrzArtifact? RawData 
        { 
            get => _rawArtifact; 
            set => _rawArtifact = value; 
        }

        public string RawMrzData => _rawArtifact != null ? new string(_rawArtifact.GetRawDataSpan().ToArray()) : string.Empty;

        public void Dispose()
        {
            _rawArtifact?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}