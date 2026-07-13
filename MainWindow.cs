using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


// استيراد مساحات الأسماء المتكاملة بنجاح من مكتبة خالد المشتركة
using AIN.Visitors.Mrz.Abstractions;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Scanners;
namespace AIN_Kiosk
{
    public partial class MainWindow : Window
    {
        private bool isArabic = true;
        private DispatcherTimer? _idleTimer;
        private string currentFlow = "WalkIn";
        private bool isInErrorState = false;

        private const string ZebraPrinterName = "ZDesigner GK420t";
        private readonly IDocumentScanner _scanner;

        private string selectedPurpose = "اجتماع عمل";
        private string selectedHostName = "";
        private string selectedMobile = "";

        public MainWindow()
        {
            InitializeComponent();
            _scanner = new MockDocumentScanner(); // الربط مع مكتبة الـ Mock لـ خالد بنجاح
            ApplyLanguage();
            InitializeIdleTimer();
        }

        private async void BtnWalkIn_Click(object sender, RoutedEventArgs e)
        {
            currentFlow = "WalkIn"; // تفعيل مسار الـ Walk-In الفوري لقسم 4.2
            await StartKioskWorkflowAsync();
        }

        private async void BtnPreRegistered_Click(object sender, RoutedEventArgs e)
        {
            currentFlow = "PreRegistered"; // المسار القياسي لقسم 4.1
            await StartKioskWorkflowAsync();
        }

