using System;
using System.Threading.Tasks;
using AIN.Visitors.Mrz;
using AIN.Visitors.Mrz.Abstractions; 
using AIN.Visitors.Mrz.Scanners;     

namespace AIN_Kiosk.Adapters
{
    public class ScannerAdapter
    {
        private readonly IDocumentScanner _mrzScanner;

        public ScannerAdapter()
        {
           
            _mrzScanner = new MockDocumentScanner();
        }

        public async Task<bool> ExecuteScanAsync()
        {
            try
            {
            
                await Task.Delay(2500);

                var scanResult = await _mrzScanner.ScanAsync();

                
                if (scanResult != null && scanResult.IsSuccess)
                {
                    return true;
                }

                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MRZ Scan Error]: {ex.Message}");
                return false;
            }
        }
    }
}