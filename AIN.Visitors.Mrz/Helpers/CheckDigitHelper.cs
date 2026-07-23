using System;

namespace AIN.Visitors.Mrz.Helpers
{
    /// <summary>
    /// مساعد حساب أرقام التحقق (تم تصحيح ثغرة الأحرف غير القانونية ودعم ReadOnlySpan للأداء العالي).
    /// </summary>
    public static class CheckDigitHelper
    {
        // الأوزان القياسية المعتمدة من ICAO لحساب أرقام التحقق (7, 3, 1 وتتكرر)
        private static readonly int[] IcaoWeights = { 7, 3, 1 };

        /// <summary>
        /// حساب رقم التحقق لنص معين مع الرفض الفوري للأحرف غير القانونية قبل الحساب.
        /// تدعم ReadOnlySpan<char> لكي تقبل (string و Span و ReadOnlySpan) معاً!
        /// </summary>
        public static char Calculate(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                return '0';
            }

            int sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // التحقق الفوري: إذا كان الحرف غير قانوني (ليس A-Z وليس 0-9 وليس <) نرفض المعالجة فوراً
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || c == '<'))
                {
                    throw new ArgumentException($"Illegal character '{c}' detected. Processing aborted before check-digit calculation.");
                }

                int charValue = GetValue(c);
                sum += charValue * IcaoWeights[i % 3];
            }

            int remainder = sum % 10;
            return (char)('0' + remainder);
        }

        public static char CalculateCheckDigit(ReadOnlySpan<char> input) => Calculate(input);

        public static bool Verify(ReadOnlySpan<char> input, char expectedCheckDigit)
        {
            try
            {
                char calculated = Calculate(input);
                return calculated == expectedCheckDigit;
            }
            catch (ArgumentException)
            {
                // إذا احتوى النص على أحرف غير قانونية، يفشل التحقق مباشرة ولا يُعامل كصفر
                return false;
            }
        }

        /// <summary>
        /// الاسم الطويل لدالة التحقق.
        /// </summary>
        public static bool VerifyCheckDigit(ReadOnlySpan<char> input, char expectedCheckDigit) => Verify(input, expectedCheckDigit);

        /// <summary>
        /// تحويل الحرف إلى قيمته الرياضية المعتمدة في ICAO 9303.
        /// تم تصحيح الثغرة: أي حرف غير قانوني سيرمي Exception صريح بدلاً من إرجاع 0 بصمت.
        /// </summary>
        public static int GetValue(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A' + 10;
            }
            if (c == '<')
            {
                return 0; // حرف التعبئة فقط هو الذي يحصل على القيمة صفر
            }

            // منع إعطاء الصفر بصمت لأي رمز آخر
            throw new ArgumentException($"Unsupported character '{c}' encountered. Cannot assign mathematical value.");
        }

        /// <summary>
        /// الأسماء البديلة لدالة تحويل الحرف لضمان التوافق.
        /// </summary>
        public static int GetCharacterValue(char c) => GetValue(c);
        public static int GetNumericValue(char c) => GetValue(c);
    }
}