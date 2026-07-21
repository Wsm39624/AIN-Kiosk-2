using System.Windows;

namespace AIN_Kiosk
{
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
    }
}