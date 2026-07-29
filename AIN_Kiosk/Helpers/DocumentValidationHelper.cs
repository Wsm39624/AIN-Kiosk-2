using System;

namespace AIN_Kiosk.Helpers
{
    public static class DocumentValidationHelper
    {
        public static bool IsValidDocument(string docNumber, string docType, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(docNumber))
                return false;

            string cleanDoc = docNumber.Trim();

            return docType?.ToLower() switch
            {
                "passport" => cleanDoc.Length >= 6 && cleanDoc.Length <= 15,
                "national_id" => cleanDoc.Length == 10 && long.TryParse(cleanDoc, out _),
                "iqama" => cleanDoc.Length == 10 && cleanDoc.StartsWith("2") && long.TryParse(cleanDoc, out _),
                _ => cleanDoc.Length >= 5 && cleanDoc.Length <= 20
            };
        }
    }
}