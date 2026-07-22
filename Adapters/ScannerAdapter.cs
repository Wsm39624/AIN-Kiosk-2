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
            // استدعاء السكينر من مكتبة خالد
            _mrzScanner = new MockDocumentScanner();
        }

        /// <summary>
        /// ينفذ عملية فحص الوثيقة برمجياً ويرجع true في حال النجاح و false في حال الفشل
        /// </summary>
        public async Task<bool> ExecuteScanAsync()
        {
            try
            {
                // 1. إعطاء مهلة زمنية 2.5 ثانية لإتاحة الفرصة للزائر لقراءة الشاشة ووضع الهوية
                await Task.Delay(2500);

                // 2. طلب الفحص من مكتبة خالد
                var scanResult = await _mrzScanner.ScanAsync();

                // 3. التثبت من القراءة
                if (scanResult != null && scanResult.IsSuccess)
                {
                    return true;
                }

                // في حال عدم توفر النتيجة أو فشلها
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MRZ Scan Error]: {ex.Message}");
                return false;
            }
        }
    }
}