using System;

namespace AIN_Kiosk.Services
{
    // Local mock provider supplying developmental privacy notice retention statements
    public class LocalMockKioskPrivacyNoticeProvider : IKioskPrivacyNoticeProvider
    {
        public string GetPrivacyNoticeVersion() => "v1.0-Interim";

        public string GetRetentionStatement(bool isArabic)
        {
            // Returns local mock retention statement pending dynamic backend configuration
            return isArabic
                ? "10 سنوات (قيمة اختبار محلي - بانتظار الربط بالخلفية)"
                : "10 Years (Local mock value - pending backend integration)";
        }
    }
}