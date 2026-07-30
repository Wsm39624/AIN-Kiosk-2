using System;

namespace AIN.Visitors.Mrz.Models
{
    // Scan result container for MRZ processing
    public class ScanResult : IDisposable
    {
        public NormalizedDocumentData Data { get; set; } = new NormalizedDocumentData();
        public ValidationEvidence Evidence { get; set; } = new ValidationEvidence();
        public bool IsSuccess => Evidence.ValidationStatus == "Valid" || Evidence.ValidationStatus == "Warning";

        public MrzErrorCode ErrorCode { get; set; } = MrzErrorCode.None;
        public ParsedMrzResult? ParsedResult { get; set; } = new ParsedMrzResult();

        public string FullNameEnglish => $"{Data.GivenNames} {Data.Surname}".Trim();
        public string? FullNameArabic { get; set; } = null;
        public string NationalID => Data.DocumentNumber;

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