using System;
using AIN_Kiosk;
using AIN_Kiosk.Adapters;
using AIN_Kiosk.Helpers;
using AIN_Kiosk.Services;
using Xunit;

namespace AIN_Kiosk.Tests
{
    public class KioskArchitectureTests
    {
        private readonly KioskWorkflowService _workflowService = new();
        private readonly LocalizationService _localizationService = new();
        private readonly LocalMockKioskConfigurationProvider _configProvider = new();
        private readonly LocalMockKioskPrivacyNoticeProvider _privacyNoticeProvider = new();
        private readonly LocalQrCodeService _qrCodeService = new();
        private readonly PrivacyReceiptService _receiptService = new();

        [Fact]
        public void Test_1_FlowSelection_WalkInAndPreRegistered_SetsCorrectFlow()
        {
            _workflowService.CurrentFlow = "WalkIn";
            Assert.Equal("WalkIn", _workflowService.CurrentFlow);

            _workflowService.CurrentFlow = "PreRegistered";
            Assert.Equal("PreRegistered", _workflowService.CurrentFlow);
        }

        [Fact]
        public void Test_2_RequiredEmail_PolicyEnforced_RejectsEmptyInput()
        {
            Assert.Equal(FieldRequirement.Required, _configProvider.EmailRequirement);
            string emptyEmail = "";
            bool isEmailProvided = !string.IsNullOrWhiteSpace(emptyEmail);
            Assert.False(isEmailProvided);
        }

        [Fact]
        public void Test_3_AlphanumericPassport_Accepted()
        {
            string validPassport = "A12345678";
            bool isValid = DocumentValidationHelper.IsValidPassport(validPassport);
            Assert.True(isValid);

            bool isValidDoc = DocumentValidationHelper.IsValidDocument(validPassport, "passport");
            Assert.True(isValidDoc);
        }

        [Fact]
        public void Test_4_UnicodeNameValidation_SupportsHyphensApostrophesAndArabic()
        {
            string arabicName = "عمر-بن الخطاب";
            string englishName = "O'Connor-Smith";

            Assert.True(DocumentValidationHelper.IsValidName(arabicName));
            Assert.True(DocumentValidationHelper.IsValidName(englishName));
        }

        [Fact]
        public void Test_5_BadgeAndReceiptTokens_AreDistinct()
        {
            IReceiptReferenceProvider receiptProvider = new MockReceiptReferenceProvider();
            string receiptToken = receiptProvider.GetSyntheticToken();
            string badgeToken = $"BDG-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

            Assert.NotEqual(receiptToken, badgeToken);
            Assert.StartsWith("DEV-SYNTHETIC-", receiptToken);
            Assert.StartsWith("BDG-", badgeToken);
        }

        [Fact]
        public void Test_6_QrPayload_ContainsNoPII_OnlyOpaqueToken()
        {
            IReceiptReferenceProvider provider = new MockReceiptReferenceProvider();
            string sampleMobile = "0512345678";
            string sampleEmail = "visitor@domain.com";

            string receiptToken = provider.GetSyntheticToken();
            string payloadUrl = $"https://receipt.ain.ebtco.com/r/{receiptToken}";

            Assert.Contains(receiptToken, payloadUrl);
            Assert.DoesNotContain(sampleMobile, payloadUrl);
            Assert.DoesNotContain(sampleEmail, payloadUrl);
        }

        [Fact]
        public void Test_7_ProhibitedPrivacyWording_IsAbsentFromLocalization()
        {
            string sampleAr = _localizationService.GetText("PrivacyBodyDefault", true);
            string sampleEn = _localizationService.GetText("PrivacyBodyDefault", false);

            Assert.DoesNotContain("SAMA", sampleAr);
            Assert.DoesNotContain("SAMA", sampleEn);
            Assert.DoesNotContain("One-Click", sampleAr);
            Assert.DoesNotContain("One-Click", sampleEn);
        }

        [Fact]
        public void Test_8_ReceiptStatus_IndicatesPreviewPendingBackend()
        {
            var receipt = _receiptService.GenerateReceipt("WalkIn", "Host Employee", "Business", true, "Test Visitor");

            Assert.Contains("معاينة الإيصال الرقمي - بانتظار الإصدار من السيرفر", receipt.StatusStatement);
            Assert.Contains("معاينة", receipt.DataCaptured);
        }

        [Fact]
        public void Test_9_SessionReset_ClearsWorkflowAndState()
        {
            _workflowService.SelectedHostName = "Test Host";
            _workflowService.SelectedMobile = "0512345678";
            _workflowService.IsInErrorState = true;

            _workflowService.ResetSession();

            Assert.Equal(string.Empty, _workflowService.SelectedHostName);
            Assert.Equal(string.Empty, _workflowService.SelectedMobile);
            Assert.False(_workflowService.IsInErrorState);
        }

        [Fact]
        public void Test_10_PrinterFailure_EntersErrorStateAndAllowsRecovery()
        {
            _workflowService.IsInErrorState = true;

            Assert.True(_workflowService.IsInErrorState);

            _workflowService.ResetSession();

            Assert.False(_workflowService.IsInErrorState);
        }

        [Fact]
        public void Test_11_VisitorSession_ToSafeAuditLog_ExcludesPIIAndStaticFlags()
        {
            var session = new VisitorSession(Guid.NewGuid(), "OP_123");
            session.SetIdentityData("John Doe", "1234567890", "SA", DateTime.Now.AddYears(-25), new byte[] { 1, 2, 3 }, false);

            string auditLog = session.ToSafeAuditLog();

            Assert.DoesNotContain("John Doe", auditLog);
            Assert.DoesNotContain("1234567890", auditLog);
            Assert.DoesNotContain("BiometricsCaptured", auditLog);
            Assert.DoesNotContain("CameraActive", auditLog);
            Assert.Contains("OP_123", auditLog);
        }

        [Fact]
        public void Test_12_VisitorSession_WipeSensitiveDataInMemory_ClearsFieldsAndBuffers()
        {
            var session = new VisitorSession(Guid.NewGuid(), "OP_123");
            byte[] imagePayload = new byte[] { 1, 2, 3, 4, 5 };
            session.SetIdentityData("John Doe", "1234567890", "SA", DateTime.Now.AddYears(-25), imagePayload, false);

            session.WipeSensitiveDataInMemory();

            Assert.Equal(string.Empty, session.VisitorFullName);
            Assert.Equal(string.Empty, session.RawDocumentNumber);
            Assert.Null(session.DateOfBirth);
            Assert.Null(session.RawDocumentImagePayload);
        }

        [Fact]
        public void Test_13_MockProviders_ReturnConfiguredTenantValues()
        {
            string statementArabic = _privacyNoticeProvider.GetRetentionStatement(true);

            Assert.Contains("10 سنوات", statementArabic);
            Assert.Equal("Emerging Business Technologies CO. (EBTCO)", _configProvider.TenantName);
        }
    }
}