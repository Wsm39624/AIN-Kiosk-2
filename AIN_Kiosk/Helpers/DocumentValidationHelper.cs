using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AIN_Kiosk.Helpers
{
    public static class DocumentValidationHelper
    {
        // Validates document number based on document type and country profile
        public static bool IsValidDocument(string docNumber, string docType, string countryCode = "SA")
        {
            if (string.IsNullOrWhiteSpace(docNumber))
                return false;

            string cleanDoc = docNumber.Trim();

            return docType?.ToLowerInvariant() switch
            {
                "passport" => IsValidPassport(cleanDoc),
                "national_id" => IsValidSaudiNationalId(cleanDoc),
                "iqama" => IsValidIqama(cleanDoc),
                _ => cleanDoc.Length >= 5 && cleanDoc.Length <= 20 && Regex.IsMatch(cleanDoc, @"^[a-zA-Z0-9\-\/]+$")
            };
        }

        // Validates passport number supporting alphanumeric characters between 6 and 15 characters
        public static bool IsValidPassport(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber)) return false;
            string clean = passportNumber.Trim();
            return clean.Length >= 6 && clean.Length <= 15 && Regex.IsMatch(clean, @"^[a-zA-Z0-9]+$");
        }

        // Validates Saudi National ID (10 numeric digits starting with 1)
        public static bool IsValidSaudiNationalId(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId)) return false;
            string clean = nationalId.Trim();
            return clean.Length == 10 && clean.StartsWith("1") && long.TryParse(clean, out _);
        }

        // Validates Saudi Iqama (10 numeric digits starting with 2)
        public static bool IsValidIqama(string iqama)
        {
            if (string.IsNullOrWhiteSpace(iqama)) return false;
            string clean = iqama.Trim();
            return clean.Length == 10 && clean.StartsWith("2") && long.TryParse(clean, out _);
        }

        // Unicode-aware name validator supporting letters, spaces, hyphens, apostrophes, and Arabic combining marks
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            foreach (char c in name)
            {
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
                bool isValidChar = char.IsLetter(c) ||
                                   char.IsWhiteSpace(c) ||
                                   c == '-' || c == '\'' ||
                                   cat == UnicodeCategory.NonSpacingMark ||
                                   cat == UnicodeCategory.SpacingCombiningMark;

                if (!isValidChar) return false;
            }

            return true;
        }
    }
}