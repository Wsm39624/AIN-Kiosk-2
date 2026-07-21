using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AIN_Kiosk
{
    public partial class MainWindow
    {
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
            string localQrPayload = $"https://receipt.ain.ebtco.com/r/{_receiptToken}";

            try
            {
                KioskQrImage.Source = _qrCodeService.GenerateBitmap(localQrPayload);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Local QR generation failed: {ex.Message}");
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

        private void BtnDoneReceipt_Click(object sender, RoutedEventArgs e)
        {
            ResetToHomeView(forceResetToArabic: true);
        }
    }
}