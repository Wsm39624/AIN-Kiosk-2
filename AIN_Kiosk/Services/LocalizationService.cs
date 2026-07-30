using System;
using System.Collections.Generic;

namespace AIN_Kiosk.Services
{
    public class LocalizationService
    {
        private string _currentCulture = "ar";

        // Dictionary holding localized UI string resources
        private readonly Dictionary<string, (string Ar, string En)> _stringResources = new()
        {
            // Home View resources
            { "WelcomeTitle", ("مرحباً بك في جهاز الخدمة الذاتية", "Welcome to the Self-Service Kiosk") },
            { "WelcomeSubtitle", ("يرجى اختيار مسار الزيارة المناسب للبدء", "Please select the appropriate visit path to start") },
            { "BtnPreRegistered", ("زائر مسجل", "Pre-Registered") },
            { "BtnWalkIn", ("زائر بدون موعد", "Walk-In") },
            { "BtnRetroactive", ("تسجيل بأثر رجعي", "Retroactive Entry") },
            { "BtnHelp", ("مساعدة", "Help") },

            // Scan View resources
            { "ScanTitleWalkIn", ("جاري معالجة مسار زائر بدون موعد...", "Processing Walk-In flow...") },
            { "ScanTitlePreRegistered", ("جاري معالجة مسار زائر مسجل مسبقاً...", "Processing Pre-Registered flow...") },
            { "ScanSubtitle", ("يرجى وضع بطاقة الهوية أو جواز السفر على جهاز الفحص", "Please place your ID card or passport on the scanner") },

            // Details View resources
            { "DetailsTitle", ("استكمال معلومات الزيارة", "Complete Visit Information") },
            { "DetailsSubtitle", ("يرجى تحديد الشخص المضيف وسبب زيارتك اليوم", "Please specify the host and your purpose of visit") },
            { "LblHostName", ("اسم الشخص المضيف:", "Host Name:") },
            { "LblPurpose", ("سبب الزيارة:", "Purpose of Visit:") },
            { "LblMobile", ("رقم الجوال - اختياري:", "Mobile Number - Optional:") },
            { "ChipMeeting", ("اجتماع عمل", "Business Meeting") },
            { "ChipInterview", ("مقابلة شخصية", "Job Interview") },
            { "ChipMaintenance", ("صيانة فنية", "Technical Maintenance") },
            { "ChipDelivery", ("توصيل / شحنة", "Delivery / Courier") },
            { "ChipVendor", ("مورد / عميل", "Vendor / Supplier") },
            { "ChipOther", ("أخرى", "Other") },
            { "BtnNextToPrivacy", ("التالي: مراجعة سياسة الخصوصية", "Next: Review Privacy Policy") },

            // Privacy and Receipt View resources (Updated to neutral action wording per Item 4)
            { "QrLabel", ("امسح الرمز للاحتفاظ بالوثيقة", "Scan QR to keep the receipt") },
            { "BtnAcceptPrint", ("متابعة وطباعة البطاقة", "Continue & Print Badge") },
            { "BtnDoneReceipt", ("إنهاء والعودة للرئيسية", "Finish & Return Home") },

            // Retroactive View resources
            { "TxtRetroTitle", ("تسجيل زائر بأثر رجعي (Retroactive Entry)", "Retroactive Entry Logging") },
            { "TxtRetroWarning", ("تنبيه أمني: أنت تقوم بتسجيل حركة زائر سابقة يدوياً. يخضع هذا الإجراء للتدقيق الصارم.", "Security Notice: You are recording a past visitor entry manually. This action is fully audited.") },
            { "LblRetroVisName", ("اسم الزائر بالكامل:", "Visitor Full Name:") },
            { "LblRetroDocNum", ("رقم الوثيقة / الهوية:", "Document / ID Number:") },
            { "LblRetroHost", ("اسم الموظف المضيف:", "Host Employee Name:") },
            { "LblRetroPurpose", ("غرض الزيارة الفعلي:", "Actual Visit Purpose:") },
            { "ChkRetroAttestation", ("أقر وأصادق بصفتي المضيف بأنني رافقت هذا الزائر مادية وأن البيانات أعلاه دقيقة.", "I confirm I escorted this visitor and the above metadata is accurate.") },
            { "BtnSubmitRetroactive", ("تأكيد وحفظ السجل بأثر رجعي", "Confirm & Save Retroactive Record") },
            { "BtnCancelRetroactive", ("إلغاء والعودة للرئيسية", "Cancel & Return Home") },

            // Privacy Notice resources (Ensuring neutral information disclosure per Item 3)
            { "PrivacyTitleWalkIn", ("إشعار الخصوصية وشروط معالجة البيانات", "Privacy Notice & Data Processing Terms") },
            { "PrivacyBodyWalkIn", ("يتم جمع بياناتك للتحقق من الهوية وإصدار تصاريح الدخول وفقاً لسياسة المنشأة المعمول بها.", "Your data is collected for identity verification and badge issuance in accordance with the facility's policy.") },
            { "PrivacyTitleDefault", ("إشعار الخصوصية وشروط معالجة البيانات", "Privacy Notice & Data Processing Terms") },
            { "PrivacyBodyDefault", ("يتم جمع ومعالجة بيانات الزائرين وفقاً للأنظمة والسياسات المعتمدة لدى المنشأة. يرجى مراجعة الإشعار قبل المتابعة.", "Visitor data is collected and processed according to approved facility policies. Please review the notice before proceeding.") },

            // Additional Action and Support resources
            { "BtnAccept", ("متابعة واستمرار", "Continue") },
            { "BtnCancel", ("إلغاء", "Cancel") },
            { "HelpTitle", ("المساعدة والدعم", "Help & Support") },
            { "HelpBody", ("يرجى التواصل مع موظف الاستقبال للحصول على المساعدة الفورية.", "Please approach the reception desk for immediate assistance.") }
        };

        public string GetText(string key, bool isArabic)
        {
            if (_stringResources.TryGetValue(key, out var resource))
            {
                return isArabic ? resource.Ar : resource.En;
            }
            return string.Empty;
        }

        public string GetString(string key)
        {
            return GetText(key, _currentCulture == "ar");
        }

        public void SetCulture(string culture)
        {
            _currentCulture = culture;
        }
    }
}