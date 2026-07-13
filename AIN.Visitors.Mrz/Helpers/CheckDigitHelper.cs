using System;

namespace AIN.Visitors.Mrz.Helpers
{
    public static class CheckDigitHelper
    {
        private static readonly int[] Weights = { 7, 3, 1 };

        // التعديل السحري هنا: استقبال المؤشر بدلاً من النص لصفر استهلاك للذاكرة
        public static int Calculate(ReadOnlySpan<char> input)
        {
            int sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                int value = GetCharacterValue(c);
                int weight = Weights[i % 3];
                sum += value * weight;
            }
            return sum % 10;
        }

        private static int GetCharacterValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            if (c == '<') return 0;
            return 0;
        }
    }
}