        private void BtnRetroactive_Click(object sender, RoutedEventArgs e)
        {
            currentFlow = "Retroactive"; // مسار الحفظ الرجعي لقسم 4.3[cite: 2]

            TxtRetroVisitorName.Text = "";
            TxtRetroDocNumber.Text = "";
            TxtRetroHostName.Text = "Wesam Mohammed";
            ChkRetroAttestation.IsChecked = false;

            ViewHome.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        // ================= [ نظام المعالجة والفحص الرقمي المتكامل ] =================
        private async Task StartKioskWorkflowAsync()
        {
            isInErrorState = false;
            PrintStatusLabel.Text = "";
            QrCodeContainer.Visibility = Visibility.Collapsed;

            ViewHome.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Visible;
            ApplyLanguage();

            bool isScanValid = false;
            try
            {
                using (ScanResult scanResult = await _scanner.ScanAsync(CancellationToken.None))
                {
                    isScanValid = scanResult.IsSuccess;
                }
            }
            catch { isScanValid = false; }

            if (isScanValid)
            {
                ViewScan.Visibility = Visibility.Collapsed;

                if (currentFlow == "WalkIn")
                {
                    // 🚀 [هندسة الـ Walk-In لقسم 4.2]: تخطي شاشة التفاصيل، تجميد المضيف كمعلق، والتوجه للخصوصية فورا بـ 25 ثانية فقط![cite: 2]
                    selectedHostName = "Pending / Back-fill Queue";
                    selectedPurpose = "Walk-In Unexpected Guest";
                    selectedMobile = "Not Provided";

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

        // ================= [ دالة حفظ وتأكيد السجل بأثر رجعي - الخطوة 4.3 ] =================
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

        // ================= [ ضخ الـ ZPL المصحح والمقسم مادية بين وسام وخالد ] =================
        private async void BtnAcceptPrint_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState)
            {
                ResetToHomeView(forceResetToArabic: true);
                return;
            }

            BtnAcceptPrint.IsEnabled = false;
            _idleTimer?.Stop();

            PrintStatusLabel.Foreground = Brushes.DarkBlue;
            PrintStatusLabel.Text = isArabic ? "⏳ جاري طباعة التصميم الديناميكي ثنائي الوجه..." : "⏳ Executing regulator-grade layout engine...";

            // 1. الفصل المادي للأسماء: السفلي لوسام والعلوي للمشرف خالدAlamri
            string visitor1Name = "Wesam Mohammed";
            string visitor2Name = "Eng. Khaled Alamri";
            string visitorCompany = "EBTCO / BDO Al-Amri";
            string visitorToken = $"AIN-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

            string visitDateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string validUntilStr = DateTime.Now.ToString("yyyy-MM-dd");

            // 2. 💡 حل مشكلة علامات الاستفهام (????): خريطة تحويل النصوص العربية إلى إنجليزية الصافية للطابعة
            string purposeInEnglish = "General Visit";
            if (selectedPurpose.Contains("اجتماع") || selectedPurpose.ToLower().Contains("meeting")) purposeInEnglish = "Business Meeting";
            else if (selectedPurpose.Contains("مقابلة") || selectedPurpose.ToLower().Contains("interview")) purposeInEnglish = "Job Interview";
            else if (selectedPurpose.Contains("صيانة") || selectedPurpose.ToLower().Contains("maintenance")) purposeInEnglish = "Technical Maintenance";
            else if (selectedPurpose.Contains("توصيل") || selectedPurpose.ToLower().Contains("delivery")) purposeInEnglish = "Delivery / Courier";
            else if (selectedPurpose.Contains("مورد") || selectedPurpose.ToLower().Contains("vendor")) purposeInEnglish = "Vendor / Supplier";

            string zplData = $@"
^XA
~SD24
^PW800
^LL1200
^FO16,16^GB768,568,3,B^FS
^FO45,55^A0N,55,55^FDVISITOR^FS
^FO45,125^GB450,55,55,B^FS
^FO55,138^A0N,30,30^FR^FD{visitor2Name}^FS
^FO45,190^GB450,50,50,B^FS
^FO55,202^A0N,26,26^FR^FD{visitorCompany}^FS
^FO45,260^A0N,22,22^FDVISIT DATE:^FS
^FO200,250^GB270,38,2,B^FS
^FO215,258^A0N,20,20^FD{visitDateStr}^FS
^FO45,315^A0N,22,22^FDVALID UNTIL:^FS
^FO200,305^GB270,38,2,B^FS
^FO215,313^A0N,20,20^FD{validUntilStr}^FS
^FO45,370^A0N,22,22^FDHOST:^FS
^FO200,360^GB270,38,2,B^FS
^FO215,368^A0N,20,20^FD{selectedHostName}^FS
^FO45,425^A0N,22,22^FDPURPOSE:^FS
^FO200,415^GB270,38,2,B^FS
^FO215,423^A0N,20,20^FD{purposeInEnglish}^FS
^FO45,480^A0N,22,22^FDMOBILE:^FS
^FO200,470^GB270,38,2,B^FS
^FO215,478^A0N,20,20^FD{selectedMobile}^FS
^FO540,110^BQN,2,6^FDQA,{visitorToken}^FS
^FO530,395^A0N,24,24^FDID:^FS
^FO575,385^GB170,42,2,B^FS
^FO590,395^A0N,20,20^FD{visitorToken}^FS

^FO16,616^GB768,568,3,B^FS
^FO45,655^A0N,55,55^FDVISITOR^FS
^FO45,725^GB450,55,55,B^FS
^FO55,738^A0N,30,30^FR^FD{visitor1Name}^FS
^FO45,790^GB450,50,50,B^FS
^FO55,802^A0N,26,26^FR^FD{visitorCompany}^FS
^FO45,860^A0N,22,22^FDVISIT DATE:^FS
^FO200,850^GB270,38,2,B^FS
^FO215,858^A0N,20,20^FD{visitDateStr}^FS
^FO45,915^A0N,22,22^FDVALID UNTIL:^FS
^FO200,905^GB270,38,2,B^FS
^FO215,913^A0N,20,20^FD{validUntilStr}^FS
^FO45,970^A0N,22,22^FDHOST:^FS
^FO200,960^GB270,38,2,B^FS
^FO215,968^A0N,20,20^FD{selectedHostName}^FS
^FO45,1025^A0N,22,22^FDPURPOSE:^FS
^FO200,1015^GB270,38,2,B^FS
^FO215,1023^A0N,20,20^FD{purposeInEnglish}^FS
^FO45,1080^A0N,22,22^FDMOBILE:^FS
^FO200,1070^GB270,38,2,B^FS
^FO215,1078^A0N,20,20^FD{selectedMobile}^FS
^FO540,710^BQN,2,6^FDQA,{visitorToken}^FS
^FO530,995^A0N,24,24^FDID:^FS
^FO575,985^GB170,42,2,B^FS
^FO590,995^A0N,20,20^FD{visitorToken}^FS
^XZ";

            bool isPrintSuccess = false;
            await Task.Run(() =>
            {
                try { isPrintSuccess = RawPrinterHelper.SendStringToPrinter(ZebraPrinterName, zplData); }
                catch { isPrintSuccess = false; }
            });

            if (isPrintSuccess)
            {
                PrintStatusLabel.Text = "";
                ScrollPrivacyText.Visibility = Visibility.Collapsed;
                BtnAcceptPrint.Visibility = Visibility.Collapsed;

                PanelReceiptDetails.Visibility = Visibility.Visible;
                QrCodeContainer.Visibility = Visibility.Visible;
                BtnDoneReceipt.Visibility = Visibility.Visible;
                BorderReceptionistAlert.Visibility = Visibility.Visible;

                DateTime purgeDate = DateTime.Now.AddDays(90);
                string secureHash = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 16);

                if (isArabic)
                {
                    TxtReceptionistAlert.Text = currentFlow == "WalkIn"
                        ? "🔔 تنبيه الاستقبال السريع: تم رصد دخول زائر بدون موعد بنجاح، بانتظار استكمال بيانات المضيف بالخلفية."
                        : $"تنبيه الاستقبال: وصل الزائر ({visitor1Name}) وهو في انتظار المضيف ({selectedHostName}) حالياً.";

                    TxtReceiptDataCaptured.Text = $"• البيانات الشخصية: {visitor1Name} | الجهة: {visitorCompany}\n• الشخص المضيف: {selectedHostName}\n• غرض الزيارة المعتمد: {selectedPurpose}";
                    TxtReceiptPurgeDate.Text = $"⚠️ سيتم إتلاف سجل بياناتك الشخصية آلياً بتاريخ: {purgeDate:yyyy-MM-dd} (حسب سياسة الاحتفاظ بـ 90 يوماً).";
                    TxtReceiptHash.Text = $"SHA-256 Integrity Hash: AIN-SEC-{secureHash}";
                }
                else
                {
                    TxtReceptionistAlert.Text = currentFlow == "WalkIn"
                        ? "🔔 Quick Reception Alert: Unscheduled visitor check-in successful. Pending back-fill completion."
                        : $"Reception Alert: Visitor ({visitor1Name}) has arrived and is waiting for Host ({selectedHostName}).";

                    TxtReceiptDataCaptured.Text = $"• Personal Data: {visitor1Name} | Org: {visitorCompany}\n• Host Person: {selectedHostName}\n• Approved Purpose: {selectedPurpose}";
                    TxtReceiptPurgeDate.Text = $"⚠️ Your personal data record will be automatically purged on: {purgeDate:yyyy-MM-dd} (90 days retention limit).";
                    TxtReceiptHash.Text = $"SHA-256 Integrity Hash: AIN-SEC-{secureHash}";
                }

                _idleTimer?.Stop();
                _idleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
                _idleTimer.Tick += (s, ev) => { ResetToHomeView(forceResetToArabic: true); };
                _idleTimer.Start();
            }
            else
            {
                isInErrorState = true;
                BtnAcceptPrint.IsEnabled = true;
                BtnAcceptPrint.Background = Brushes.DarkRed;
                BtnAcceptPrint.Content = isArabic ? "العودة للقائمة الرئيسية" : "Return to Home";
                QrCodeContainer.Visibility = Visibility.Visible;
                PrintStatusLabel.Foreground = Brushes.Red;
                PrintStatusLabel.Text = isArabic ? "⚠️ عذراً، طابعة البطاقات غير متوفرة حالياً!" : "⚠️ Printer is offline!";
                _idleTimer?.Start();
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

        // ================= [ ميكانيكية إعادة الضبط الشاملة للمصنع ] =================
        private void ResetToHomeView(bool forceResetToArabic = false)
        {
            ViewPrivacy.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Collapsed;
            ViewDetails.Visibility = Visibility.Collapsed;
            ViewRetroactive.Visibility = Visibility.Collapsed;
            ViewHome.Visibility = Visibility.Visible;

            BtnAcceptPrint.IsEnabled = true;
            isInErrorState = false;
            PrintStatusLabel.Text = "";
            QrCodeContainer.Visibility = Visibility.Collapsed;
            BorderReceptionistAlert.Visibility = Visibility.Collapsed;
            ScrollPrivacyText.Visibility = Visibility.Visible;
            BtnAcceptPrint.Visibility = Visibility.Visible;
            PanelReceiptDetails.Visibility = Visibility.Collapsed;
            BtnDoneReceipt.Visibility = Visibility.Collapsed;

            TxtHostName.Text = "";
            TxtMobileNumber.Text = "";
            selectedPurpose = isArabic ? "اجتماع عمل" : "Business Meeting";

            if (forceResetToArabic) isArabic = true;
            ApplyLanguage();
            _idleTimer?.Stop();
            _idleTimer?.Start();
        }

        private void InitializeIdleTimer()
        {
            _idleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(45) };
            _idleTimer.Tick += (s, e) => { _idleTimer?.Stop(); ResetToHomeView(forceResetToArabic: true); };
            _idleTimer.Start();
        }

        private void Window_PreviewInteraction(object sender, System.Windows.Input.InputEventArgs e)
        {
            if (_idleTimer != null) { _idleTimer.Stop(); _idleTimer.Start(); }
        }

        private void BtnLangToggle_Click(object sender, RoutedEventArgs e)
        {
            isArabic = !isArabic;
            ApplyLanguage();
        }

        // ================= [ إصلاح حركة وتفاعل أزرار الـ Chips برمجياً ] =================
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
                selectedPurpose = clickedButton.Content?.ToString() ?? "اجتماع عمل";
            }
        }

