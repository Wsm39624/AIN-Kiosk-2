using System;
using System.Threading;
using System.Threading.Tasks;
using AIN.Visitors.Mrz.Abstractions;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Parsers;

namespace AIN.Visitors.Mrz.Scanners
{
    public enum MockScenario { Success, InvalidMrz, ScannerUnavailable, Timeout, Cancellation, HardwareFailure }

    public class MockDocumentScanner : IDocumentScanner
    {
        public MockScenario Scenario { get; set; } = MockScenario.Success;

        public async Task<ScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            // محاكاة تأخير الأجهزة برمجياً وبدون Console.WriteLine
            await Task.Delay(100, cancellationToken); 

            return Scenario switch
            {
                MockScenario.InvalidMrz => CreateInvalidResult(),
                MockScenario.ScannerUnavailable => throw new InvalidOperationException("Scanner unavailable."),
                MockScenario.Timeout => throw new TimeoutException("Scanner timed out."),
                MockScenario.Cancellation => throw new OperationCanceledException(cancellationToken),
                MockScenario.HardwareFailure => throw new Exception("Hardware failure."),
                _ => CreateSuccessResult() 
            };
        }

        private ScanResult CreateSuccessResult()
        {
            // عينة جواز سفر TD3 حقيقية وسليمة (صالح من ملف العينات المرفق P<UTOERIKSSON<<ANNA<MARIA...)
            string line1 = "P<UTOERIKSSON<<ANNA<MARIA<<<<<<<<<<<<<<<<<<<";
            string line2 = "L898902C36UTO7408122F1204159<<<<<<<<<<<<<<08";

            // تمرير النص على المحلل الحقيقي لضمان التحقق الفعلي (الشرط 10)
            ParsedMrzResult parsed = Td3Parser.Parse(line1.AsSpan(), line2.AsSpan());

            return MapToScanResult(parsed);
        }

        private ScanResult CreateInvalidResult()
        {
            // عينة TD3 فاسدة (تاريخ ميلاد مستحيل شهر 13 من ملف العينات BAD-03)
            string line1 = "P<UTOERIKSSON<<ANNA<MARIA<<<<<<<<<<<<<<<<<<<";
            string line2 = "L898902C36UTO7413122F3004157<<<<<<<<<<<<<<04";

            ParsedMrzResult parsed = Td3Parser.Parse(line1.AsSpan(), line2.AsSpan());

            return MapToScanResult(parsed);
        }

        // دالة مساعدة لربط العقد الموحد ParsedMrzResult بعقد الماسح المشترك ScanResult الخاص بوسام ومحمد
        private static ScanResult MapToScanResult(ParsedMrzResult parsed)
        {
            return new ScanResult
            {
                Data = new NormalizedDocumentData
                {
                    DocumentType = parsed.DocumentType,
                    IssuingState = parsed.IssuingState,
                    DocumentNumber = parsed.DocumentNumber,
                    Surname = parsed.Surname,
                    GivenNames = parsed.GivenNames,
                    DateOfBirth = parsed.DateOfBirth,
                    Sex = parsed.Sex,
                    DateOfExpiry = parsed.DateOfExpiry,
                    Nationality = parsed.Nationality,
                    OptionalData = parsed.OptionalData
                },
                Evidence = new ValidationEvidence
                {
                    ValidationStatus = parsed.ValidationStatus,
                    ErrorCodes = parsed.ErrorCodes,
                    CheckDigitResults = parsed.CheckDigitResults,
                    CompositeCheckDigitResult = parsed.CompositeCheckDigitResult
                }
            };
        }
    }
}