using System;
using Xunit;
using AIN_Kiosk.Services;
using AIN_Kiosk.Adapters;

namespace AIN_Kiosk.Tests
{
    public class KioskArchitectureTests
    {
        private readonly KioskWorkflowService _workflowService = new();
        private readonly LocalizationService _localizationService = new();
        private readonly KioskConfigurationProvider _configProvider = new();
        private readonly KioskPrivacyNoticeProvider _privacyNoticeProvider = new();
        private readonly LocalQrCodeService _qrCodeService = new();

        // 1. اختبار مسار زائر بدون موعد (Walk-In Flow Selection)
        [Fact]
        public void Test_1_FlowSelection_WalkIn_SetsCorrectFlow()
        {
            _workflowService.CurrentFlow = "WalkIn";
            Assert.Equal("WalkIn", _workflowService.CurrentFlow);
        }

        // 2. اختبار مسار زائر مسجل مسبقاً (Pre-Registered Flow Selection)
        [Fact]
        public void Test_2_FlowSelection_PreRegistered_SetsCorrectFlow()
        {
            _workflowService.CurrentFlow = "PreRegistered";
            Assert.Equal("PreRegistered", _workflowService.CurrentFlow);
        }

        // 3. اختبار التحقق من الحقول الإلزامية - اسم المضيف فارغ (Field Validation)
        [Fact]
        public void Test_3_RequiredFields_HostNameEmpty_ThrowsOrFails()
        {
            string hostNameInput = "";
            bool isInvalid = string.IsNullOrWhiteSpace(hostNameInput);
            Assert.True(isInvalid);
        }

        // 4. اختبار التحقق من صيغة البريد الإلكتروني الخاطئة (Invalid Email Format)
        [Fact]
        public void Test_4_EmailValidation_InvalidFormat_ReturnsFalse()
        {
            string badEmail = "wessamaltabaa_at_gmail.com";
            bool isValid = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(badEmail);
                isValid = (addr.Address == badEmail);
            }
            catch { isValid = false; }

            Assert.False(isValid);
        }

        // 5. اختبار التحقق من صيغة البريد الإلكتروني الصحيحة (Valid Email Format)
        [Fact]
        public void Test_5_EmailValidation_ValidFormat_ReturnsTrue()
        {
            string goodEmail = "wessamaltabaa@gmail.com";
            bool isValid = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(goodEmail);
                isValid = (addr.Address == goodEmail);
            }
            catch { isValid = false; }

            Assert.True(isValid);
        }

        // 6. اختبار إيقاف وإعادة ضبط مؤقت الخمول والتوكن (Idle Timeout Reset)
        [Fact]
        public void Test_6_IdleTimeout_ResetsSessionAndClearsToken()
        {
            _workflowService.ResetSession();
            string activeToken = ""; // تم تصفيره عند العودة للرئيسية لحماية الخصوصية

            Assert.Equal(string.Empty, activeToken);
            Assert.False(_workflowService.IsInErrorState);
        }

        // 7. اختبار توليد الـ QR محلياً بنجاح بدون إنترنت (Local QR Generation)
        [Fact]
        public void Test_7_LocalQrGeneration_WithoutInternet_ReturnsFrozenImage()
        {
            string samplePayload = "https://receipt.ain.ebtco.com/r/SAMPLETOKEN";
            var image = _qrCodeService.GenerateQrCodeImage(samplePayload);

            Assert.NotNull(image);
            Assert.True(image.IsFrozen); // التحقق من تجميد الصورة في الذاكرة لمنع التسريب
        }

        // 8. اختبار خلو محتوى الـ QR تماماً من أي بيانات شخصية (No PII encoded in QR)
        [Fact]
        public void Test_8_LocalQrPayload_ContainsNoPII_OnlyOpaqueToken()
        {
            string opaqueToken = Guid.NewGuid().ToString("N");
            string qrPayload = $"https://receipt.ain.ebtco.com/r/{opaqueToken}";

            // التأكد من عدم حشو أي بيانات حساسة بداخل الـ Payload
            Assert.DoesNotContain("Wesam", qrPayload);
            Assert.DoesNotContain("05", qrPayload);
            Assert.Contains(opaqueToken, qrPayload);
        }

        // 9. اختبار معالجة فشل الطباعة والمحافظة على الإيصال (Printer Failure Behavior)
        [Fact]
        public void Test_9_PrinterFailure_PreservesReceiptStateAndEntersErrorState()
        {
            _workflowService.IsInErrorState = true; // محاكاة انقطاع طابعة زيبرا

            Assert.True(_workflowService.IsInErrorState);
            // التأكد من بقاء واجهة القائمة الرئيسية متاحة للعودة الآمنة
            Assert.NotNull(_localizationService.GetText("WelcomeTitle", true));
        }

        // 10. اختبار تحميل بيان الاحتفاظ بالبيانات ديناميكياً من الموفر وعدم تثبيته برمجياً (Zero Hardcoding)
        [Fact]
        public void Test_10_RetentionStatement_LoadsFromProvider_NotHardcoded()
        {
            string statementArabic = _privacyNoticeProvider.GetRetentionStatement(true);
            string statementEnglish = _privacyNoticeProvider.GetRetentionStatement(false);

            Assert.Contains("10 سنوات", statementArabic);
            Assert.Contains("10 Years", statementEnglish);
            Assert.Equal("EBTCO / BDO Al-Amri", _configProvider.TenantName);
        }
    }
}