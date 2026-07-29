using System;

namespace AIN_Kiosk.Adapters
{
    // الواجهة ضرورية لعمل الـ Dependency Injection (DI) والاختبارات (Mocking)
    public interface IPrinterAdapter
    {
        bool SendStringToPrinter(string printerName, string zplData);
    }

    public class PrinterAdapter : IPrinterAdapter
    {
        // تم إضافة 'virtual' لكي نتمكن من عمل Mocking لهذه الدالة في xUnit
        public virtual bool SendStringToPrinter(string printerName, string zplData)
        {
            try
            {
                // عزل أداة المساعدة الخاصة بالطابعة المادية هنا
                return RawPrinterHelper.SendStringToPrinter(printerName, zplData);
            }
            catch (Exception)
            {
                // إرجاع false في حال فشل الاتصال بالطابعة أو تعذر إرسال البيانات
                return false;
            }
        }
    }
}