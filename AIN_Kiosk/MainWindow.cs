using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AIN_Kiosk.Services;
using AIN_Kiosk.Adapters;

namespace AIN_Kiosk
{
    public partial class MainWindow : Window
    {
        private bool isArabic = true;
        private string _receiptToken = string.Empty;

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
            _configProvider = new LocalMockKioskConfigurationProvider();
            _privacyNoticeProvider = new LocalMockKioskPrivacyNoticeProvider();
            _registrationQueue = new MockRegistrationQueue();
            _receiptReferenceProvider = new MockReceiptReferenceProvider();

            _workflowService.OnIdleTimeout += () =>
            {
                ResetToHomeView(forceResetToArabic: true);
            };

            ApplyLanguage();
            InitializeIdleTimer();
        }

        // Dynamic validator for supervisor prototype gate PIN
        private bool ValidateSupervisorPin(string enteredPin)
        {
            return enteredPin == _configProvider.SupervisorDemoPin;
        }

        #region Input Rules & Validation Rules

        private void Input_OnlyLetters_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z\u0600-\u06FF\s\-']+$");
        }

        private void Input_OnlyDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        // Item 10: Support alphanumeric input for passport document numbers
        private void Input_Alphanumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        #endregion

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
    }
}