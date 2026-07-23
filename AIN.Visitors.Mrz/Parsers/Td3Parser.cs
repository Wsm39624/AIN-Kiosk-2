using System;
using System.Text;
using AIN.Visitors.Mrz.Helpers;
using AIN.Visitors.Mrz.Models;

namespace AIN.Visitors.Mrz.Parsers
{
    public static class Td3Parser
    {
        public static ParsedMrzResult Parse(ReadOnlySpan<char> line1, ReadOnlySpan<char> line2)
        {
            var result = new ParsedMrzResult
            {
                DocumentFormat = "TD3",
                ValidationStatus = "Valid"
            };

            // 1. التحقق من أطوال الأسطر (الشرط الأساسي: 44 حرف لكل سطر في TD3)
            if (line1.Length != 44 || line2.Length != 44)
            {
                result.ValidationStatus = "Invalid";
                result.ErrorCodes.Add(nameof(MrzErrorCode.InvalidLength));
                return result;
            }

            try
            {
                // --- استخراج الحقول الخام من السطر الأول (Line 1) ---
                string docType = line1.Slice(0, 2).ToString().Replace("<", "").Trim();
                string issuingState = line1.Slice(2, 3).ToString();
                string namesRaw = line1.Slice(5, 39).ToString();

                result.DocumentType = docType;

                // --- استخراج الحقول من السطر الثاني (Line 2) وفحص أرقام التحقق الفردية ---
                // رقم المستند (الخانات 0 إلى 8) ورقم التحقق (الخانة 9)
                var docNumberSpan = line2.Slice(0, 9);
                char docNumberCheckExpected = line2[9];
                bool isDocNumValid = CheckDigitHelper.Verify(docNumberSpan, docNumberCheckExpected);
                result.CheckDigitResults["DocumentNumber"] = isDocNumValid;
                if (!isDocNumValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.DocumentNumberCheckDigitFailure));
                }

                // رمز الجنسية (الخانات 10 إلى 12)
                string nationality = line2.Slice(10, 3).ToString();

                // تاريخ الميلاد (الخانات 13 إلى 18) ورقم التحقق (الخانة 19)
                var dobSpan = line2.Slice(13, 6);
                char dobCheckExpected = line2[19];
                bool isDobValid = CheckDigitHelper.Verify(dobSpan, dobCheckExpected);
                result.CheckDigitResults["DateOfBirth"] = isDobValid;
                if (!isDobValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.BirthDateCheckDigitFailure));
                }

                // الجنس (الخانة 20)
                string sexRaw = line2.Slice(20, 1).ToString();

                // تاريخ الانتهاء (الخانات 21 إلى 26) ورقم التحقق (الخانة 27)
                var expirySpan = line2.Slice(21, 6);
                char expiryCheckExpected = line2[27];
                bool isExpiryValid = CheckDigitHelper.Verify(expirySpan, expiryCheckExpected);
                result.CheckDigitResults["DateOfExpiry"] = isExpiryValid;
                if (!isExpiryValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.ExpiryDateCheckDigitFailure));
                }

                // البيانات الاختيارية/الشخصية (الخانات 28 إلى 41) ورقم التحقق الخاص بها (الخانة 42)
                var optionalSpan = line2.Slice(28, 14);
                char optionalCheckExpected = line2[42];
                bool isOptionalValid = true;
                
                // في جوازات السفر، إذا كان الحقل الاختياري يحتوي على أحرف تعبئة فقط (<) فإن رقم التحقق يكون < أو 0
                if (!optionalSpan.TrimStart('<').IsEmpty)
                {
                    isOptionalValid = CheckDigitHelper.Verify(optionalSpan, optionalCheckExpected);
                    if (!isOptionalValid)
                    {
                        result.ValidationStatus = "Invalid";
                        result.ErrorCodes.Add(nameof(MrzErrorCode.OptionalDataCheckDigitFailure));
                    }
                }
                result.CheckDigitResults["OptionalData"] = isOptionalValid;

                // --- فحص رقم التحقق المركب Composite Check Digit (الخانة 43) ---
                // الحساب يجمع: رقم المستند + رقم تحققه + تاريخ الميلاد + رقم تحققه + تاريخ الانتهاء + رقم تحققه + البيانات الاختيارية + رقم تحققها
                char compositeExpected = line2[43];
                var compositeBuilder = new StringBuilder(43);
                compositeBuilder.Append(line2.Slice(0, 10));  // Doc num + check
                compositeBuilder.Append(line2.Slice(13, 7));  // DOB + check
                compositeBuilder.Append(line2.Slice(21, 7));  // Expiry + check
                compositeBuilder.Append(line2.Slice(28, 15)); // Optional + check

                bool isCompositeValid = CheckDigitHelper.Verify(compositeBuilder.ToString().AsSpan(), compositeExpected);
                result.CompositeCheckDigitResult = isCompositeValid;
                if (!isCompositeValid)
                {
                    result.ValidationStatus = "Invalid";
                    result.ErrorCodes.Add(nameof(MrzErrorCode.CompositeCheckDigitFailure));
                }

                // 2. تطبيق التحقق الدلالي (Semantic Validation) على الحقول المستخرجة
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
                // إذا تم اكتشاف حرف غير قانوني أثناء فحص أرقام التحقق
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