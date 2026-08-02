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

            TxtSupervisorTitle.Text = isArabic ? "بوابة المشرف [إعدادات تطويرية]" : "Supervisor Gate [Dev Settings]";
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

            // Task #10: Removed rigid 10 digits label restriction
            TxtRetroDocNumber.Tag = isArabic ? "رقم الهوية الوطنية أو جواز السفر" : "National ID or Passport Document Number";
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

            // Task #9: Configurable email handling
            LblEmail.Text = isArabic ? "البريد الإلكتروني (مطلوب للإيصال الرقمي):" : "Email Address (Required for Digital Receipt):";

            ChipMeeting.Content = _localizationService.GetText("ChipMeeting", isArabic);
            ChipInterview.Content = _localizationService.GetText("ChipInterview", isArabic);
            ChipMaintenance.Content = _localizationService.GetText("ChipMaintenance", isArabic);
            ChipDelivery.Content = _localizationService.GetText("ChipDelivery", isArabic);
            ChipVendor.Content = _localizationService.GetText("ChipVendor", isArabic);
            ChipOther.Content = _localizationService.GetText("ChipOther", isArabic);
            BtnNextToPrivacy.Content = _localizationService.GetText("BtnNextToPrivacy", isArabic);

            QrLabel.Text = isArabic ? "امسح الرمز للاحتفاظ بالوثيقة" : "Scan QR to keep the receipt";
            if (!_workflowService.IsInErrorState) BtnAcceptPrint.Content = isArabic ? "متابعة وطباعة البطاقة" : "Continue & Print Badge";
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

            // Task #8: Removed misleading hardcoded claims
            PrivacyTitle.Text = isArabic ? "إشعار الخصوصية وشروط معالجة البيانات" : "Privacy Notice & Data Processing Terms";
            PrivacyBody.Text = isArabic
                ? $"تلتزم جهة ({_configProvider.TenantName}) بمعالجة بيانات الزوار وفقاً للأطر التنظيمية المعمول بها وبناءً على شروط الخدمة المعتمدة. تشمل المعالجة طباعة بطاقة الزائر وإشعار المستضيف."
                : $"({_configProvider.TenantName}) is committed to processing visitor data in accordance with effective regulatory frameworks and tenant-approved terms. Processing includes printing visitor credentials and host notification.";

            if (PanelReceiptDetails.Visibility == Visibility.Visible)
            {
                UpdateReceiptLanguageTexts();
            }
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            string helpTitle = isArabic ? "تعليمات استخدام جهاز الخدمة الذاتية" : "Kiosk Help & Instructions";
            string helpMessage = isArabic
                ? "أهلاً بك!\n\n1. اختر 'زائر مسجل' إذا كان لديك موعد مسبق، أو 'زائر بدون موعد'.\n2. قم بمسح بطاقة الهوية/جواز السفر عند طلب القارئ.\n3. استكمل بيانات المستضيف لطباعة بطاقة الدخول واستلام الإيصال الرقمي."
                : "Welcome!\n\n1. Select 'Pre-Registered' if you have an appointment, or 'Walk-In'.\n2. Scan your National ID or Passport when prompted.\n3. Complete host details to print your visitor badge and receive a digital receipt.";

            MessageBox.Show(helpMessage, helpTitle, MessageBoxButton.OK, MessageBoxImage.Information);
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
                // Item 5: Updated to match prototype receipt status preview wording
                TxtReceiptHeader.Text = "معاينة الإيصال الرقمي - بانتظار الإصدار من السيرفر";

                TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                    ? "[معاينة محاكاة - لم يُرسل] تنبيه الاستقبال: تسجيل دخول زائر بدون موعد."
                    : $"[معاينة محاكاة - لم يُرسل] تنبيه الاستقبال: الزائر بانتظار المضيف ({host}).";

                TxtReceiptDataCaptured.Text = $"• [معاينة إيصال] الجهة: {_configProvider.TenantName}\n• المضيف: {host}\n• الغرض: {purpose}";
                TxtReceiptPurgeDate.Text = $"سياسة الاحتفاظ المعتمدة: {_privacyNoticeProvider.GetRetentionStatement(true)}";
                TxtReceiptHash.Text = $"مرجع الإيصال الرقمي: {_receiptToken}";
            }
            else
            {
                // Item 5: Updated to match prototype receipt status preview wording
                TxtReceiptHeader.Text = "Digital Privacy Receipt Preview — Backend issuance pending";

                TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                    ? "[Simulated Preview - Not Sent] Receptionist alert: Walk-in check-in logged."
                    : $"[Simulated Preview - Not Sent] Receptionist alert: Visitor waiting for Host ({host}).";

                TxtReceiptDataCaptured.Text = $"• [Receipt Preview] Org: {_configProvider.TenantName}\n• Host: {host}\n• Purpose: {purpose}";
                TxtReceiptPurgeDate.Text = $"Effective Retention Policy: {_privacyNoticeProvider.GetRetentionStatement(false)}";
                TxtReceiptHash.Text = $"Digital Receipt Reference: {_receiptToken}";
            }

            if (TxtPrivacyPortalInstruction != null)
            {
                TxtPrivacyPortalInstruction.Text = isArabic
                    ? "لطلب استعراض البيانات الشخصية أو ممارسة حقوق الخصوصية، يرجى مسح رمز الـ QR أعلاه بجوالك للانتقال المباشر إلى بوابة الويب الآمنة."
                    : "To view personal data or exercise privacy rights, please scan the QR code above with your mobile device to access the secure web portal.";
            }
        }
    }
}