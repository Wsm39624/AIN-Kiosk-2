using System;

namespace AIN_Kiosk.Adapters
{
    public class PrinterAdapter
    {
        public bool SendStringToPrinter(string printerName, string zplData)
        {
            try
            {
                // عزل أداة المساعدة الخاصة بالطابعة المادية هنا
                return RawPrinterHelper.SendStringToPrinter(printerName, zplData);
            }
            catch
            {
                return false;
            }
        }
    }
}