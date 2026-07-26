using System.Windows;

namespace AIN_Kiosk
{
    /// <summary>
    /// Partial class handling supervisor console authentication and retroactive record entries.
    /// Addresses Task #7 (Prototype Supervisor Gate labeling) and Task #10 (Flexible document length validation).
    /// </summary>
    public partial class MainWindow
    {
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
            // Task #7: Labeled explicitly as a Prototype Supervisor Gate (Not Production Authentication)
            if (TxtSupervisorPin.Password == SupervisorPinCode)
            {
                ViewSupervisorAuth.Visibility = Visibility.Collapsed;
                ViewSupervisorConsole.Visibility = Visibility.Visible;
            }
            else
            {
                string errorTitle = isArabic ? "بوابة المشرف التجريبية" : "Prototype Supervisor Gate";
                string errorMsg = isArabic
                    ? "[Prototype Supervisor Gate Not Production Authentication]\nرمز الأمان المدخل غير صحيح!"
                    : "[Prototype Supervisor Gate Not Production Authentication]\nInvalid supervisor PIN code entered!";

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

            // Task #10: Corrected document validation to allow varied document/passport lengths (5 to 20 characters)
            string docNum = TxtRetroDocNumber.Text.Trim();
            if (docNum.Length < 5 || docNum.Length > 20)
            {
                string msg = isArabic
                    ? "رقم الوثيقة غير صحيح (يجب أن يكون بين 5 إلى 20 خانة بحسب نوع الوثيقة)!"
                    : "Invalid document number! Length must be between 5 and 20 characters based on document type.";
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
    }
}