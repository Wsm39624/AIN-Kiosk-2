using System;
using System.Windows.Threading;

namespace AIN_Kiosk.Services
{
    public class KioskWorkflowService
    {
        // 🔄 الأحداث (Events) لإبلاغ الواجهة بالتحديثات تلقائياً
        public event Action<string>? OnFlowChanged;
        public event Action<bool>? OnErrorStateChanged;
        public event Action? OnIdleTimeout;

        // 🧠 الحالات الداخليّة للمسار (State Management)
        private string _currentFlow = "WalkIn";
        private bool _isInErrorState = false;
        private DispatcherTimer? _idleTimer;

        // البيانات الخاصة بالزائر الحالي
        public string SelectedPurpose { get; set; } = "اجتماع عمل";
        public string SelectedHostName { get; set; } = string.Empty;
        public string SelectedMobile { get; set; } = string.Empty;

        public string CurrentFlow
        {
            get => _currentFlow;
            set
            {
                if (_currentFlow != value)
                {
                    _currentFlow = value;
                    OnFlowChanged?.Invoke(_currentFlow);
                }
            }
        }

        public bool IsInErrorState
        {
            get => _isInErrorState;
            set
            {
                if (_isInErrorState != value)
                {
                    _isInErrorState = value;
                    OnErrorStateChanged?.Invoke(_isInErrorState);
                }
            }
        }

        // ⏱️ إدارة مؤقت الخمول (Idle Timer) بشكل مستقل
        public void InitializeIdleTimer(TimeSpan timeout)
        {
            _idleTimer?.Stop();

            _idleTimer = new DispatcherTimer
            {
                Interval = timeout
            };
            _idleTimer.Tick += (s, e) => OnIdleTimeout?.Invoke();
            _idleTimer.Start();
        }

        public void ResetIdleTimer()
        {
            if (_idleTimer != null)
            {
                _idleTimer.Stop();
                _idleTimer.Start();
            }
        }

        public void StopIdleTimer()
        {
            _idleTimer?.Stop();
        }

        // 🧹 تصفير بيانات الجلسة عند العودة للشاشة الرئيسية
        public void ResetSession()
        {
            CurrentFlow = "WalkIn";
            IsInErrorState = false;
            SelectedPurpose = "اجتماع عمل";
            SelectedHostName = string.Empty;
            SelectedMobile = string.Empty;
            ResetIdleTimer();
        }
    }
}