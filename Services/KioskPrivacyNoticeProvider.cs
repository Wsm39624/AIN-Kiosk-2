using System;
using System.Collections.Generic;
using System.Text;

namespace AIN_Kiosk.Services
{
    public class KioskPrivacyNoticeProvider
    {
        public string GetPrivacyNoticeVersion() => "v1.0-Interim";

        public string GetRetentionStatement(bool isArabic)
        {
            // تم سحب الـ 90 يوماً وتغييرها لـ 10 سنوات كقيمة افتراضية مع توضيح أنها محاكاة
            return isArabic
                ? "10 سنوات (قيمة اختبار غير معتمدة - سيتم جلبها من لوحة التحكم لاحقاً)"
                : "10 Years (Unapproved test value - pending ABP backend integration)";
        }
    }
}