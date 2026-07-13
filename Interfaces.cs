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
}