using System;
using System.Text;
using AIN.Visitors.Mrz.Helpers;
using AIN.Visitors.Mrz.Models;

namespace AIN.Visitors.Mrz.Parsers
{
    public static class Td2Parser
    {
        public static ParsedMrzResult Parse(ReadOnlySpan<char> line1, ReadOnlySpan<char> line2)
        {
            var result = new ParsedMrzResult
            {
                DocumentFormat = "TD2",
                ValidationStatus = "Valid"
            };

            if (line1.Length != 36 || line2.Length != 36)
            {
                result.ValidationStatus = "Invalid";
                result.ErrorCodes.Add(nameof(MrzErrorCode.InvalidLength));
                return result;
            }

            try
            {
                // --- استخراج الحقول من السطر الأول (Line 1) ---
                string docType = line1.Slice(0, 2).ToString().Replace("<", "").Trim();
                string issuingState = line1.Slice(2, 3).ToString();
                string namesRaw = line1.Slice(5, 31).ToString();

                result.DocumentType = docType;

                // --- استخراج الحقول من السطر الثاني (Line 2) وفحص أرقام التحقق ---
                var docNumberSpan = line2.Slice(0, 9);
                char docNumberCheckExpected = line2[9];
                bool isDocNumValid = CheckDigitHelper.Verify(docNumberSpan, docNumberCheckExpected);
                result.CheckDigitResults["DocumentNumber"] = isDocNumValid;
                if (!isDocNumValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.DocumentNumberCheckDigitFailure));
                }

                string nationality = line2.Slice(10, 3).ToString();

                var dobSpan = line2.Slice(13, 6);
                char dobCheckExpected = line2[19];
                bool isDobValid = CheckDigitHelper.Verify(dobSpan, dobCheckExpected);
                result.CheckDigitResults["DateOfBirth"] = isDobValid;
                if (!isDobValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.BirthDateCheckDigitFailure));
                }

                string sexRaw = line2.Slice(20, 1).ToString();

                var expirySpan = line2.Slice(21, 6);
                char expiryCheckExpected = line2[27];
                bool isExpiryValid = CheckDigitHelper.Verify(expirySpan, expiryCheckExpected);
                result.CheckDigitResults["DateOfExpiry"] = isExpiryValid;
                if (!isExpiryValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.ExpiryDateCheckDigitFailure));
                }

                var optionalSpan = line2.Slice(28, 7);
                
                // --- فحص رقم التحقق المركب Composite Check Digit ---
                char compositeExpected = line2[35];
                var compositeBuilder = new StringBuilder(35);
                compositeBuilder.Append(line2.Slice(0, 10));
                compositeBuilder.Append(line2.Slice(13, 7));
                compositeBuilder.Append(line2.Slice(21, 7));
                compositeBuilder.Append(line2.Slice(28, 7));

                bool isCompositeValid = CheckDigitHelper.Verify(compositeBuilder.ToString().AsSpan(), compositeExpected);
                result.CompositeCheckDigitResult = isCompositeValid;
                if (!isCompositeValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.CompositeCheckDigitFailure));
                }

                // 2. تطبيق التحقق الدلالي (Semantic Validation)
                result.DocumentNumber = docNumberSpan.ToString().Replace("<", "").Trim();
                
                MrzSemanticValidator.ValidateSemantics(
                    result,
                    namesRaw,
                    dobSpan.ToString(),
                    expirySpan.ToString(),
                    sexRaw,
                    nationality,
                    issuingState,
                    optionalSpan.ToString()
                );
            }
            catch (ArgumentException)
            {
                result.ValidationStatus = "Invalid";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.IllegalCharacter)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.IllegalCharacter));
                }
            }
            catch (Exception)
            {
                result.ValidationStatus = "Invalid";
                result.ErrorCodes.Add(nameof(MrzErrorCode.InternalParserFailure));
            }

            return result;
        }
    }
}