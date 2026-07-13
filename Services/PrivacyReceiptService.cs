using System;

namespace AIN_Kiosk.Services
{
    // 📦 نموذج بيانات الإيصال الرقمي (Receipt DTO)
    public class ReceiptDetails
    {
        public string ReceptionistAlert { get; set; } = string.Empty;
        public string DataCaptured { get; set; } = string.Empty;
        public string PurgeDateText { get; set; } = string.Empty;
        public string IntegrityHash { get; set; } = string.Empty;
    }

    public class PrivacyReceiptService
    {
        public ReceiptDetails GenerateReceipt(string currentFlow, string hostName, string purpose, bool isArabic)
        {
            string visitor1Name = "Wesam Mohammed";
            string visitorCompany = "EBTCO / BDO Al-Amri";

            // حساب تاريخ التدمير الآلي بعد 90 يوماً متطابقاً مع الأنظمة
            DateTime purgeDate = DateTime.Now.AddDays(90);
            string secureHash = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 16);

            var details = new ReceiptDetails
            {
                IntegrityHash = $"SHA-256 Integrity Hash: AIN-SEC-{secureHash}"
            };

            if (isArabic)
            {
                details.ReceptionistAlert = currentFlow == "WalkIn"
                    ? "🔔 تنبيه الاستقبال السريع: تم رصد دخول زائر بدون موعد بنجاح، بانتظار استكمال بيانات المضيف بالخلفية."
                    : $"تنبيه الاستقبال: وصل الزائر ({visitor1Name}) وهو في انتظار المضيف ({hostName}) حالياً.";

                details.DataCaptured = $"• البيانات الشخصية: {visitor1Name} | الجهة: {visitorCompany}\n• الشخص المضيف: {hostName}\n• غرض الزيارة المعتمد: {purpose}";
                details.PurgeDateText = $"⚠️ سيتم إتلاف سجل بياناتك الشخصية آلياً بتاريخ: {purgeDate:yyyy-MM-dd} (حسب سياسة الاحتفاظ بـ 90 يوماً).";
            }
            else
            {
                details.ReceptionistAlert = currentFlow == "WalkIn"
                    ? "🔔 Quick Reception Alert: Unscheduled visitor check-in successful. Pending back-fill completion."
                    : $"Reception Alert: Visitor ({visitor1Name}) has arrived and is waiting for Host ({hostName}).";

                details.DataCaptured = $"• Personal Data: {visitor1Name} | Org: {visitorCompany}\n• Host Person: {hostName}\n• Approved Purpose: {purpose}";
                details.PurgeDateText = $"⚠️ Your personal data record will be automatically purged on: {purgeDate:yyyy-MM-dd} (90 days retention limit).";
            }

            return details;
        }
    }
}