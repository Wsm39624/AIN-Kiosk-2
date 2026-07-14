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

        // توكن الجلسة الموحد والمبهم لحماية الخصوصية
        private string _receiptToken = string.Empty;

        // حقن الخدمات والمحولات المعمارية المعتمدة
        private readonly KioskWorkflowService _workflowService;
        private readonly BadgePrintService _printService;
        private readonly PrivacyReceiptService _receiptService;
        private readonly LocalizationService _localizationService;
        private readonly ScannerAdapter _scannerAdapter;
        private readonly LocalQrCodeService _qrCodeService;

        // موفرات الإعدادات والخصوصية لمنع الـ Hardcoding
        private readonly KioskConfigurationProvider _configProvider;
        private readonly KioskPrivacyNoticeProvider _privacyNoticeProvider;

        public MainWindow()
        {
            InitializeComponent();

            // تهيئة حزمة الخدمات المعزولة
            _workflowService = new KioskWorkflowService();
            _printService = new BadgePrintService();
            _receiptService = new PrivacyReceiptService();
            _localizationService = new LocalizationService();
            _scannerAdapter = new ScannerAdapter();
            _qrCodeService = new LocalQrCodeService();
            _configProvider = new KioskConfigurationProvider();
            _privacyNoticeProvider = new KioskPrivacyNoticeProvider();

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

        private void BtnRetroactive_Click(object sender, RoutedEventArgs e)
        {
            _workflowService.CurrentFlow = "Retroactive";

            TxtRetroVisitorName.Text = "";
            TxtRetroDocNumber.Text = "";
            TxtRetroHostName.Text = "Wesam Mohammed";

            // 🔒 تصحيح اسم العنصر البرمجي لمطابقة الـ XAML
            ChkRetroAttestation.IsChecked = false;

            ViewHome.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        // ================= [ نظام المعالجة والفحص الرقمي ] =================
        private async Task StartKioskWorkflowAsync()
        {
            _workflowService.IsInErrorState = false;
            PrintStatusLabel.Text = "";
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
                Debug.WriteLine($"[Scanner Error] Scan failed to execute safely: {ex.Message}");
                MessageBox.Show(
                    isArabic ? "⚠️ حدث خطأ فني غير متوقع في جهاز قارئ الهوية، يرجى المحاولة لاحقاً."
                             : "⚠️ An unexpected scanner error occurred. Please try again later.",
                    "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Error
                );
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
                    TxtMobileNumber.Text = "";

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

        // ================= [ دالة السجل بأثر رجعي - الأمانة الهندسية ] =================
        private void BtnSubmitRetroactive_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRetroVisitorName.Text) || string.IsNullOrWhiteSpace(TxtRetroDocNumber.Text) || string.IsNullOrWhiteSpace(TxtRetroHostName.Text))
            {
                MessageBox.Show(isArabic ? "عذراً، يجب تعبئة كافة البيانات الأساسية لحفظ السجل!" : "All fields are required for retroactive logging!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔒 تصحيح اسم العنصر البرمجي لمطابقة الـ XAML
            if (ChkRetroAttestation.IsChecked != true)
            {
                MessageBox.Show(isArabic ? "يجب تفعيل مربع الإقرار والمصادقة الإلكترونية لاعتماد السجل بأثر رجعي!" : "You must check the attestation box to proceed!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                isArabic ? "✓ [محاكاة - لم يتم الحفظ] تم تأكيد السجل بأثر رجعي محلياً بنجاح.\n\n⚠️ تنبيه: الربط الفعلي مع الخلفية معلق (Backend integration pending)."
                         : "✓ [Simulated - not persisted] Retroactive record confirmed locally.\n\n⚠️ Notice: Backend integration pending.",
                "AIN Kiosk - Prototype Status", MessageBoxButton.OK, MessageBoxImage.Information
            );
            ResetToHomeView(forceResetToArabic: true);
        }

        private void BtnCancelRetroactive_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        // ================= [ محركات الطباعة والخصوصية المعزولة ] =================
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
            PrintStatusLabel.Text = isArabic ? "⏳ جاري محاكاة طباعة التصميم ثنائي الوجه (Zebra Printer Emulator)..." : "⏳ Running Zebra printer fallback emulator...";

            bool isPrintSuccess = await _printService.PrintVisitorBadgeAsync(
                _workflowService.SelectedHostName,
                _workflowService.SelectedPurpose,
                _workflowService.SelectedMobile,
                _receiptToken
            );

            if (isPrintSuccess)
            {
                PrintStatusLabel.Text = "";
                ScrollPrivacyText.Visibility = Visibility.Collapsed;
                BtnAcceptPrint.Visibility = Visibility.Collapsed;

                PanelReceiptDetails.Visibility = Visibility.Visible;
                QrCodeContainer.Visibility = Visibility.Visible;
                BtnDoneReceipt.Visibility = Visibility.Visible;
                BorderReceptionistAlert.Visibility = Visibility.Visible;

                ReceiptDetails receipt = _receiptService.GenerateReceipt(
                    _workflowService.CurrentFlow,
                    _workflowService.SelectedHostName,
                    _workflowService.SelectedPurpose,
                    isArabic
                );

                string secureHash = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 16);

                if (isArabic)
                {
                    TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                        ? "🔔 [إشعار استقبال وهمي - لم يُرسل] تنبيه الاستقبال السريع: تم رصد حركة دخول زائر بدون موعد، بانتظار استكمال البيانات بالخلفية."
                        : $"🔔 [إشعار استقبال وهمي - لم يُرسل] تنبيه الاستقبال: الزائر (Wesam Mohammed) في انتظار المضيف ({_workflowService.SelectedHostName}) حالياً.";

                    TxtReceiptDataCaptured.Text = $"• [محاكاة - لم يتم الحفظ] البيانات الشخصية: Wesam Mohammed | الجهة: {_configProvider.TenantName}\n• الشخص المضيف: {_workflowService.SelectedHostName}\n• غرض الزيارة: {_workflowService.SelectedPurpose}";
                    TxtReceiptPurgeDate.Text = $"⚠️ [قيمة اختبار غير معتمدة] سيتم إتلاف سجل بياناتك الشخصية آلياً بعد: {_privacyNoticeProvider.GetRetentionStatement(true)}";
                    TxtReceiptHash.Text = $"رمز سلامة معاينة الإيصال (ليس دليلاً للتدقيق): AIN-SEC-{secureHash} | Token: {_receiptToken}";
                }
                else
                {
                    TxtReceptionistAlert.Text = _workflowService.CurrentFlow == "WalkIn"
                        ? "🔔 [Mock notification not sent] Quick Reception Alert: Unscheduled visitor check-in recorded. Pending back-fill completion."
                        : $"🔔 [Mock notification not sent] Reception Alert: Visitor (Wesam Mohammed) has arrived and is waiting for Host ({_workflowService.SelectedHostName}).";

                    TxtReceiptDataCaptured.Text = $"• [Simulated - not persisted] Personal Data: Wesam Mohammed | Org: {_configProvider.TenantName}\n• Host Person: {_workflowService.SelectedHostName}\n• Approved Purpose: {_workflowService.SelectedPurpose}";
                    TxtReceiptPurgeDate.Text = $"⚠️ [Unapproved test value] Personal data record will be automatically purged after: {_privacyNoticeProvider.GetRetentionStatement(false)}";
                    TxtReceiptHash.Text = $"Mock Integrity Hash (Receipt preview - not audit evidence): AIN-SEC-{secureHash} | Token: {_receiptToken}";
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
                PrintStatusLabel.Text = isArabic ? "⚠️ عذراً، محاكاة طابعة البطاقات غير متوفرة حالياً (Printer fallback demo)!" : "⚠️ Printer fallback demo: printer is offline!";
                _workflowService.ResetIdleTimer();
            }
        }

        private void BtnDoneReceipt_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        // ================= [ ميكانيكية إعادة الضبط الشاملة ] =================
        private void ResetToHomeView(bool forceResetToArabic = false)
        {
            ViewPrivacy.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Collapsed;
            ViewDetails.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Collapsed;
            ViewHome.Visibility = Visibility.Visible;

            BtnAcceptPrint.IsEnabled = true;
            PrintStatusLabel.Text = "";
            QrCodeContainer.Visibility = Visibility.Collapsed;
            BorderReceptionistAlert.Visibility = Visibility.Collapsed;
            ScrollPrivacyText.Visibility = Visibility.Visible;
            BtnAcceptPrint.Visibility = Visibility.Visible;
            PanelReceiptDetails.Visibility = Visibility.Collapsed;
            BtnDoneReceipt.Visibility = Visibility.Collapsed;

            TxtHostName.Text = "";
            TxtMobileNumber.Text = "";
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

        // ================= [ أزرار التحكم والوصول الآمن لبيانات الويب ] =================
        private void BtnViewPersonalData_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                isArabic ? "[محاكاة - الالتزام بالنظام معلق]\n\nسيتم توجيهك بأمان إلى بوابة الويب التابعة لـ (عين) للتحقق من هويتك عبر الـ OTP واستعراض بياناتك بشكل محمي."
                         : "[Simulated - Backend integration pending]\n\nRedirecting to the secure web portal for identity verification via OTP to review personal data.",
                "AIN Kiosk - Privacy Portal", MessageBoxButton.OK, MessageBoxImage.Information
            );
        }

        private void BtnExercisePrivacyRights_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                isArabic ? "[محاكاة - الالتزام بالنظام معلق]\n\nسيتم توجيهك إلى بوابة حقوق حماية البيانات لتقديم طلب رسمي ومتابعته."
                         : "[Simulated - Backend integration pending]\n\nRedirecting to the data protection portal to exercise your rights.",
                "AIN Kiosk - Privacy Rights Portal", MessageBoxButton.OK, MessageBoxImage.Information
            );
        }

        // ================= [ دالة لوحة المفاتيح والتركيز التلقائي ] =================
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
                Debug.WriteLine($"[Touch Keyboard Error] Failed to initialize OS keyboard utility: {ex.Message}");
            }
        }

        // ================= [ التحقق وصياغة الـ QR المحلي الخالي من الـ PII ] =================
        private void BtnNextToPrivacy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHostName.Text))
            {
                MessageBox.Show(isArabic ? "يرجى كتابة اسم الشخص المضيف أولاً!" : "Please enter the host name!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show(
                        isArabic ? "⚠️ صيغة البريد الإلكتروني غير صحيحة، يرجى التأكد منها لتسهيل عملية إرسال الـ OTP لاحقاً!"
                                 : "⚠️ Invalid email format. Please check the email address to facilitate future OTP delivery!",
                        "AIN Kiosk - Validation", MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                    return;
                }
            }

            _workflowService.SelectedHostName = TxtHostName.Text;
            _workflowService.SelectedMobile = string.IsNullOrWhiteSpace(TxtMobileNumber.Text) ? "لم يسجل" : TxtMobileNumber.Text;

            try
            {
                // إنشاء توكن عشوائي مبهم وتخزينه في الجلسة 
                _receiptToken = Guid.NewGuid().ToString("N").ToUpperInvariant();

                // دمج التوكن مع رابط الإيصال المعتمد
                string qrPayload = $"https://receipt.ain.ebtco.com/r/{_receiptToken}";

                // التوليد المحلي الصارم بدون إنترنت
                KioskQrImage.Source = _qrCodeService.GenerateQrCodeImage(qrPayload);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QR Engine Error] Local generation failed on render stage: {ex.Message}");
                MessageBox.Show(
                    isArabic ? "⚠️ عذراً، فشل نظام الكشك في إنشاء الرمز المربع محلياً، سيتم الاستمرار بدون عرض الـ QR."
                             : "⚠️ Local QR code generation failed. Continuing checkout without QR code.",
                    "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning
                );
            }

            ViewDetails.Visibility = Visibility.Collapsed;
            ViewPrivacy.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        // ================= [ محرك الترجمة واستبدال الرموز النائبة ديناميكياً ] =================
        private void ApplyLanguage()
        {
            if (_workflowService == null || _localizationService == null || _configProvider == null || _privacyNoticeProvider == null) return;

            KioskWindow.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // 🔒 إزالة علامة الاستفهام (?) لحظر تحذيرات المترجم حول الـ Nullability
            string langText = _localizationService.GetText("BtnLangToggle", !isArabic);
            BtnLangToggle.Content = string.IsNullOrEmpty(langText) ? (isArabic ? "English" : "العربية") : langText;

            WelcomeTitle.Text = _localizationService.GetText("WelcomeTitle", isArabic);
            WelcomeSubtitle.Text = _localizationService.GetText("WelcomeSubtitle", isArabic);
            BtnPreRegistered.Content = _localizationService.GetText("BtnPreRegistered", isArabic);
            BtnWalkIn.Content = _localizationService.GetText("BtnWalkIn", isArabic);
            BtnRetroactive.Content = _localizationService.GetText("BtnRetroactive", isArabic);
            BtnHelp.Content = _localizationService.GetText("BtnHelp", isArabic);

            ScanTitle.Text = _workflowService.CurrentFlow == "WalkIn"
                ? _localizationService.GetText("ScanTitleWalkIn", isArabic)
                : _localizationService.GetText("ScanTitlePreRegistered", isArabic);
            ScanSubtitle.Text = _localizationService.GetText("ScanSubtitle", isArabic);

            DetailsTitle.Text = _localizationService.GetText("DetailsTitle", isArabic);
            DetailsSubtitle.Text = _localizationService.GetText("DetailsSubtitle", isArabic);
            LblHostName.Text = _localizationService.GetText("LblHostName", isArabic);
            LblPurpose.Text = _localizationService.GetText("LblPurpose", isArabic);
            LblMobile.Text = _localizationService.GetText("LblMobile", isArabic);
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

            TxtRetroTitle.Text = _localizationService.GetText("TxtRetroTitle", isArabic);
            TxtRetroWarning.Text = _localizationService.GetText("TxtRetroWarning", isArabic);
            LblRetroVisName.Text = _localizationService.GetText("LblRetroVisName", isArabic);
            LblRetroDocNum.Text = _localizationService.GetText("LblRetroDocNum", isArabic);
            LblRetroHost.Text = _localizationService.GetText("LblRetroHost", isArabic);
            LblRetroPurpose.Text = _localizationService.GetText("LblRetroPurpose", isArabic);

            // 🔒 تصحيح اسم العنصر البرمجي لمطابقة الـ XAML
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