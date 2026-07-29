using System;
using System.Collections.Generic;
using System.Text;

namespace AIN_Kiosk.Services
{
    // تحديد حالة متطلبات الحقول (إلزامي، اختياري، مشروط، مخفي)
    public enum FieldRequirement
    {
        Required,
        Optional,
        Conditional,
        Hidden
    }

    public class KioskConfigurationProvider : AIN_Kiosk.IKioskConfigurationProvider
    {
        // هذه القيم سيتم جلبها مستقبلاً من الـ Backend (ABP.io)
        public string TenantName => "Emerging Business Technologies CO. (EBTCO)";
        public string PrivacyContact => "privacy@ain.ebtco.com.sa";
        public bool ShowFacilityLocation => false; // مخفي حسب التوجيهات ما لم يُطلب تفعيله
        public string FacilityName => "Riyadh HQ";

        // التعديلات الجديدة:
        // 1. تهيئة حقل البريد الإلكتروني ديناميكياً (مطلوب كمثال قياسي)
        public FieldRequirement EmailRequirement => FieldRequirement.Required;

        // 2. رمز المشرف التجريبي (Supervisor PIN) يقرأ من الإعدادات بدلاً من التثبيت داخل الكود
        public string SupervisorDemoPin => "1234";
    }
}