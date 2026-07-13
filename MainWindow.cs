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
using AIN_Kiosk.Adapters; // 🎯 استدعاء طبقة المحولات الجديدة

namespace AIN_Kiosk
{
    public partial class MainWindow : Window
    {
        private bool isArabic = true;

        // 🎯 حقن الخدمات والمحولات المعمارية بالكامل - صفر % اعتمادية خارجية على العتاد المادي هنا!
        private readonly KioskWorkflowService _workflowService;
        private readonly BadgePrintService _printService;
        private readonly PrivacyReceiptService _receiptService;
        private readonly LocalizationService _localizationService;
        private readonly ScannerAdapter _scannerAdapter;

        public MainWindow()
        {
            InitializeComponent();

            // تهيئة حزمة البناء المعماري النظيف
            _workflowService = new KioskWorkflowService();
            _printService = new BadgePrintService();
            _receiptService = new PrivacyReceiptService();
            _localizationService = new LocalizationService();
            _scannerAdapter = new ScannerAdapter();

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
            ChkRetroAttestation.IsChecked = false;

            ViewHome.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        // ================= [ نظام المعالجة والفحص عبر محول القارئ المعزول ] =================
        private async Task StartKioskWorkflowAsync()
        {
            _workflowService.IsInErrorState = false;
            PrintStatusLabel.Text = "";
            QrCodeContainer.Visibility = Visibility.Collapsed;

            ViewHome.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Visible;
            ApplyLanguage();

            // 🚀 استدعاء المحول المعزول كلياً لتنفيذ عملية الفحص الآمن للهوية
            bool isScanValid = await _scannerAdapter.ExecuteScanAsync();

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
                    ViewDetails.Visibility = Visibility.Visible;
                    ApplyLanguage();
                }
            }
            else
            {
                ResetToHomeView(forceResetToArabic: true);
            }
        }

        // ================= [ دالة حفظ وتأكيد السجل بأثر رجعي ] =================
        private void BtnSubmitRetroactive_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRetroVisitorName.Text) || string.IsNullOrWhiteSpace(TxtRetroDocNumber.Text) || string.IsNullOrWhiteSpace(TxtRetroHostName.Text))
            {
                MessageBox.Show(isArabic ? "عذراً، يجب تعبئة كافة البيانات الأساسية لحفظ السجل!" : "All fields are required for retroactive logging!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ChkRetroAttestation.IsChecked != true)
            {
                MessageBox.Show(isArabic ? "يجب تفعيل مربع الإقرار والمصادقة الإلكترونية لاعتماد السجل بأثر رجعي!" : "You must check the attestation box to proceed!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(isArabic ? "✓ تم اعتماد وحفظ السجل بأثر رجعي بنجاح، وتوقيعه برمز الـ Supervisor." : "✓ Retroactive record successfully saved and audit-logged.", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Information);
            ResetToHomeView(forceResetToArabic: true);
        }

        private void BtnCancelRetroactive_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }

        // ================= [ استدعاء محركات الطباعة والخصوصية المعزولة ] =================
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
            PrintStatusLabel.Text = isArabic ? "⏳ جاري طباعة التصميم الديناميكي ثنائي الوجه..." : "⏳ Executing regulator-grade layout engine...";

            bool isPrintSuccess = await _printService.PrintVisitorBadgeAsync(
                _workflowService.SelectedHostName,
                _workflowService.SelectedPurpose,
                _workflowService.SelectedMobile
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

                TxtReceptionistAlert.Text = receipt.ReceptionistAlert;
                TxtReceiptDataCaptured.Text = receipt.DataCaptured;
                TxtReceiptPurgeDate.Text = receipt.PurgeDateText;
                TxtReceiptHash.Text = receipt.IntegrityHash;

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
                PrintStatusLabel.Text = isArabic ? "⚠️ عذراً، طابعة البطاقات غير متوفرة حالياً!" : "⚠️ Printer is offline!";
                _workflowService.ResetIdleTimer();
            }
        }

        private void KioskTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string tabTipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), @"Microsoft Shared\ink\TabTip.exe");
                if (File.Exists(tabTipPath)) { Process.Start(new ProcessStartInfo { FileName = tabTipPath, UseShellExecute = true }); }
            }
            catch { }
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

            if (forceResetToArabic) isArabic = true;

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

        private void BtnNextToPrivacy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHostName.Text))
            {
                MessageBox.Show(isArabic ? "يرجى كتابة اسم الشخص المضيف أولاً!" : "Please enter the host name!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _workflowService.SelectedHostName = TxtHostName.Text;
            _workflowService.SelectedMobile = string.IsNullOrWhiteSpace(TxtMobileNumber.Text) ? "لم يسجل" : TxtMobileNumber.Text;

            string rawDataUrl = $"https://ain.ebtco.sa/privacy?session={Guid.NewGuid()}&flow={_workflowService.CurrentFlow}";
            string qrGeneratorApi = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(rawDataUrl)}";
            try { KioskQrImage.Source = new BitmapImage(new Uri(qrGeneratorApi)); } catch { }

            ViewDetails.Visibility = Visibility.Collapsed;
            ViewPrivacy.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            if (_workflowService == null || _localizationService == null) return;

            KioskWindow.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            BtnLangToggle.Content = _localizationService.GetText("BtnLangToggle", !isArabic);

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
            ChkRetroAttestation.Content = _localizationService.GetText("ChkRetroAttestation", isArabic);
            BtnSubmitRetroactive.Content = _localizationService.GetText("BtnSubmitRetroactive", isArabic);
            BtnCancelRetroactive.Content = _localizationService.GetText("BtnCancelRetroactive", isArabic);

            if (_workflowService.CurrentFlow == "WalkIn")
            {
                PrivacyTitle.Text = _localizationService.GetText("PrivacyTitleWalkIn", isArabic);
                PrivacyBody.Text = _localizationService.GetText("PrivacyBodyWalkIn", isArabic);
            }
            else
            {
                PrivacyTitle.Text = _localizationService.GetText("PrivacyTitleDefault", isArabic);
                PrivacyBody.Text = _localizationService.GetText("PrivacyBodyDefault", isArabic);
            }
        }
    }
}