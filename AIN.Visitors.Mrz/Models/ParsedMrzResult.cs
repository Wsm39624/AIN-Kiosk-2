using System;
using System.Collections.Generic;

namespace AIN.Visitors.Mrz.Models
{
    /// <summary>
    /// العقد الموحد المؤرخ لمخرجات تحليل MRZ حسب شروط المخطط المعتمد (القسم 3 و 4 و 10).
    /// </summary>
    public class ParsedMrzResult
    {
        public string ContractVersion { get; set; } = "1.0.0";
        public string ParserVersion { get; set; } = "2.0.0";

        public string DocumentFormat { get; set; } = string.Empty; // TD1, TD2, or TD3
        public string DocumentType { get; set; } = string.Empty; // e.g., P, I, A, C
        public string IssuingState { get; set; } = string.Empty; // رمز الدولة المصدرة (3 أحرف ICAO)
        public string Nationality { get; set; } = string.Empty; // رمز الجنسية (3 أحرف ICAO)

        public string DocumentNumber { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty; // الاسم الأخير المطبّع بدون أحرف التعبئة <
        public string GivenNames { get; set; } = string.Empty; // الأسماء الأولى المطبّعة بدون أحرف التعبئة <
        public string DateOfBirth { get; set; } = string.Empty; // التنسيق القياسي المؤرخ ISO Format: YYYY-MM-DD
        public string? Sex { get; set; } = null; // M, F, X أو null في حال عدم التحديد
        public string DateOfExpiry { get; set; } = string.Empty; // التنسيق القياسي المؤرخ ISO Format: YYYY-MM-DD
        public string? OptionalData { get; set; } = null; // البيانات الاختيارية

        public Dictionary<string, bool> CheckDigitResults { get; set; } = new Dictionary<string, bool>();
        public bool CompositeCheckDigitResult { get; set; }
        public string ValidationStatus { get; set; } = "Unknown"; // حالات: Valid, Invalid, Warning
        public List<string> ErrorCodes { get; set; } = new List<string>(); // قائمة الرموز الدقيقة من فهرس الأخطاء

        public string ParsedAt { get; set; } = DateTime.UtcNow.ToString("o"); // التوقيت العالمي ISO 8601 UTC
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString(); // معرّف التتبع الفريد
    }
}