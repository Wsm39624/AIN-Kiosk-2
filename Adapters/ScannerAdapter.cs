using System;
using System.Threading;
using System.Threading.Tasks;
using AIN.Visitors.Mrz.Abstractions;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Scanners;

namespace AIN_Kiosk.Adapters
{
    public class ScannerAdapter
    {
        private readonly IDocumentScanner _hardwareScanner;

        public ScannerAdapter()
        {
            // تغليف وتهيئة القارئ الأصلي بداخل المحول
            _hardwareScanner = new MockDocumentScanner();
        }

        public async Task<bool> ExecuteScanAsync()
        {
            try
            {
                using (ScanResult scanResult = await _hardwareScanner.ScanAsync(CancellationToken.None))
                {
                    return scanResult.IsSuccess;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}