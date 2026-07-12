using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIN_Kiosk
{
    // النتيجة القادمة من جهاز الفحص - مجهزة للتدمير الآمن من الذاكرة لحماية الخصوصية
    public sealed class ScanResult : IDisposable
    {
        public bool IsSuccess { get; set; }
        public string Metadata { get; set; } = string.Empty;
        public byte[]? RawPayload { get; set; }

        public void Dispose()
        {
            if (RawPayload != null)
            {
                Array.Clear(RawPayload, 0, RawPayload.Length);
                RawPayload = null;
            }
        }
    }

    // واجهة جهاز فحص الهويات المجردة
    public interface IDocumentScanner
    {
        Task<ScanResult> ScanAsync(CancellationToken cancellationToken);
    }

    // واجهة طابعة الملصقات (Zebra)
    public interface IBadgePrinter
    {
        Task<bool> PrintBadgeAsync(string zplTemplate, CancellationToken cancellationToken);
    }

    // واجهة المزامنة مع الـ Backend
    public interface IVisitorSyncTransport
    {
        Task<bool> SyncVisitorAsync(object session, CancellationToken cancellationToken);
    }
}