using System;
using System.Windows;
using AIN_Kiosk.Helpers;

namespace AIN_Kiosk
{
    // Partial class handling supervisor prototype gate authentication and retroactive record entries
    public partial class MainWindow
    {
        private int _supervisorFailedAttempts = 0;
        private DateTime? _supervisorLockoutExpiry = null;
        private const int MaxSupervisorAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(30);

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
            string gateTitle = isArabic
                ? "بوابة المشرف التجريبية - ليست مصادقة إنتاجية"
                : "Prototype Supervisor Gate — Not Production Authentication";

            // Enforce temporary lockout if failed attempt threshold was reached
            if (_supervisorLockoutExpiry.HasValue && DateTime.UtcNow < _supervisorLockoutExpiry.Value)
            {
                TimeSpan remaining = _supervisorLockoutExpiry.Value - DateTime.UtcNow;
                string lockoutMsg = isArabic
                    ? $"[Prototype Supervisor Gate — Not Production Authentication]\nالبوابة مقفلة مؤقتاً. يرجى الانتظار {Math.Ceiling(remaining.TotalSeconds)} ثانية."
                    : $"[Prototype Supervisor Gate — Not Production Authentication]\nAccess temporarily locked. Please wait {Math.Ceiling(remaining.TotalSeconds)} seconds.";

                MessageBox.Show(lockoutMsg, gateTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSupervisorPin.Password = string.Empty;
                return;
            }

            string enteredPin = TxtSupervisorPin.Password ?? string.Empty;

            // Always clear PIN input after submission attempt for security
            TxtSupervisorPin.Password = string.Empty;

            if (ValidateSupervisorPin(enteredPin))
            {
                _supervisorFailedAttempts = 0;
                _supervisorLockoutExpiry = null;

                ViewSupervisorAuth.Visibility = Visibility.Collapsed;
                ViewSupervisorConsole.Visibility = Visibility.Visible;
            }
            else
            {
                _supervisorFailedAttempts++;

                if (_supervisorFailedAttempts >= MaxSupervisorAttempts)
                {
                    _supervisorLockoutExpiry = DateTime.UtcNow.Add(LockoutDuration);
                    string lockoutMsg = isArabic
                        ? $"[Prototype Supervisor Gate — Not Production Authentication]\nتم تجاوز عدد المحاولات المسموح بها. تم قفل البوابة مؤقتاً لمدة {LockoutDuration.TotalSeconds} ثانية."
                        : $"[Prototype Supervisor Gate — Not Production Authentication]\nMaximum failed attempts exceeded. Access locked for {LockoutDuration.TotalSeconds} seconds.";

                    MessageBox.Show(lockoutMsg, gateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    int remainingAttempts = MaxSupervisorAttempts - _supervisorFailedAttempts;
                    string errorMsg = isArabic
                        ? $"[Prototype Supervisor Gate — Not Production Authentication]\nرمز الأمان غير صحيح. المحاولات المتبقية: {remainingAttempts}"
                        : $"[Prototype Supervisor Gate — Not Production Authentication]\nInvalid PIN code. Remaining attempts: {remainingAttempts}";

                    MessageBox.Show(errorMsg, gateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancelPin_Click(object sender, RoutedEventArgs e)
        {
            TxtSupervisorPin.Password = string.Empty;
            ResetToHomeView(forceResetToArabic: true);
        }

        private void BtnExitConsole_Click(object sender, RoutedEventArgs e)
        {
            TxtSupervisorPin.Password = string.Empty;
            ResetToHomeView(forceResetToArabic: true);
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

            string docNum = TxtRetroDocNumber.Text.Trim();
            string selectedDocType = "national_id";
            if (!DocumentValidationHelper.IsValidDocument(docNum, selectedDocType, "SA"))
            {
                string msg = isArabic
                    ? "رقم الوثيقة/الهوية غير صحيح بالنسبة للنوع المحدد!"
                    : "Invalid document number for the selected type!";
                MessageBox.Show(msg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ChkRetroAttestation.IsChecked != true)
            {
                string msg = isArabic ? "يجب الموافقة على الإقرار والتعهد لإكمال التسجيل!" : "You must check the attestation box to proceed!";
                MessageBox.Show(msg, validationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Generate synthetic correlation reference for audit queue logging to protect visitor PII
            string syntheticSessionRef = $"RETRO-{Guid.NewGuid():N}"[..10].ToUpperInvariant();
            _registrationQueue.EnqueueMockRecord(_workflowService.CurrentFlow, syntheticSessionRef);

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
    }
}