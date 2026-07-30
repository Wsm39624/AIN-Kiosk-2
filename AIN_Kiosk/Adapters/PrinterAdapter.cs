using System;
using System.Diagnostics;

namespace AIN_Kiosk.Adapters
{
    // Printer adapter interface enabling dependency injection and test mocking
    public interface IPrinterAdapter
    {
        bool SendStringToPrinter(string printerName, string zplData);
    }

    public class PrinterAdapter : IPrinterAdapter
    {
        // Virtual method allowing test frameworks to mock thermal printing operations
        public virtual bool SendStringToPrinter(string printerName, string zplData)
        {
            try
            {
                // Delegate physical thermal printing to raw printer helper
                return RawPrinterHelper.SendStringToPrinter(printerName, zplData);
            }
            catch (Exception ex)
            {
                // Log sanitized diagnostic information containing only exception type and message
                // Excludes visitor PII, badge content, ZPL payloads, or sensitive tokens
                Debug.WriteLine($"[Printer Error] Exception Type: {ex.GetType().Name}, Technical Message: {ex.Message}");
                return false;
            }
        }
    }
}