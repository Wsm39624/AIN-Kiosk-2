using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

        #region Input Rules & Validation Rules

        private void Input_OnlyLetters_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z\u0600-\u06FF\s]+$");
        }

        private void Input_OnlyDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        #endregion

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

        private void BtnRetroactiveEntry_Click(object sender, RoutedEventArgs e)
        {
            _workflowService.CurrentFlow = "Retroactive";

            TxtRetroVisitorName.Text = string.Empty;
            TxtRetroDocNumber.Text = string.Empty;
            TxtRetroHostName.Text = string.Empty;
            ChkRetroAttestation.IsChecked = false;

            ViewSupervisorConsole.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

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
                Debug.WriteLine($"[Scanner Fault] Scanner exception: {ex.Message}");

                string errorTitle = isArabic ? "تنبيه النظام" : "System Alert";
                string errorMsg = isArabic ? "حدث خطأ فني غير متوقع في قارئ الهوية." : "An unexpected scanner error occurred.";

                MessageBox.Show(errorMsg, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (isScanValid)
            {
                ViewScan.Visibility = Visibility.Collapsed;

                if (_workflowService.CurrentFlow == "WalkIn")
                {
                    _workflowService.SelectedHostName = isArabic ? "قيد التدقيق" : "Pending Queue";
                    _workflowService.SelectedPurpose = isArabic ? "زيارة بدون موعد" : "Walk-In Guest";
                    _workflowService.SelectedMobile = isArabic ? "غير مسجل" : "Not Provided";

                    ViewPrivacy.Visibility = Visibility.Visible;
                    ApplyLanguage();
                }
                else
                {
                    TxtHostName.Text = string.Empty;
                    TxtMobileNumber.Text = string.Empty;
                    if (TxtEmail != null) TxtEmail.Text = string.Empty;

                    ViewDetails.Visibility = Visibility.Visible;
                    ApplyLanguage();
                }
            }
            else
            {
                ResetToHomeView(forceResetToArabic: true);
            }
        }

        private void BtnSubmitRetroactive_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = isArabic ? "تحقق المدخلات" : "Validation Alert";

            if (string.IsNullOrWhiteSpace(TxtRetroVisitorName.Text) ||
                string.IsNullOrWhiteSpace(TxtRetroDocNumber.Text) ||
                string.IsNullOrWhiteSpace(TxtRetroHostName.Text))
            {
                string msg = isArabic ? "عذراً، يجب تعبئة جميع الحقول الأساسية!" : "All required fields must be completed!";
                MessageBox.Show(msg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TxtRetroDocNumber.Text.Trim().Length != 10)
            {
                string msg = isArabic ? "رقم الهوية / الوثيقة يجب أن يتكون من 10 أرقام بالضبط!" : "National ID / Document number must be exactly 10 digits!";
                MessageBox.Show(msg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ChkRetroAttestation.IsChecked != true)
            {
                string msg = isArabic ? "يجب الموافقة على الإقرار والتعهد لإكمال التسجيل!" : "You must check the attestation box to proceed!";
                MessageBox.Show(msg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _registrationQueue.EnqueueMockRecord(_workflowService.CurrentFlow, TxtRetroVisitorName.Text);

            string completionMsg = isArabic
                ? "[محاكاة واجهة - لم يتم الحفظ بالخلفية]\nتم تأكيد نموذج السجل بأثر رجعي محلياً بنجاح."
                : "[UI Simulation - Not persisted]\nRetroactive record submitted locally.";

            MessageBox.Show(completionMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Information);

            ResetToHomeView(forceResetToArabic: true);
        }

        private void BtnCancelRetroactive_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

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
            PrintStatusLabel.Text = isArabic ? "[محاكاة] جاري معالجة طباعة البطاقة..." : "[Simulation] Processing badge print...";

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

                UpdateReceiptLanguageTexts();

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
                PrintStatusLabel.Text = isArabic ? "طابعة البطاقات غير متوفرة محلياً (Printer Fallback Demo)." : "Printer fallback demo: printer is offline.";

                _workflowService.ResetIdleTimer();
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
                TxtReceiptPurgeDate.Text = $"الأساس القانوني: نظام حماية البيانات الشخصية (PDPL)\nفترة الاحتفاظ المحددة: {_privacyNoticeProvider.GetRetentionStatement(true)}";
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
        }

        private void BtnDoneReceipt_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

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
            if (TxtEmail != null) TxtEmail.Text = string.Empty;

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
            string validationTitle = isArabic ? "بوابة الخصوصية" : "Privacy Portal";
            string portalMsg = isArabic
                ? "[محاكاة توجيه]\nسيتم توجيهك إلى بوابة الويب الآمنة للتحقق من هويتك واستعراض البيانات."
                : "[Simulated Redirect]\nRedirecting to the secure web portal to review personal data.";

            MessageBox.Show(portalMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExercisePrivacyRights_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = isArabic ? "بوابة الحقوق" : "Privacy Rights";
            string rightsMsg = isArabic
                ? "[محاكاة توجيه]\nسيتم توجيهك إلى بوابة حقوق حماية البيانات لتقديم طلب رسمي."
                : "[Simulated Redirect]\nRedirecting to the data protection portal to exercise your rights.";

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
                Debug.WriteLine($"[Touch Keyboard Fault] {ex.Message}");
            }
        }

        private void BtnNextToPrivacy_Click(object sender, RoutedEventArgs e)
        {
            string validationTitle = isArabic ? "تحقق المدخلات" : "Input Validation";

            if (string.IsNullOrWhiteSpace(TxtHostName.Text))
            {
                string msg = isArabic ? "يرجى كتابة اسم الشخص المضيف أولاً!" : "Please enter the host name!";
                MessageBox.Show(msg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(TxtMobileNumber.Text))
            {
                string mob = TxtMobileNumber.Text.Trim();
                if (mob.Length != 10 || !mob.StartsWith("05"))
                {
                    string invalidMobMsg = isArabic ? "رقم الجوال يجب أن يتكون من 10 أرقام ويبدأ بـ 05 (مثال: 0512345678)!" : "Mobile number must be 10 digits and start with 05 (e.g., 0512345678)!";
                    MessageBox.Show(invalidMobMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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
                    string invalidEmailMsg = isArabic ? "صيغة البريد الإلكتروني غير صحيحة!" : "Invalid email format!";
                    MessageBox.Show(invalidEmailMsg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _workflowService.SelectedHostName = TxtHostName.Text.Trim();
            _workflowService.SelectedMobile = string.IsNullOrWhiteSpace(TxtMobileNumber.Text) ? (isArabic ? "غير مسجل" : "Not Provided") : TxtMobileNumber.Text.Trim();

            _receiptToken = Guid.NewGuid().ToString("N").ToUpperInvariant();

            // 1. إنشاء التوكن التجريبي المباشر
            _receiptToken = Guid.NewGuid().ToString("N").ToUpperInvariant();

            // 2. الصيغة المعتمدة نهائياً حسب الإيميل الرابع (رابط استعراض الإيصال)
            string localQrPayload = $"https://receipt.ain.ebtco.com/r/{_receiptToken}";

            try
            {
                KioskQrImage.Source = _qrCodeService.GenerateBitmap(localQrPayload);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Local QR generation failed: {ex.Message}");
                KioskQrImage.Source = null;

                MessageBox.Show(
                    isArabic
                        ? "تعذر إنشاء رمز الاستجابة السريعة محلياً."
                        : "The QR code could not be generated locally.",
                    "AIN Kiosk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
            ApplyLanguage();
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
                string errorTitle = isArabic ? "خطأ في الصلاحية" : "Authorization Fault";
                string errorMsg = isArabic ? "رمز الأمان المدخل غير صحيح!" : "Invalid supervisor PIN code entered!";

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

            if (BtnViewPersonalData != null) BtnViewPersonalData.Content = isArabic ? "عرض بياناتي الشخصية" : "View My Personal Data";
            if (BtnExercisePrivacyRights != null) BtnExercisePrivacyRights.Content = isArabic ? "ممارسة حقوق الخصوصية" : "Exercise Privacy Rights";

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
    }
}