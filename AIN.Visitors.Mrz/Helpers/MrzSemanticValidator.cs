using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AIN.Visitors.Mrz.Models;

namespace AIN.Visitors.Mrz.Helpers
{
    /// <summary>
    /// المحقق الدلالي الشامل لوثائق السفر (يغطي جميع التحققات الـ 11 المطلوبة في المخطط المعتمد).
    /// تم تحديثه لاستخدام nameof(MrzErrorCode) لتجنب الأخطاء الإملائية وضمان التطابق التام مع كتالوج الأخطاء.
    /// </summary>
    public static class MrzSemanticValidator
    {
        // التحقيق 8: قائمة معتمدة لرموز الدول (يمكن التوسع فيها لاحقاً عبر الملف التعريفي Profile)
        private static readonly HashSet<string> KnownCountryCodes = new HashSet<string>
        {
            "SAU", "UTO", "USA", "GBR", "FRA", "DEU", "CAN", "EGY", "ARE", "KWT", "BHR", "QAT", "OMN", "JOR", "LBN", "D0", "XXA", "XXB", "XXC"
        };

        /// <summary>
        /// تنفيذ التحقق الدلالي لجميع الشروط وتحديث كائن النتيجة دون رمي Exceptions.
        /// </summary>
        public static void ValidateSemantics(
            ParsedMrzResult result,
            string rawNameField,
            string rawBirthDate,
            string rawExpiryDate,
            string rawSex,
            string rawNationality,
            string rawIssuingState,
            string? rawOptionalData = null)
        {
            // ---------------------------------------------------------------------
            // التحقيق 7: Illegal-character detection (اكتشاف الأحرف غير القانونية)
            // ---------------------------------------------------------------------
            string allRawData = $"{rawNameField}{rawBirthDate}{rawExpiryDate}{rawSex}{rawNationality}{rawIssuingState}{rawOptionalData}";
            if (!Regex.IsMatch(allRawData, "^[A-Z0-9<]*$"))
            {
                result.ValidationStatus = "Invalid";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.IllegalCharacter)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.IllegalCharacter));
                }
            }

            // ---------------------------------------------------------------------
            // التحقيق 11: Filler validation (التحقق من صحة استخدام وتنظيف أحرف التعبئة)
            // ---------------------------------------------------------------------
            if (!string.IsNullOrEmpty(rawNameField) && (rawNameField.StartsWith("<") || rawNameField.Contains("<<<")))
            {
                result.ValidationStatus = result.ValidationStatus == "Invalid" ? "Invalid" : "Warning";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.InvalidFormat)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.InvalidFormat));
                }
            }

            // ---------------------------------------------------------------------
            // التحقيق 9: Missing name-separator handling (معالجة نقص فاصل الأسماء <<)
            // ---------------------------------------------------------------------
            if (!string.IsNullOrEmpty(rawNameField))
            {
                if (!rawNameField.Contains("<<"))
                {
                    result.ValidationStatus = result.ValidationStatus == "Invalid" ? "Invalid" : "Warning";
                    if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.InvalidFormat)))
                    {
                        result.ErrorCodes.Add(nameof(MrzErrorCode.InvalidFormat));
                    }
                    // في حال غياب الفاصل المزدوج، يُعتبر النص كاملاً هو اللقب (Surname) بعد تنظيف الفواصل الفردية
                    result.Surname = rawNameField.Replace("<", " ").Trim();
                    result.GivenNames = string.Empty;
                }
                else
                {
                    var nameParts = rawNameField.Split(new[] { "<<" }, StringSplitOptions.None);
                    result.Surname = nameParts[0].Replace("<", " ").Trim();
                    result.GivenNames = nameParts.Length > 1 ? nameParts[1].Replace("<", " ").Trim() : string.Empty;
                }
            }

            // ---------------------------------------------------------------------
            // التحقيق 8: Country and nationality-code handling (معالجة رموز الدول والجنسيات)
            // ---------------------------------------------------------------------
            ValidateCountryCode(result, rawNationality, "Nationality");
            ValidateCountryCode(result, rawIssuingState, "IssuingState");

            // ---------------------------------------------------------------------
            // التحقيق 6: Invalid sex-marker detection (اكتشاف رموز الجنس غير الصالحة)
            // ---------------------------------------------------------------------
            if (rawSex != "M" && rawSex != "F" && rawSex != "X" && rawSex != "<")
            {
                result.ValidationStatus = "Invalid";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.InvalidSexMarker)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.InvalidSexMarker));
                }
                result.Sex = null;
            }
            else
            {
                result.Sex = rawSex == "<" ? null : rawSex;
            }

            // ---------------------------------------------------------------------
            // التحقيق 1, 3 & 4: التواريخ المستحيلة، وقواعد تحديد القرن، والقرن الغامض للولادة
            // ---------------------------------------------------------------------
            if (TryResolveDate(rawBirthDate, true, out DateTime birthDate, out bool isBirthAmbiguous))
            {
                result.DateOfBirth = birthDate.ToString("yyyy-MM-dd");
                if (isBirthAmbiguous)
                {
                    result.ValidationStatus = result.ValidationStatus == "Invalid" ? "Invalid" : "Warning";
                    if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.AmbiguousCentury)))
                    {
                        result.ErrorCodes.Add(nameof(MrzErrorCode.AmbiguousCentury));
                    }
                }
                // تاريخ الميلاد لا يجوز أن يكون في المستقبل
                if (birthDate > DateTime.UtcNow)
                {
                    result.ValidationStatus = "Invalid";
                    if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.ImpossibleBirthDate)))
                    {
                        result.ErrorCodes.Add(nameof(MrzErrorCode.ImpossibleBirthDate));
                    }
                }
            }
            else
            {
                result.ValidationStatus = "Invalid";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.ImpossibleBirthDate)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.ImpossibleBirthDate));
                }
            }

            // ---------------------------------------------------------------------
            // التحقيق 2, 3 & 5: تاريخ الانتهاء المستحيل، وقواعد القرن، واكتشاف المستند المنتهي
            // ---------------------------------------------------------------------
            if (TryResolveDate(rawExpiryDate, false, out DateTime expiryDate, out _))
            {
                result.DateOfExpiry = expiryDate.ToString("yyyy-MM-dd");
                // التحقق هل المستند منتهي الصلاحية بناءً على تاريخ اليوم
                if (expiryDate < DateTime.UtcNow.Date)
                {
                    result.ValidationStatus = "Invalid";
                    if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.ExpiredDocument)))
                    {
                        result.ErrorCodes.Add(nameof(MrzErrorCode.ExpiredDocument));
                    }
                }
            }
            else
            {
                result.ValidationStatus = "Invalid";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.ImpossibleExpiryDate)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.ImpossibleExpiryDate));
                }
            }

            // ---------------------------------------------------------------------
            // التحقيق 10: Optional-data validation (التحقق من والتعامل مع البيانات الاختيارية)
            // ---------------------------------------------------------------------
            if (!string.IsNullOrEmpty(rawOptionalData))
            {
                string cleanedOptional = rawOptionalData.TrimEnd('<');
                result.OptionalData = string.IsNullOrEmpty(cleanedOptional) ? null : cleanedOptional;
            }
        }

        private static void ValidateCountryCode(ParsedMrzResult result, string code, string fieldName)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 3)
            {
                result.ValidationStatus = "Invalid";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.InvalidFormat)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.InvalidFormat));
                }
                return;
            }

            if (!KnownCountryCodes.Contains(code) && !code.StartsWith("XX") && code != "<<<")
            {
                result.ValidationStatus = result.ValidationStatus == "Invalid" ? "Invalid" : "Warning";
                if (!result.ErrorCodes.Contains(nameof(MrzErrorCode.UnknownCountryCode)))
                {
                    result.ErrorCodes.Add(nameof(MrzErrorCode.UnknownCountryCode));
                }
            }

            if (fieldName == "Nationality") result.Nationality = code == "<<<" ? string.Empty : code;
            if (fieldName == "IssuingState") result.IssuingState = code == "<<<" ? string.Empty : code;
        }

        private static bool TryResolveDate(string yymmdd, bool isBirthDate, out DateTime resolvedDate, out bool isAmbiguous)
        {
            resolvedDate = DateTime.MinValue;
            isAmbiguous = false;

            if (string.IsNullOrEmpty(yymmdd) || yymmdd.Length != 6)
                return false;

            if (!int.TryParse(yymmdd.Substring(0, 2), out int year) ||
                !int.TryParse(yymmdd.Substring(2, 2), out int month) ||
                !int.TryParse(yymmdd.Substring(4, 2), out int day))
            {
                return false;
            }

            int currentYearMod100 = DateTime.UtcNow.Year % 100;
            int currentCentury = (DateTime.UtcNow.Year / 100) * 100;

            int fullYear;
            if (isBirthDate)
            {
                if (Math.Abs(year - currentYearMod100) <= 5)
                {
                    isAmbiguous = true;
                }

                if (year > currentYearMod100)
                    fullYear = (currentCentury - 100) + year;
                else
                    fullYear = currentCentury + year;
            }
            else
            {
                if (year < 70 && year < currentYearMod100 - 20)
                    fullYear = currentCentury + year;
                else if (year >= 70)
                    fullYear = (currentCentury - 100) + year;
                else
                    fullYear = currentCentury + year;
            }

            try
            {
                resolvedDate = new DateTime(fullYear, month, day, 0, 0, 0, DateTimeKind.Utc);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}