using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIN_Kiosk
{
    //  (Zebra)
    public interface IBadgePrinter
    {
        Task<bool> PrintBadgeAsync(string zplTemplate, CancellationToken cancellationToken);
    }

    //  Backend
    public interface IVisitorSyncTransport
    {
        Task<bool> SyncVisitorAsync(object session, CancellationToken cancellationToken);
    }

    //  الواجهة البرمجية المطلوبة رقابياً لتأمين مسار المزامنة دونpersistence
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

    public interface IKioskConfigurationProvider
    {
        string TenantName { get; }
        string PrivacyContact { get; }
    }

    //  واجهة استهلاك فترات الاحتفاظ بالبيانات المعتمدة رقابياً
    public interface IKioskPrivacyNoticeProvider
    {
        string GetRetentionStatement(bool isArabic);
    }

    public interface IReceiptReferenceProvider
    {
        string GetSyntheticToken();
    }

    //  المحاكي الملتزم بإرجاع توكن تطويري صريح وغير مضلل رقابياً
    public class MockReceiptReferenceProvider : IReceiptReferenceProvider
    {
        public string GetSyntheticToken()
        {
         
            return "DEV-SYNTHETIC-" + System.Guid.NewGuid().ToString("N").ToUpperInvariant();
        }
    }
}