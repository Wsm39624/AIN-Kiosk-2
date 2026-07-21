using System.Windows;

namespace AIN_Kiosk
{
    public partial class MainWindow
    {
        private void BtnLangToggle_Click(object sender, RoutedEventArgs e)
        {
            isArabic = !isArabic;
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            if (_workflowService == null || _localizationService == null || _configProvider == null || _privacyNoticeProvider == null) return;

            KioskWindow.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            BtnLangToggle.Content = isArabic ? "English" : "العربية";
            TxtHeaderLogo.Text = "AIN | عين";

            WelcomeTitle.Text = _localizationService.GetText("WelcomeTitle", isArabic);
            WelcomeSubtitle.Text = _localizationService.GetText("WelcomeSubtitle", isArabic);
            BtnPreRegistered.Content = _localizationService.GetText("BtnPreRegistered", isArabic);
            BtnWalkIn.Content = _localizationService.GetText("BtnWalkIn", isArabic);
            BtnHelp.Content = _localizationService.GetText("BtnHelp", isArabic);

            if (BtnSupervisorTrigger != null)
            {
                BtnSupervisorTrigger.Visibility = (ViewHome.Visibility == Visibility.Visible)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            TxtSupervisorTitle.Text = isArabic ? "بوابة المشرف الأمنية (زر تجريبي)" : "Supervisor Security Gateway (Demo Button)";
            TxtSupervisorSubtitle.Text = isArabic ? "الرجاء إدخال رمز التحقق لتفويض الصلاحيات" : "Please enter authentication code to authorize access";
            BtnSubmitPin.Content = isArabic ? "تحقق" : "Verify";
            BtnCancelPin.Content = isArabic ? "إلغاء" : "Cancel";

            TxtConsoleTitle.Text = isArabic ? "لوحة تحكم المشرف والعمليات الاستثنائية" : "Supervisor Console & Exceptional Actions";
            BtnRetroactiveEntry.Content = isArabic ? "تسجيل زائر بأثر رجعي" : "Retroactive Visitor Logging";
            BtnExitConsole.Content = isArabic ? "خروج العودة للرئيسية" : "Exit and Return Home";

            TxtHostName.Tag = isArabic ? "أدخل اسم الموظف المستضيف هنا" : "Enter host employee name here";
            TxtMobileNumber.Tag = isArabic ? "مثال: 05XXXXXXXX" : "Example: 05XXXXXXXX";
            TxtEmail.Tag = isArabic ? "مثال: user@domain.com" : "Example: user@domain.com";

            TxtRetroVisitorName.Tag = isArabic ? "أدخل اسم الزائر الكامل" : "Enter visitor full name";
            TxtRetroDocNumber.Tag = isArabic ? "رقم الهوية أو جواز السفر (10 أرقام)" : "National ID or Passport (10 digits)";
            TxtRetroHostName.Tag = isArabic ? "اسم الموظف المستضيف" : "Host employee name";
            TxtRetroPurpose.Tag = isArabic ? "اكتب الغرض الفعلي من الزيارة" : "Type actual visit purpose";

            ScanTitle.Text = _workflowService.CurrentFlow == "WalkIn"
                ? _localizationService.GetText("ScanTitleWalkIn", isArabic)
                : _localizationService.GetText("ScanTitlePreRegistered", isArabic);
            ScanSubtitle.Text = _localizationService.GetText("ScanSubtitle", isArabic);

            DetailsTitle.Text = _localizationService.GetText("DetailsTitle", isArabic);
            DetailsSubtitle.Text = _localizationService.GetText("DetailsSubtitle", isArabic);
            LblHostName.Text = _localizationService.GetText("LblHostName", isArabic);
            LblPurpose.Text = _localizationService.GetText("LblPurpose", isArabic);
            LblMobile.Text = _localizationService.GetText("LblMobile", isArabic);
            LblEmail.Text = isArabic ? "البريد الإلكتروني - اختياري:" : "Email Address - Optional:";
            ChipMeeting.Content = _localizationService.GetText("ChipMeeting", isArabic);
            ChipInterview.Content = _localizationService.GetText("ChipInterview", isArabic);
            ChipMaintenance.Content = _localizationService.GetText("ChipMaintenance", isArabic);
            ChipDelivery.Content = _localizationService.GetText("ChipDelivery", isArabic);
            ChipVendor.Content = _localizationService.GetText("ChipVendor", isArabic);
            ChipOther.Content = _localizationService.GetText("ChipOther", isArabic);
            BtnNextToPrivacy.Content = _localizationService.GetText("BtnNextToPrivacy", isArabic);

            QrLabel.Text = isArabic ? "امسح الرمز للاحتفاظ بالوثيقة" : "Scan QR to keep the receipt";
            if (!_workflowService.IsInErrorState) BtnAcceptPrint.Content = isArabic ? "أوافق واطبع البطاقة" : "Agree & Print Badge";
            BtnDoneReceipt.Content = isArabic ? "إنهـاء والعودة للرئيسية" : "Finish & Return Home";

            TxtRetroTitle.Text = isArabic ? "تسجيل زائر بأثر رجعي" : "Retroactive Entry";
            TxtRetroWarning.Text = _localizationService.GetText("TxtRetroWarning", isArabic);
            LblRetroVisName.Text = _localizationService.GetText("LblRetroVisName", isArabic);
            LblRetroDocNum.Text = _localizationService.GetText("LblRetroDocNum", isArabic);
            LblRetroHost.Text = _localizationService.GetText("LblRetroHost", isArabic);
            LblRetroPurpose.Text = _localizationService.GetText("LblRetroPurpose", isArabic);
            ChkRetroAttestation.Content = _localizationService.GetText("ChkRetroAttestation", isArabic);
            BtnSubmitRetroactive.Content = isArabic ? "تأكيد وحفظ السجل بأثر رجعي" : "Confirm & Save Retroactive Record";
            BtnCancelRetroactive.Content = isArabic ? "إلغاء والعودة للرئيسية" : "Cancel & Return Home";

            PrivacyTitle.Text = isArabic ? "إقرار سياسة الخصوصية الفوري" : "One-Click Privacy Consent";
            PrivacyBody.Text = isArabic
                ? "يتعهد نظام (عين) لحلول الزوار بمعالجة بياناتك الشخصية وفقاً لسياسات الخصوصية والأنظمة المعمول بها. بضغطك على زر أوافق، فإنك تمنح النظام صلاحية معالجة هذه البيانات بشكل آمن ومؤقت لاستخراج بطاقة الزائر."
                : "AIN Visitors System is committed to processing your personal data in accordance with applicable privacy policies and regulations. By clicking Agree, you authorize the system to process this data securely and temporarily to issue your visitor badge.";

            if (PanelReceiptDetails.Visibility == Visibility.Visible)
            {
                UpdateReceiptLanguageTexts();
            }
        }

        private void UpdateReceiptLanguageTexts()
        {
            string host = _workflowService.SelectedHostName;
            if (string.IsNullOrWhiteSpace(host) || host == "قيد التدقيق" || host == "Pending Queue")
            {
                host = isArabic ? "قيد التدقيق" : "Pending Queue";
            }
            else if (host == "غير محدد" || host == "Unspecified")
            {
                host = isArabic ? "غير محدد" : "Unspecified";
            }

            string purpose = _workflowService.SelectedPurpose;
            if (isArabic)
            {
                purpose = purpose switch
                {
                    "Business Meeting" => "اجتماع عمل",
                    "Personal Interview" => "مقابلة شخصية",
                    "Technical Maintenance" => "صيانة فنية",
                    "Delivery / Shipment" => "توصيل / شحنة",
                    "Vendor / Client" => "مورد / عميل",
                    "Other" => "أخرى",
                    "Walk-In Guest" => "زيارة بدون موعد",
                    "Walk-In Unexpected Guest" => "زيارة بدون موعد",
                    _ => purpose
                };
            }
            else
            {
                purpose = purpose switch
                {
                    "اجتماع عمل" => "Business Meeting",
                    "مقابلة شخصية" => "Personal Interview",
                    "صيانة فنية" => "Technical Maintenance",
                    "توصيل / شحنة" => "Delivery / Shipment",
                    "مورد / عميل" => "Vendor / Client",
                    "أخرى" => "Other",
                    "زيارة بدون موعد" => "Walk-In Guest",
                    "Walk-In Unexpected Guest" => "Walk-In Guest",
                    _ => purpose
                };
            }

            if (isArabic)
            {
                TxtReceiptHeader.Text = "✓ تم إصدار إيصال الخصوصية الرقمي بنجاح";

                TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                    ? "[معاينة محاكاة - لم يُرسل] تنبيه الاستقبال: تسجيل دخول زائر بدون موعد."
                    : $"[معاينة محاكاة - لم يُرسل] تنبيه الاستقبال: الزائر بانتظار المضيف ({host}).";

                TxtReceiptDataCaptured.Text = $"• [معاينة إيصال] الجهة: {_configProvider.TenantName}\n• المضيف: {host}\n• الغرض: {purpose}";
                TxtReceiptPurgeDate.Text = $"الأساس القانوني: نظام حماية البيانات الشخصية \nفترة الاحتفاظ المحددة: {_privacyNoticeProvider.GetRetentionStatement(true)}";
                TxtReceiptHash.Text = $"رمز الوصول المرجعي المباشر: {_receiptToken}";
            }
            else
            {
                TxtReceiptHeader.Text = "✓ Digital Privacy Receipt Issued Successfully";

                TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                    ? "[Simulated Preview - Not Sent] Receptionist alert: Walk-in check-in logged."
                    : $"[Simulated Preview - Not Sent] Receptionist alert: Visitor waiting for Host ({host}).";

                TxtReceiptDataCaptured.Text = $"• [Receipt Preview] Org: {_configProvider.TenantName}\n• Host: {host}\n• Purpose: {purpose}";
                TxtReceiptPurgeDate.Text = $"Legal Basis: Personal Data Protection Law (PDPL)\nRetention Period: {_privacyNoticeProvider.GetRetentionStatement(false)}";
                TxtReceiptHash.Text = $"Opaque Token Reference: {_receiptToken}";
            }

            if (TxtPrivacyPortalInstruction != null)
            {
                TxtPrivacyPortalInstruction.Text = isArabic
                    ? "لطلب استعراض البيانات الشخصية أو ممارسة حقوق الخصوصية، يرجى مسح رمز الـ QR أعلاه بجوالك للانتقال المباشر إلى بوابة الويب الآمنة (تتطلب تحقق OTP)."
                    : "To view personal data or exercise privacy rights, please scan the QR code above with your mobile device to access the secure web portal (requires OTP verification).";
            }
        }
    }
}