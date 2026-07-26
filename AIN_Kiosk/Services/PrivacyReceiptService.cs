using System;

namespace AIN_Kiosk.Services
{
    
    // Digital receipt data transfer object for session processing.
    
    public class ReceiptDetails
    {
        public string ReceptionistAlert { get; set; } = string.Empty;
        public string DataCaptured { get; set; } = string.Empty;
    }

    
    // Service responsible for generating digital receipt details for visitors.
    
    public class PrivacyReceiptService
    {
        
        // Generates receipt details using active session workflow data without fake hashes or hardcoded retention rules.
        
        public ReceiptDetails GenerateReceipt(string currentFlow, string hostName, string purpose, bool isArabic, string visitorName = "")
        {
            // Use synthetic generic fallback if visitor name is not supplied by active workflow session
            string displayVisitorName = string.IsNullOrWhiteSpace(visitorName)
                ? (isArabic ? "زائر" : "Visitor")
                : visitorName;

            var details = new ReceiptDetails();

            if (isArabic)
            {
                details.ReceptionistAlert = currentFlow == "WalkIn"
                    ? "تنبيه الاستقبال: تم رصد دخول زائر بدون موعد بنجاح، بانتظار استكمال بيانات المضيف."
                    : $"تنبيه الاستقبال: وصل الزائر ({displayVisitorName}) وهو في انتظار المضيف ({hostName}) حالياً.";

                details.DataCaptured = $"• اسم الزائر: {displayVisitorName}\n• الشخص المضيف: {hostName}\n• غرض الزيارة: {purpose}";
            }
            else
            {
                details.ReceptionistAlert = currentFlow == "WalkIn"
                    ? "Reception Alert: Unscheduled visitor check-in recorded. Awaiting host details completion."
                    : $"Reception Alert: Visitor ({displayVisitorName}) has arrived and is waiting for Host ({hostName}).";

                details.DataCaptured = $"• Visitor Name: {displayVisitorName}\n• Host Person: {hostName}\n• Purpose: {purpose}";
            }

            return details;
        }
    }
}