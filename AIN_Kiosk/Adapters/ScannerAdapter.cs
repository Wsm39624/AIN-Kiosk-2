using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AIN.Visitors.Mrz.Abstractions;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Scanners;

namespace AIN_Kiosk.Adapters
{
    // Document scanner adapter layer (Integrated for development — parser acceptance pending)
    public class ScannerAdapter
    {
        private readonly IDocumentScanner _mrzScanner;

        public ScannerAdapter()
        {
            _mrzScanner = new MockDocumentScanner();
        }

        // Executes scan operation and returns full approved normalized ScanResult to the workflow pipeline
        public async Task<ScanResult> ExecuteScanResultAsync()
        {
            try
            {
                await Task.Delay(2500);

                var scanResult = await _mrzScanner.ScanAsync();
                if (scanResult != null)
                {
                    return scanResult;
                }

                var failedResult = new ScanResult { ErrorCode = MrzErrorCode.HardwareError };
                failedResult.Evidence.ValidationStatus = "Invalid";
                return failedResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MRZ Scan Error] Exception Type: {ex.GetType().Name}, Technical Message: {ex.Message}");
                var errorResult = new ScanResult { ErrorCode = MrzErrorCode.HardwareError };
                errorResult.Evidence.ValidationStatus = "Invalid";
                return errorResult;
            }
        }

        public async Task<bool> ExecuteScanAsync()
        {
            var result = await ExecuteScanResultAsync();
            return result.IsSuccess;
        }
    }
}