using System;
using System.Threading;
using System.Threading.Tasks;
using AIN_Kiosk.Services;

namespace AIN_Kiosk
{
    // Badge printer hardware abstraction interface
    public interface IBadgePrinter
    {
        Task<bool> PrintBadgeAsync(string zplTemplate, CancellationToken cancellationToken);
    }

    // Backend visitor synchronization transport interface
    public interface IVisitorSyncTransport
    {
        Task<bool> SyncVisitorAsync(object session, CancellationToken cancellationToken);
    }

    // In-memory registration queue interface for transient session simulation
    public interface IRegistrationQueue
    {
        void EnqueueMockRecord(string flow, string sessionRef);
    }

    // Mock implementation of transient registration queue without PII logging
    public class MockRegistrationQueue : IRegistrationQueue
    {
        public void EnqueueMockRecord(string flow, string sessionRef)
        {
            // Logs use synthetic correlation references to avoid writing visitor PII to debug output
            System.Diagnostics.Debug.WriteLine($"[Mock Queue] Enqueued {flow} for session ref {sessionRef} (Simulated - not persisted).");
        }
    }

    // Kiosk configuration provider contract
    public interface IKioskConfigurationProvider
    {
        string TenantName { get; }
        string PrivacyContact { get; }
        bool ShowFacilityLocation { get; }
        string FacilityName { get; }
        FieldRequirement EmailRequirement { get; }
        string SupervisorDemoPin { get; }
    }

    // Privacy notice provider contract
    public interface IKioskPrivacyNoticeProvider
    {
        string GetRetentionStatement(bool isArabic);
    }

    // Digital receipt reference provider contract
    public interface IReceiptReferenceProvider
    {
        string GetSyntheticToken();
    }

    // Local mock provider returning synthetic developmental receipt references
    public class MockReceiptReferenceProvider : IReceiptReferenceProvider
    {
        public string GetSyntheticToken()
        {
            return "DEV-SYNTHETIC-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        }
    }
}