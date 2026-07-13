using System.Threading;
using System.Threading.Tasks;
using AIN.Visitors.Mrz.Models;

namespace AIN.Visitors.Mrz.Abstractions
{
    public interface IDocumentScanner
    {
        Task<ScanResult> ScanAsync(CancellationToken cancellationToken = default);
    }
}