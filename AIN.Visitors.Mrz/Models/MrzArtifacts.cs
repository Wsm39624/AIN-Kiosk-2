using System;
using System.Collections.Generic;

namespace AIN.Visitors.Mrz.Models
{
    /// <summary>
    /// البيانات المطبّعة والموحدة للمستند (آمنة تماماً ولا تحتوي على أي نصوص خام أو صور).
    /// </summary>
    public class NormalizedDocumentData 
    {
        public string DocumentType { get; set; } = string.Empty;
        public string IssuingState { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string GivenNames { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty; // YYYY-MM-DD
        public string? Sex { get; set; } = null;                // M, F, X, or null
        public string DateOfExpiry { get; set; } = string.Empty;// YYYY-MM-DD
        public string Nationality { get; set; } = string.Empty;
        public string? OptionalData { get; set; } = null;
    }

    /// <summary>
    /// أدلة التحقق وموثوقية المستند.
    /// </summary>
    public class ValidationEvidence 
    {
        public string ValidationStatus { get; set; } = "Unknown"; // Valid, Invalid, Warning
        public List<string> ErrorCodes { get; set; } = new();
        public Dictionary<string, bool> CheckDigitResults { get; set; } = new();
        public bool CompositeCheckDigitResult { get; set; }
    }

    /// <summary>
    /// حاوية النص الخام المقيدة (Restricted Artifact) - يتم استدعاؤها عبر واجهات مخصصة للبيانات الحساسة فقط.
    /// </summary>
    public class RawMrzArtifact : IDisposable
    {
        private char[]? _buffer;

        public RawMrzArtifact(ReadOnlySpan<char> rawData)
        {
            _buffer = rawData.ToArray();
        }

        public ReadOnlySpan<char> GetRawDataSpan()
        {
            if (_buffer == null) throw new ObjectDisposedException(nameof(RawMrzArtifact));
            return _buffer.AsSpan();
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _buffer = null;
            }
            GC.SuppressFinalize(this);
        }
    }

    public class DocumentImageReference 
    {
        public string ImageId { get; set; } = string.Empty;
    }
}