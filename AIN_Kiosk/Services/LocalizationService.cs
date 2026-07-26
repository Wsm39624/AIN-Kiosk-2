using System;
using System.Collections.Generic;

namespace AIN_Kiosk.Services
{
    public class LocalizationService
    {
        // مخزن النصوص اللغوية الشامل لعناصر الواجهة
        private readonly Dictionary<string, (string Ar, string En)> _stringResources = new()
        {
            // الشاشة الرئيسية (Home View)
            { "WelcomeTitle", ("مرحباً بك في جهاز الخدمة الذاتية", "Welcome to the Self-Service Kiosk") },
            { "WelcomeSubtitle", ("يرجى اختيار مسار الزيارة المناسب للبدء", "Please select the appropriate visit path to start") },
            { "BtnPreRegistered", ("زائر مسجل", "Pre-Registered") },
            { "BtnWalkIn", ("زائر بدون موعد", "Walk-In") },
            { "BtnRetroactive", ("تسجيل بأثر رجعي", "Retroactive Entry") },
            { "BtnHelp", ("مساعدة", "Help") },

            // شاشة الفحص (Scan View)
            { "ScanTitleWalkIn", ("جاري معالجة مسار زائر بدون موعد...", "Processing Walk-In flow...") },
            { "ScanTitlePreRegistered", ("جاري معالجة مسار زائر مسجل مسبقاً...", "Processing Pre-Registered flow...") },
            { "ScanSubtitle", ("يرجى وضع بطاقة الهوية أو جواز السفر على جهاز الفحص", "Please place your ID card or passport on the scanner") },

            // شاشة استكمال البيانات (Details View)
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

            // شاشة الإيصال والطباعة (Privacy & Receipt)
            { "QrLabel", ("امسح الرمز للاحتفاظ بالوثيقة", "Scan QR to keep the receipt") },
            { "BtnAcceptPrint", ("أوافق واطبع البطاقة", "Accept & Print Badge") },
            { "BtnDoneReceipt", ("إنهاء والعودة للرئيسية", "Finish & Return Home") },

            // شاشة التسجيل بأثر رجعي (Retroactive View)
            { "TxtRetroTitle", ("تسجيل زائر بأثر رجعي (Retroactive Entry)", "Retroactive Entry Logging") },
            { "TxtRetroWarning", ("⚠️ تنبيه أمني: أنت تقوم بتسجيل حركة زائر سابقة يدوياً. يخضع هذا الإجراء للتدقيق الصارم.", "⚠️ Security Notice: You are recording a past visitor entry manually. This action is fully audited.") },
            { "LblRetroVisName", ("اسم الزائر بالكامل:", "Visitor Full Name:") },
            { "LblRetroDocNum", ("رقم الوثيقة / الهوية:", "Document / ID Number:") },
            { "LblRetroHost", ("اسم الموظف المضيف:", "Host Employee Name:") },
            { "LblRetroPurpose", ("غرض الزيارة الفعلي:", "Actual Visit Purpose:") },
            { "ChkRetroAttestation", ("أقر وأصادق بصفتي المضيف بأنني رافقت هذا الزائر مادية وأن البيانات أعلاه دقيقة.", "I confirm I escorted this visitor and the above metadata is accurate.") },
            { "BtnSubmitRetroactive", ("تأكيد وحفظ السجل بأثر رجعي", "Confirm & Save Retroactive Record") },
            { "BtnCancelRetroactive", ("إلغاء والعودة للرئيسية", "Cancel & Return Home") },

            // نصوص سياسة الخصوصية وحماية البيانات (PDPL)
            { "PrivacyTitleWalkIn", ("إقرار سياسة الخصوصية الفوري (One-Click Consent)", "One-Click Privacy Consent") },
            { "PrivacyBodyWalkIn", ("بضغطك على الزر أدناه، يتم منح الموافقة الفورية على معالجة الهوية آلياً وإصدار بطاقة الدخول الممتثلة لنظام PDPL.", "By clicking the button below, you grant immediate authorization to process your ID metadata and issue a temporary entry badge compliant with PDPL regulations.") },
            { "PrivacyTitleDefault", ("وثيقة سياسة الخصوصية وحماية البيانات", "Privacy Policy & Data Protection Contract") },
            { "PrivacyBodyDefault", ("يتعهد نظام (عين) لحلول الزوار بحماية بياناتك الشخصية وفقاً للأنظمة واللوائح الصادرة من الهيئة السعودية للبيانات والذكاء الاصطناعي (سدايا) ومصرف تداول المركزي (ساما). بضغطك على زر (أوافق واطبع البطاقة)، فإنك تمنح النظام صلاحية معالجة هذه البيانات بشكل آمن ومؤقت.", "The (AIN) Visitor Solutions system is committed to protecting your personal data in accordance with the regulations issued by SDAIA and SAMA. By clicking (Accept & Print Badge), you grant the system permission to process this data securely.") }
        };

        // دالة جلب النص بناءً على مفتاح العنصر واللغة الحالية
        public string GetText(string key, bool isArabic)
        {
            if (_stringResources.TryGetValue(key, out var resource))
            {
                return isArabic ? resource.Ar : resource.En;
            }
            return string.Empty;
        }
    }
}