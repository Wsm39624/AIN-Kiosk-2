using System;
using System.Threading;
using System.Threading.Tasks;
using AIN_Kiosk.Services;

namespace AIN_Kiosk
{
    // واجهة طابعة الشارات (Zebra)
    public interface IBadgePrinter
    {
        Task<bool> PrintBadgeAsync(string zplTemplate, CancellationToken cancellationToken);
    }

    // واجهة المزامنة مع الـ Backend
    public interface IVisitorSyncTransport
    {
        Task<bool> SyncVisitorAsync(object session, CancellationToken cancellationToken);
    }

    // الواجهة البرمجية المطلوبة رقابياً لتأمين مسار المزامنة دون persistence
    public interface IRegistrationQueue
    {
        void EnqueueMockRecord(string flow, string visitorName);
    }

    public class MockRegistrationQueue : IRegistrationQueue
    {
        public void EnqueueMockRecord(string flow, string visitorName)
        {
            System.Diagnostics.Debug.WriteLine($"[Mock Queue] Enqueued {flow} for {visitorName} (Simulated - not persisted).");
        }
    }

    // واجهة إعدادات الكشك (تم تحديثها لتشمل متطلبات البريد ورمز المشرف)
    public interface IKioskConfigurationProvider
    {
        string TenantName { get; }
        string PrivacyContact { get; }
        bool ShowFacilityLocation { get; }
        string FacilityName { get; }
        FieldRequirement EmailRequirement { get; }
        string SupervisorDemoPin { get; }
    }

    // واجهة استهلاك فترات الاحتفاظ بالبيانات المعتمدة رقابياً
    public interface IKioskPrivacyNoticeProvider
    {
        string GetRetentionStatement(bool isArabic);
    }

    public interface IReceiptReferenceProvider
    {
        string GetSyntheticToken();
    }

    // المحاكي الملتزم بإرجاع توكن تطويري صريح وغير مضلل رقابياً
    public class MockReceiptReferenceProvider : IReceiptReferenceProvider
    {
        public string GetSyntheticToken()
        {
            return "DEV-SYNTHETIC-" + System.Guid.NewGuid().ToString("N").ToUpperInvariant();
        }
    }
}