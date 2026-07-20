using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIN_Kiosk.Services;
using AIN_Kiosk.Adapters;

namespace AIN_Kiosk
{
    public partial class MainWindow : Window
    {
        private bool isArabic = true;
        private string _receiptToken = string.Empty;
        private const string SupervisorPinCode = "1234";

        // Domain services and architecture abstraction definitions
        private readonly KioskWorkflowService _workflowService;
        private readonly BadgePrintService _printService;
        private readonly PrivacyReceiptService _receiptService;
        private readonly LocalizationService _localizationService;
        private readonly ScannerAdapter _scannerAdapter;
        private readonly LocalQrCodeService _qrCodeService;
        private readonly IRegistrationQueue _registrationQueue;
        private readonly IReceiptReferenceProvider _receiptReferenceProvider;
        private readonly IKioskConfigurationProvider _configProvider;
        private readonly IKioskPrivacyNoticeProvider _privacyNoticeProvider;

        public MainWindow()
        {
            InitializeComponent();

            // Core domain engine boundaries initialization
            _workflowService = new KioskWorkflowService();
            _printService = new BadgePrintService();
            _receiptService = new PrivacyReceiptService();
            _localizationService = new LocalizationService();
            _scannerAdapter = new ScannerAdapter();
            _qrCodeService = new LocalQrCodeService();
            _configProvider = new KioskConfigurationProvider();
            _privacyNoticeProvider = new KioskPrivacyNoticeProvider();
            _registrationQueue = new MockRegistrationQueue();
            _receiptReferenceProvider = new MockReceiptReferenceProvider();

            _workflowService.OnIdleTimeout += () =>
            {
                ResetToHomeView(forceResetToArabic: true);
            };

            ApplyLanguage();
            InitializeIdleTimer();
        }

        private async void BtnWalkIn_Click(object sender, RoutedEventArgs e)
        {
            _workflowService.CurrentFlow = "WalkIn";
            await StartKioskWorkflowAsync();
        }

        private async void BtnPreRegistered_Click(object sender, RoutedEventArgs e)
        {
            _workflowService.CurrentFlow = "PreRegistered";
            await StartKioskWorkflowAsync();
        }

