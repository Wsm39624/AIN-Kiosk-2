using System;

namespace AIN_Kiosk.Services
{
    // Digital receipt data transfer object for session processing
    public class ReceiptDetails
    {
        public string ReceptionistAlert { get; set; } = string.Empty;
        public string DataCaptured { get; set; } = string.Empty;
        public string StatusStatement { get; set; } = string.Empty;
    }

    // Service responsible for generating digital receipt details for visitors
    public class PrivacyReceiptService
    {
        // Generates receipt details with prototype issuance status wording
        public ReceiptDetails GenerateReceipt(string currentFlow, string hostName, string purpose, bool isArabic, string visitorName = "")
        {
            string displayVisitorName = string.IsNullOrWhiteSpace(visitorName)
                ? (isArabic ? "زائر" : "Visitor")
                : visitorName;

            var details = new ReceiptDetails
            {
                StatusStatement = isArabic
                    ? "معاينة الإيصال الرقمي - بانتظار الإصدار من السيرفر"
                    : "Digital Privacy Receipt Preview — Backend issuance pending"
            };

            if (isArabic)
            {
                details.ReceptionistAlert = currentFlow == "WalkIn"
                    ? "تنبيه الاستقبال: تم تسجيل طلب معاينة الإيصال لزائر بدون موعد، بانتظار معالجة السيرفر."
                    : $"تنبيه الاستقبال: وصل الزائر ({displayVisitorName}) وهو في انتظار المضيف ({hostName}) حالياً.";

                details.DataCaptured = $"• حالة الإيصال: معاينة - بانتظار معالجة السيرفر\n• اسم الزائر: {displayVisitorName}\n• الشخص المضيف: {hostName}\n• غرض الزيارة: {purpose}";
            }
            else
            {
                details.ReceptionistAlert = currentFlow == "WalkIn"
                    ? "Reception Alert: Unscheduled visitor preview recorded. Awaiting backend processing."
                    : $"Reception Alert: Visitor ({displayVisitorName}) has arrived and is waiting for Host ({hostName}).";

                details.DataCaptured = $"• Receipt Status: Preview — Backend issuance pending\n• Visitor Name: {displayVisitorName}\n• Host Person: {hostName}\n• Purpose: {purpose}";
            }

            return details;
        }
    }
}