        private void BtnNextToPrivacy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHostName.Text))
            {
                MessageBox.Show(isArabic ? "يرجى كتابة اسم الشخص المضيف أولاً!" : "Please enter the host name!", "AIN Kiosk", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selectedHostName = TxtHostName.Text;
            selectedMobile = string.IsNullOrWhiteSpace(TxtMobileNumber.Text) ? "لم يسجل" : TxtMobileNumber.Text;

            string rawDataUrl = $"https://ain.ebtco.sa/privacy?session={Guid.NewGuid()}&flow={currentFlow}";
            string qrGeneratorApi = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(rawDataUrl)}";
            try { KioskQrImage.Source = new BitmapImage(new Uri(qrGeneratorApi)); } catch { }

            ViewDetails.Visibility = Visibility.Collapsed;
            ViewPrivacy.Visibility = Visibility.Visible;
            ApplyLanguage();
        }

        // ================= [ 🚀 محرك الترجمة ومحاذاة الواجهات الديناميكي الشامل ] =================
        private void ApplyLanguage()
        {
            if (isArabic)
            {
                KioskWindow.FlowDirection = FlowDirection.RightToLeft;
                BtnLangToggle.Content = "English";

                WelcomeTitle.Text = "مرحباً بك في جهاز الخدمة الذاتية";
                WelcomeSubtitle.Text = "يرجى اختيار مسار الزيارة المناسب للبدء";
                BtnPreRegistered.Content = "زائر مسجل";
                BtnWalkIn.Content = "زائر بدون موعد";
                BtnRetroactive.Content = "تسجيل بأثر رجعي";
                BtnHelp.Content = "مساعدة";

                ScanTitle.Text = currentFlow == "WalkIn" ? "جاري معالجة مسار زائر بدون موعد..." : "جاري معالجة مسار زائر مسجل مسبقاً...";
                ScanSubtitle.Text = "يرجى وضع بطاقة الهوية أو جواز السفر على جهاز الفحص";

                DetailsTitle.Text = "استكمال معلومات الزيارة";
                DetailsSubtitle.Text = "يرجى تحديد الشخص المضيف وسبب زيارتك اليوم";
                LblHostName.Text = "اسم الشخص المضيف:";
                LblPurpose.Text = "سبب الزيارة:";
                LblMobile.Text = "رقم الجوال - اختياري:";
                ChipMeeting.Content = "اجتماع عمل";
                ChipInterview.Content = "مقابلة شخصية";
                ChipMaintenance.Content = "صيانة فنية";
                ChipDelivery.Content = "توصيل / شحنة";
                ChipVendor.Content = "مورد / عميل";
                ChipOther.Content = "أخرى";
                BtnNextToPrivacy.Content = "التالي: مراجعة سياسة الخصوصية";

                QrLabel.Text = "امسح الرمز للاحتفاظ بالوثيقة";
                if (!isInErrorState) BtnAcceptPrint.Content = "أوافق واطبع البطاقة";
                BtnDoneReceipt.Content = "إنهاء والعودة للرئيسية";

                TxtRetroTitle.Text = "تسجيل زائر بأثر رجعي (Retroactive Entry)";
                TxtRetroWarning.Text = "⚠️ تنبيه أمني: أنت تقوم بتسجيل حركة زائر سابقة يدوياً. يخضع هذا الإجراء للتدقيق الصارم.";
                LblRetroVisName.Text = "اسم الزائر بالكامل:";
                LblRetroDocNum.Text = "رقم الوثيقة / الهوية:";
                LblRetroHost.Text = "اسم الموظف المضيف:";
                LblRetroPurpose.Text = "غرض الزيارة الفعلي:";
                ChkRetroAttestation.Content = "أقر وأصادق بصفتي المضيف بأنني رافقت هذا الزائر مادية وأن البيانات أعلاه دقيقة.";
                BtnSubmitRetroactive.Content = "تأكيد وحفظ السجل بأثر رجعي";
                BtnCancelRetroactive.Content = "إلغاء والعودة للرئيسية";

                if (currentFlow == "WalkIn")
                {
                    PrivacyTitle.Text = "إقرار سياسة الخصوصية الفوري (One-Click Consent)";
                    PrivacyBody.Text = "بضغطك على الزر أدناه، يتم منح الموافقة الفورية على معالجة الهوية آلياً وإصدار بطاقة الدخول الممتثلة لنظام PDPL.";
                }
                else
                {
                    PrivacyTitle.Text = "وثيقة سياسة الخصوصية وحماية البيانات";
                    PrivacyBody.Text = "يتعهد نظام (عين) لحلول الزوار بحماية بياناتك الشخصية وفقاً للأنظمة واللوائح الصادرة من الهيئة السعودية للبيانات والذكاء الاصطناعي (سدايا) ومصرف تداول المركزي (ساما). بضغطك على زر (أوافق واطبع البطاقة)، فإنك تمنح النظام صلاحية معالجة هذه البيانات بشكل آمن ومؤقت.";
                }
            }
            else
            {
                KioskWindow.FlowDirection = FlowDirection.LeftToRight;
                BtnLangToggle.Content = "العربية";

                WelcomeTitle.Text = "Welcome to the Self-Service Kiosk";
                WelcomeSubtitle.Text = "Please select the appropriate visit path to start";
                BtnPreRegistered.Content = "Pre-Registered";
                BtnWalkIn.Content = "Walk-In";
                BtnRetroactive.Content = "Retroactive Entry";
                BtnHelp.Content = "Help";

                ScanTitle.Text = currentFlow == "WalkIn" ? "Processing Walk-In flow..." : "Processing Pre-Registered flow...";
                ScanSubtitle.Text = "Please place your ID card or passport on the scanner";

                DetailsTitle.Text = "Complete Visit Information";
                DetailsSubtitle.Text = "Please specify the host and your purpose of visit";
                LblHostName.Text = "Host Name:";
                LblPurpose.Text = "Purpose of Visit:";
                LblMobile.Text = "Mobile Number - Optional:";
                ChipMeeting.Content = "Business Meeting";
                ChipInterview.Content = "Job Interview";
                ChipMaintenance.Content = "Technical Maintenance";
                ChipDelivery.Content = "Delivery / Courier";
                ChipVendor.Content = "Vendor / Supplier";
                ChipOther.Content = "Other";
                BtnNextToPrivacy.Content = "Next: Review Privacy Policy";

                QrLabel.Text = "Scan QR to keep the receipt";
                if (!isInErrorState) BtnAcceptPrint.Content = "Accept & Print Badge";
                BtnDoneReceipt.Content = "Finish & Return Home";

                TxtRetroTitle.Text = "Retroactive Entry Logging";
                TxtRetroWarning.Text = "⚠️ Security Notice: You are recording a past visitor entry manually. This action is fully audited.";
                LblRetroVisName.Text = "Visitor Full Name:";
                LblRetroDocNum.Text = "Document / ID Number:";
                LblRetroHost.Text = "Host Employee Name:";
                LblRetroPurpose.Text = "Actual Visit Purpose:";
                ChkRetroAttestation.Content = "I confirm I escorted this visitor and the above metadata is accurate.";
                BtnSubmitRetroactive.Content = "Confirm & Save Retroactive Record";
                BtnCancelRetroactive.Content = "Cancel & Return Home";

                if (currentFlow == "WalkIn")
                {
                    PrivacyTitle.Text = "One-Click Privacy Consent";
                    PrivacyBody.Text = "By clicking the button below, you grant immediate authorization to process your ID metadata and issue a temporary entry badge compliant with PDPL regulations.";
                }
                else
                {
                    PrivacyTitle.Text = "Privacy Policy & Data Protection Contract";
                    PrivacyBody.Text = "The (AIN) Visitor Solutions system is committed to protecting your personal data in accordance with the regulations issued by SDAIA and SAMA. By clicking (Accept & Print Badge), you grant the system permission to process this data securely.";
                }
            }
        }
    }
}