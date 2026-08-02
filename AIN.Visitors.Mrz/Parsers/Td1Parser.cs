using System;
using System.Text;
using AIN.Visitors.Mrz.Helpers;
using AIN.Visitors.Mrz.Models;

namespace AIN.Visitors.Mrz.Parsers
{
    public static class Td1Parser
    {
        public static ParsedMrzResult Parse(ReadOnlySpan<char> line1, ReadOnlySpan<char> line2, ReadOnlySpan<char> line3)
        {
            var result = new ParsedMrzResult
            {
                DocumentFormat = "TD1",
                ValidationStatus = "Valid"
            };

            if (line1.Length != 30 || line2.Length != 30 || line3.Length != 30)
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
                string docNumberRaw = line1.Slice(5, 9).ToString();
                
                result.DocumentType = docType;

                // --- استخراج الحقول من السطر الثاني (Line 2) وفحص أرقام التحقق ---
                var docNumberSpan = line1.Slice(5, 9);
                char docNumberCheckExpected = line2[0];
                bool isDocNumValid = CheckDigitHelper.Verify(docNumberSpan, docNumberCheckExpected);
                result.CheckDigitResults["DocumentNumber"] = isDocNumValid;
                if (!isDocNumValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.DocumentNumberCheckDigitFailure));
                }

                string optional1Raw = line2.Slice(1, 15).ToString();

                var dobSpan = line2.Slice(16, 6);
                char dobCheckExpected = line2[22];
                bool isDobValid = CheckDigitHelper.Verify(dobSpan, dobCheckExpected);
                result.CheckDigitResults["DateOfBirth"] = isDobValid;
                if (!isDobValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.BirthDateCheckDigitFailure));
                }

                string sexRaw = line2.Slice(23, 1).ToString();

                var expirySpan = line2.Slice(24, 6);
                char expiryCheckExpected = line3[0];
                bool isExpiryValid = CheckDigitHelper.Verify(expirySpan, expiryCheckExpected);
                result.CheckDigitResults["DateOfExpiry"] = isExpiryValid;
                if (!isExpiryValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.ExpiryDateCheckDigitFailure));
                }

                // --- استخراج الحقول من السطر الثالث (Line 3) ---
                string nationality = line3.Slice(1, 3).ToString();
                string optional2Raw = line3.Slice(4, 11).ToString();
                
                string combinedOptional = $"{optional1Raw}{optional2Raw}".Replace("<", "").Trim();
                string namesRaw = line3.Slice(15, 15).ToString();

                // --- فحص رقم التحقق المركب Composite Check Digit ---
                char compositeExpected = line3[14];
                var compositeBuilder = new StringBuilder(30);
                compositeBuilder.Append(line1.Slice(5, 9));
                compositeBuilder.Append(line2.Slice(0, 1));
                compositeBuilder.Append(line2.Slice(1, 15));
                compositeBuilder.Append(line2.Slice(16, 7));
                compositeBuilder.Append(line2.Slice(24, 6));
                compositeBuilder.Append(line3.Slice(0, 1));
                compositeBuilder.Append(line3.Slice(4, 10));

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
                    string.IsNullOrEmpty(combinedOptional) ? null : combinedOptional
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