        // Handles secure workflow transitions to the administrative retroactive screen
        private void BtnRetroactiveEntry_Click(object sender, RoutedEventArgs e)
        {
            _workflowService.CurrentFlow = "Retroactive";

            TxtRetroVisitorName.Text = string.Empty;
            TxtRetroDocNumber.Text = string.Empty;
            TxtRetroHostName.Text = "Wesam Mohammed";
            ChkRetroAttestation.IsChecked = false;

            ViewSupervisorConsole.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        // Coordinates peripheral asynchronous scan executions
        private async Task StartKioskWorkflowAsync()
        {
            _workflowService.IsInErrorState = false;
            PrintStatusLabel.Text = string.Empty;
            QrCodeContainer.Visibility = Visibility.Collapsed;

            ViewHome.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Visible;
            ApplyLanguage();

            bool isScanValid = false;
            try
            {
                isScanValid = await _scannerAdapter.ExecuteScanAsync();
            }
            catch (Exception ex)
            {
                isScanValid = false;
                Debug.WriteLine($"[Scanner Fault] Optical scanner integration exception: {ex.Message}");

                string errorTitle = _localizationService.GetText("KioskAlertTitle", isArabic);
                if (string.IsNullOrWhiteSpace(errorTitle)) errorTitle = isArabic ? "تنبيه النظام" : "System Alert";

                string errorMsg = _localizationService.GetText("ScannerFatalErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(errorMsg)) errorMsg = isArabic ? "⚠️ حدث خطأ فني غير متوقع في جهاز قارئ الهوية، يرجى المحاولة لاحقاً." : "⚠️ An unexpected scanner error occurred. Please try again later.";

                MessageBox.Show(errorMsg, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (isScanValid)
            {
                ViewScan.Visibility = Visibility.Collapsed;

                if (_workflowService.CurrentFlow == "WalkIn")
                {
                    _workflowService.SelectedHostName = "Pending / Back-fill Queue";
                    _workflowService.SelectedPurpose = "Walk-In Unexpected Guest";
                    _workflowService.SelectedMobile = "Not Provided";

                    ViewPrivacy.Visibility = Visibility.Visible;
                    ApplyLanguage();
                }
                else
                {
                    TxtHostName.Text = "Eng. Khaled Alamri";
                    TxtMobileNumber.Text = string.Empty;

                    if (TxtEmail != null) TxtEmail.Text = "wessamaltabaa@gmail.com";

                    ViewDetails.Visibility = Visibility.Visible;
                    ApplyLanguage();
                }
            }
            else
            {
                ResetToHomeView(forceResetToArabic: true);
            }
        }

        // Processes retroactive form submission with strict criteria assertions
        private void BtnSubmitRetroactive_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = _localizationService.GetText("KioskAlertTitle", isArabic);
            if (string.IsNullOrWhiteSpace(validationTitle)) validationTitle = isArabic ? "تنبيه التحقق" : "Validation Alert";

            if (string.IsNullOrWhiteSpace(TxtRetroVisitorName.Text) || string.IsNullOrWhiteSpace(TxtRetroDocNumber.Text) || string.IsNullOrWhiteSpace(TxtRetroHostName.Text))
            {
                string missingFieldsMsg = _localizationService.GetText("MissingFieldsErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(missingFieldsMsg)) missingFieldsMsg = isArabic ? "عذراً، يجب تعبئة كافة البيانات الأساسية لحفظ السجل!" : "All fields are required for retroactive logging!";

                MessageBox.Show(missingFieldsMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ChkRetroAttestation.IsChecked != true)
            {
                string uncheckedAttestationMsg = _localizationService.GetText("UncheckedAttestationErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(uncheckedAttestationMsg)) uncheckedAttestationMsg = isArabic ? "يجب تفعيل مربع الإقرار والمصادقة الإلكترونية لاعتماد السجل بأثر رجعي!" : "You must check the attestation box to proceed!";

                MessageBox.Show(uncheckedAttestationMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _registrationQueue.EnqueueMockRecord(_workflowService.CurrentFlow, TxtRetroVisitorName.Text);

            string completionMsg = _localizationService.GetText("RetroactiveSubmitSuccessMsg", isArabic);
            if (string.IsNullOrWhiteSpace(completionMsg)) completionMsg = isArabic ? "✓ [محاكاة - لم يتم الحفظ] تم تأكيد السجل بأثر رجعي محلياً بنجاح.\n\n⚠️ تنبيه: الربط الفعلي مع الخلفية معلق (Backend integration pending)." : "✓ [Simulated - not persisted] Retroactive record confirmed locally.\n\n⚠️ Notice: Backend integration pending.";

            MessageBox.Show(completionMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Information);

            ResetToHomeView(forceResetToArabic: true);
        }

        private void BtnCancelRetroactive_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        // Deploys active hardware spool simulation routines
        private async void BtnAcceptPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_workflowService.IsInErrorState)
            {
                ResetToHomeView(forceResetToArabic: true);
                return;
            }

            BtnAcceptPrint.IsEnabled = false;
            _workflowService.StopIdleTimer();

            PrintStatusLabel.Foreground = Brushes.DarkBlue;

            string spoolingText = _localizationService.GetText("SpoolingStatusLabel", isArabic);
            if (string.IsNullOrWhiteSpace(spoolingText)) spoolingText = isArabic ? "⏳ جاري محاكاة طباعة التصميم ثنائي الوجه (Zebra Printer Emulator)..." : "⏳ Running Zebra printer fallback emulator...";
            PrintStatusLabel.Text = spoolingText;

            bool isPrintSuccess = await _printService.PrintVisitorBadgeAsync(
                _workflowService.SelectedHostName,
                _workflowService.SelectedPurpose,
                _workflowService.SelectedMobile,
                _receiptToken
            );

            if (isPrintSuccess)
            {
                PrintStatusLabel.Text = string.Empty;
                ScrollPrivacyText.Visibility = Visibility.Collapsed;
                BtnAcceptPrint.Visibility = Visibility.Collapsed;

                PanelReceiptDetails.Visibility = Visibility.Visible;
                QrCodeContainer.Visibility = Visibility.Visible;
                BtnDoneReceipt.Visibility = Visibility.Visible;
                BorderReceptionistAlert.Visibility = Visibility.Visible;

                _receiptService.GenerateReceipt(
                    _workflowService.CurrentFlow,
                    _workflowService.SelectedHostName,
                    _workflowService.SelectedPurpose,
                    isArabic
                );

                if (isArabic)
                {
                    TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                        ? "🔔 [إشعار استقبال وهمي - لم يُرسل] تنبيه الاستقبال السريع: تم رصد حركة دخول زائر بدون موعد، بانتظار استكمال البيانات بالخلفية."
                        : $"🔔 [إشعار استقبال وهمي - لم يُرسل] تنبيه الاستقبال: الزائر (Wesam Mohammed) في انتظار المضيف ({_workflowService.SelectedHostName}) حالياً.";

                    TxtReceiptDataCaptured.Text = $"• [محاكاة - لم يتم الحفظ] البيانات الشخصية: Wesam Mohammed | الجهة: {_configProvider.TenantName}\n• الشخص المضيف: {_workflowService.SelectedHostName}\n• غرض الزيارة: {_workflowService.SelectedPurpose}";
                    TxtReceiptPurgeDate.Text = $"⚠️ [قيمة اختبار غير معتمدة] سيتم إتلاف سجل بياناتك الشخصية آلياً بعد: {_privacyNoticeProvider.GetRetentionStatement(true)}";
                    TxtReceiptHash.Text = $"مرجع الوصول المبهم (معاينة إيصال عام): {_receiptToken}";
                }
                else
                {
                    TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                        ? "🔔 [Mock notification not sent] Quick Reception Alert: Unscheduled visitor check-in recorded. Pending back-fill completion."
                        : $"🔔 [Mock notification not sent] Reception Alert: Visitor (Wesam Mohammed) has arrived and is waiting for Host ({_workflowService.SelectedHostName}).";

                    TxtReceiptDataCaptured.Text = $"• [Simulated - not persisted] Personal Data: Wesam Mohammed | Org: {_configProvider.TenantName}\n• Host Person: {_workflowService.SelectedHostName}\n• Approved Purpose: {_workflowService.SelectedPurpose}";
                    TxtReceiptPurgeDate.Text = $"⚠️ [Unapproved test value] Personal data record will be automatically purged after: {_privacyNoticeProvider.GetRetentionStatement(false)}";
                    TxtReceiptHash.Text = $"Opaque Access Token (Public Receipt Reference): {_receiptToken}";
                }

                _workflowService.InitializeIdleTimer(TimeSpan.FromSeconds(60));
            }
            else
            {
                _workflowService.IsInErrorState = true;
                BtnAcceptPrint.IsEnabled = true;
                BtnAcceptPrint.Background = Brushes.DarkRed;
                BtnAcceptPrint.Content = isArabic ? "العودة للقائمة الرئيسية" : "Return to Home";
                QrCodeContainer.Visibility = Visibility.Visible;

                PrintStatusLabel.Foreground = Brushes.Red;

                string printerErrorMsg = _localizationService.GetText("PrinterOfflineErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(printerErrorMsg)) printerErrorMsg = isArabic ? "⚠️ عذراً، محاكاة طابعة البطاقات غير متوفرة حالياً (Printer fallback demo)!" : "⚠️ Printer fallback demo: printer is offline!";
                PrintStatusLabel.Text = printerErrorMsg;

                _workflowService.ResetIdleTimer();
            }
        }

        private void BtnDoneReceipt_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        // Restores primary navigation boundaries and clears captured buffer variables
        private void ResetToHomeView(bool forceResetToArabic = false)
        {
            ViewPrivacy.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Collapsed;
            ViewDetails.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Collapsed;
            ViewSupervisorAuth.Visibility = Visibility.Collapsed;
            ViewSupervisorConsole.Visibility = Visibility.Collapsed;

            ViewHome.Visibility = Visibility.Visible;

            BtnAcceptPrint.IsEnabled = true;
            PrintStatusLabel.Text = string.Empty;
            QrCodeContainer.Visibility = Visibility.Collapsed;
            BorderReceptionistAlert.Visibility = Visibility.Collapsed;
            ScrollPrivacyText.Visibility = Visibility.Visible;
            BtnAcceptPrint.Visibility = Visibility.Visible;
            PanelReceiptDetails.Visibility = Visibility.Collapsed;
            BtnDoneReceipt.Visibility = Visibility.Collapsed;

            TxtHostName.Text = string.Empty;
            TxtMobileNumber.Text = string.Empty;
            TxtSupervisorPin.Password = string.Empty;
            if (TxtEmail != null) TxtEmail.Text = "wessamaltabaa@gmail.com";

            if (forceResetToArabic) isArabic = true;

            _receiptToken = string.Empty;
            _workflowService.ResetSession();
            _workflowService.SelectedPurpose = isArabic ? "اجتماع عمل" : "Business Meeting";

            ApplyLanguage();
        }

        private void InitializeIdleTimer()
        {
            _workflowService.InitializeIdleTimer(TimeSpan.FromSeconds(45));
        }

        private void Window_PreviewInteraction(object sender, System.Windows.Input.InputEventArgs e)
        {
            _workflowService.ResetIdleTimer();
        }

        private void BtnLangToggle_Click(object sender, RoutedEventArgs e)
        {
            isArabic = !isArabic;
            ApplyLanguage();
        }

        private void ChipPurpose_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                if (clickedButton.Parent is UniformGrid parentGrid)
                {
                    foreach (var child in parentGrid.Children)
                    {
                        if (child is Button btn)
                        {
                            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                            btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
                        }
                    }
                }

                clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D6EFD"));
                clickedButton.Foreground = Brushes.White;
                _workflowService.SelectedPurpose = clickedButton.Content?.ToString() ?? "اجتماع عمل";
            }
        }

        private void BtnViewPersonalData_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = _localizationService.GetText("PrivacyPortalAlertTitle", isArabic);
            if (string.IsNullOrWhiteSpace(validationTitle)) validationTitle = isArabic ? "بوابة الخصوصية" : "Privacy Portal";

            string portalMsg = _localizationService.GetText("PrivacyPortalRedirectMsg", isArabic);
            if (string.IsNullOrWhiteSpace(portalMsg)) portalMsg = isArabic ? "[محاكاة - الالتزام بالنظام معلق]\n\nسيتم توجيهك بأمان إلى بوابة الويب التابعة لـ (عين) للتحقق من هويتك عبر الـ OTP واستعراض بياناتك بشكل محمي." : "[Simulated - Backend integration pending]\n\nRedirecting to the secure web portal for identity verification via OTP to review personal data.";

            MessageBox.Show(portalMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExercisePrivacyRights_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = _localizationService.GetText("PrivacyPortalAlertTitle", isArabic);
            if (string.IsNullOrWhiteSpace(validationTitle)) validationTitle = isArabic ? "بوابة الحقوق" : "Privacy Rights";

            string rightsMsg = _localizationService.GetText("PrivacyRightsRedirectMsg", isArabic);
            if (string.IsNullOrWhiteSpace(rightsMsg)) rightsMsg = isArabic ? "[محاكاة - الالتزام بالنظام معلق]\n\nسيتم توجيهك إلى بوابة حقوق حماية البيانات لتقديم طلب رسمي ومتابعته." : "[Simulated - Backend integration pending]\n\nRedirecting to the data protection portal to exercise your rights.";

            MessageBox.Show(rightsMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void KioskTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string tabTipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), @"Microsoft Shared\ink\TabTip.exe");
                if (File.Exists(tabTipPath))
                {
                    Process.Start(new ProcessStartInfo { FileName = tabTipPath, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Touch Keyboard Fault] OS interaction layout block: {ex.Message}");
            }
        }

        private void BtnNextToPrivacy_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = _localizationService.GetText("KioskAlertTitle", isArabic);
            if (string.IsNullOrWhiteSpace(validationTitle)) validationTitle = isArabic ? "تحقق المدخلات" : "Input Validation";

            if (string.IsNullOrWhiteSpace(TxtHostName.Text))
            {
                string missingHostMsg = _localizationService.GetText("MissingHostErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(missingHostMsg)) missingHostMsg = isArabic ? "يرجى كتابة اسم الشخص المضيف أولاً!" : "Please enter the host name!";

                MessageBox.Show(missingHostMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TxtEmail != null && !string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                string emailInput = TxtEmail.Text.Trim();
                try
                {
                    var addr = new System.Net.Mail.MailAddress(emailInput);
                    if (addr.Address != emailInput) throw new FormatException();
                }
                catch
                {
                    string invalidEmailMsg = _localizationService.GetText("InvalidEmailFormatErrorMsg", isArabic);
                    if (string.IsNullOrWhiteSpace(invalidEmailMsg)) invalidEmailMsg = isArabic ? "⚠️ صيغة البريد الإلكتروني غير صحيحة، يرجى التأكد منها لتسهيل عملية إرسال الـ OTP لاحقاً!" : "⚠️ Invalid email format. Please check the email address to facilitate future OTP delivery!";

                    MessageBox.Show(invalidEmailMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _workflowService.SelectedHostName = TxtHostName.Text;
            _workflowService.SelectedMobile = string.IsNullOrWhiteSpace(TxtMobileNumber.Text) ? "لم يسجل" : TxtMobileNumber.Text;

            try
            {
                _receiptToken = _receiptReferenceProvider.GetSyntheticToken();
                string localQrPayload = $"https://receipt.ain.ebtco.com/r/{_receiptToken}";
                KioskQrImage.Source = _qrCodeService.GenerateQrCodeImage(localQrPayload);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QR Engine Error] Local QR bitmap compilation exception: {ex.Message}");

                string qrErrorMsg = _localizationService.GetText("LocalQrGenerationErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(qrErrorMsg)) qrErrorMsg = isArabic ? "⚠️ عذراً، فشل نظام الكشك في إنشاء الرمز المربع محلياً، سيتم الاستمرار بدون عرض الـ QR." : "⚠️ Local QR code generation failed. Continuing checkout without QR code.";

                MessageBox.Show(qrErrorMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            ViewDetails.Visibility = Visibility.Collapsed;
            ViewPrivacy.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        private void BtnSupervisorTrigger_Click(object sender, RoutedEventArgs e)
        {
            ViewHome.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Collapsed;
            ViewDetails.Visibility = Visibility.Collapsed;
            ViewPrivacy.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Collapsed;
            ViewSupervisorConsole.Visibility = Visibility.Collapsed;

            ViewSupervisorAuth.Visibility = Visibility.Visible;
            TxtSupervisorPin.Focus();
        }

        private void BtnSubmitPin_Click(object sender, RoutedEventArgs e)
        {
            if (TxtSupervisorPin.Password == SupervisorPinCode)
            {
                ViewSupervisorAuth.Visibility = Visibility.Collapsed;
                ViewSupervisorConsole.Visibility = Visibility.Visible;
            }
            else
            {
                string errorTitle = _localizationService.GetText("KioskAlertTitle", isArabic);
                if (string.IsNullOrWhiteSpace(errorTitle)) errorTitle = isArabic ? "خطأ في الصلاحية" : "Authorization Fault";

                string errorMsg = _localizationService.GetText("InvalidSupervisorPinErrorMsg", isArabic);
                if (string.IsNullOrWhiteSpace(errorMsg)) errorMsg = isArabic ? "رمز الأمان المدخل غير صحيح!" : "Invalid supervisor PIN code entered!";

                MessageBox.Show(errorMsg, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                TxtSupervisorPin.Password = string.Empty;
            }
        }

        private void BtnCancelPin_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        private void BtnExitConsole_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        private void ApplyLanguage()
        {
            if (_workflowService == null || _localizationService == null || _configProvider == null || _privacyNoticeProvider == null) return;

            KioskWindow.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            string langText = _localizationService.GetText("BtnLangToggle", !isArabic);
            BtnLangToggle.Content = string.IsNullOrEmpty(langText) ? (isArabic ? "English" : "العربية") : langText;

            TxtHeaderLogo.Text = isArabic ? "عـيـن | AIN" : "AIN | EYE";
            WelcomeTitle.Text = _localizationService.GetText("WelcomeTitle", isArabic);
            WelcomeSubtitle.Text = _localizationService.GetText("WelcomeSubtitle", isArabic);
            BtnPreRegistered.Content = _localizationService.GetText("BtnPreRegistered", isArabic);
            BtnWalkIn.Content = _localizationService.GetText("BtnWalkIn", isArabic);
            BtnHelp.Content = _localizationService.GetText("BtnHelp", isArabic);

            TxtSupervisorTitle.Text = isArabic ? "بوابة المشرف الأمنية" : "Supervisor Security Gateway";
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
            TxtRetroDocNumber.Tag = isArabic ? "رقم الهوية أو جواز السفر" : "National ID or Passport number";
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

            QrLabel.Text = _localizationService.GetText("QrLabel", isArabic);
            if (!_workflowService.IsInErrorState) BtnAcceptPrint.Content = _localizationService.GetText("BtnAcceptPrint", isArabic);
            BtnDoneReceipt.Content = _localizationService.GetText("BtnDoneReceipt", isArabic);

            TxtRetroTitle.Text = isArabic ? "تسجيل زائر بأثر رجعي" : "Retroactive Entry";
            TxtRetroWarning.Text = _localizationService.GetText("TxtRetroWarning", isArabic);
            LblRetroVisName.Text = _localizationService.GetText("LblRetroVisName", isArabic);
            LblRetroDocNum.Text = _localizationService.GetText("LblRetroDocNum", isArabic);
            LblRetroHost.Text = _localizationService.GetText("LblRetroHost", isArabic);
            LblRetroPurpose.Text = _localizationService.GetText("LblRetroPurpose", isArabic);
            ChkRetroAttestation.Content = _localizationService.GetText("ChkRetroAttestation", isArabic);
            BtnSubmitRetroactive.Content = _localizationService.GetText("BtnSubmitRetroactive", isArabic);
            BtnCancelRetroactive.Content = _localizationService.GetText("BtnCancelRetroactive", isArabic);

            if (_workflowService.CurrentFlow == "WalkIn")
            {
                PrivacyTitle.Text = _localizationService.GetText("PrivacyTitleWalkIn", isArabic);
                string rawBody = _localizationService.GetText("PrivacyBodyWalkIn", isArabic);
                PrivacyBody.Text = rawBody
                    .Replace("[TenantName]", _configProvider.TenantName)
                    .Replace("[PrivacyContact]", _configProvider.PrivacyContact)
                    .Replace("[RetentionStatement]", _privacyNoticeProvider.GetRetentionStatement(isArabic));
            }
            else
            {
                PrivacyTitle.Text = _localizationService.GetText("PrivacyTitleDefault", isArabic);
                string rawBody = _localizationService.GetText("PrivacyBodyDefault", isArabic);
                PrivacyBody.Text = rawBody
                    .Replace("[TenantName]", _configProvider.TenantName)
                    .Replace("[PrivacyContact]", _configProvider.PrivacyContact)
                    .Replace("[RetentionStatement]", _privacyNoticeProvider.GetRetentionStatement(isArabic));
            }
        }
    }
}