using System;
using System.Threading.Tasks;
using AIN_Kiosk.Adapters; // 🎯 استدعاء طبقة المحولات

namespace AIN_Kiosk.Services
{
    public class BadgePrintService
    {
        private const string ZebraPrinterName = "ZDesigner GK420t";
        private readonly PrinterAdapter _printerAdapter = new(); // تهيئة المحول

        // 🎯 التوقيع الجديد المحدث ليستقبل 4 متغيرات بالتزامن
        public async Task<bool> PrintVisitorBadgeAsync(string hostName, string purpose, string mobile, string visitorToken)
        {
            string visitor1Name = "Wesam Mohammed";
            string visitor2Name = "Eng. Khaled Alamri";
            string visitorCompany = "EBTCO / BDO Al-Amri";

            string visitDateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string validUntilStr = DateTime.Now.ToString("yyyy-MM-dd");

            string purposeInEnglish = "General Visit";
            if (purpose.Contains("اجتماع") || purpose.ToLower().Contains("meeting")) purposeInEnglish = "Business Meeting";
            else if (purpose.Contains("مقابلة") || purpose.ToLower().Contains("interview")) purposeInEnglish = "Job Interview";
            else if (purpose.Contains("صيانة") || purpose.ToLower().Contains("maintenance")) purposeInEnglish = "Technical Maintenance";
            else if (purpose.Contains("توصيل") || purpose.ToLower().Contains("delivery")) purposeInEnglish = "Delivery / Courier";
            else if (purpose.Contains("مورد") || purpose.ToLower().Contains("vendor")) purposeInEnglish = "Vendor / Supplier";

            string zplData = $@"
^XA
~SD24
^PW800
^LL1200
^FO16,16^GB768,568,3,B^FS
^FO45,55^A0N,55,55^FDVISITOR^FS
^FO45,125^GB450,55,55,B^FS
^FO55,138^A0N,30,30^FR^FD{visitor2Name}^FS
^FO45,190^GB450,50,50,B^FS
^FO55,202^A0N,26,26^FR^FD{visitorCompany}^FS
^FO45,260^A0N,22,22^FDVISIT DATE:^FS
^FO200,250^GB270,38,2,B^FS
^FO215,258^A0N,20,20^FD{visitDateStr}^FS
^FO45,315^A0N,22,22^FDVALID UNTIL:^FS
^FO200,305^GB270,38,2,B^FS
^FO215,313^A0N,20,20^FD{validUntilStr}^FS
^FO45,370^A0N,22,22^FDHOST:^FS
^FO200,360^GB270,38,2,B^FS
^FO215,368^A0N,20,20^FD{hostName}^FS
^FO45,425^A0N,22,22^FDPURPOSE:^FS
^FO200,415^GB270,38,2,B^FS
^FO215,423^A0N,20,20^FD{purposeInEnglish}^FS
^FO45,480^A0N,22,22^FDMOBILE:^FS
^FO200,470^GB270,38,2,B^FS
^FO215,478^A0N,20,20^FD{mobile}^FS
^FO540,110^BQN,2,6^FDQA,{visitorToken}^FS
^FO530,395^A0N,24,24^FDID:^FS
^FO575,385^GB170,42,2,B^FS
^FO590,395^A0N,20,20^FD{visitorToken}^FS

^FO16,616^GB768,568,3,B^FS
^FO45,655^A0N,55,55^FDVISITOR^FS
^FO45,725^GB450,55,55,B^FS
^FO55,738^A0N,30,30^FR^FD{visitor1Name}^FS
^FO45,790^GB450,50,50,B^FS
^FO55,802^A0N,26,26^FR^FD{visitorCompany}^FS
^FO45,860^A0N,22,22^FDVISIT DATE:^FS
^FO200,850^GB270,38,2,B^FS
^FO215,858^A0N,20,20^FD{visitDateStr}^FS
^FO45,915^A0N,22,22^FDVALID UNTIL:^FS
^FO200,905^GB270,38,2,B^FS
^FO215,913^A0N,20,20^FD{visitDateStr}^FS
^FO45,970^A0N,22,22^FDHOST:^FS
^FO200,960^GB270,38,2,B^FS
^FO215,968^A0N,20,20^FD{hostName}^FS
^FO45,1025^A0N,22,22^FDPURPOSE:^FS
^FO200,1015^GB270,38,2,B^FS
^FO215,1023^A0N,20,20^FD{purposeInEnglish}^FS
^FO45,1080^A0N,22,22^FDMOBILE:^FS
^FO200,1070^GB270,38,2,B^FS
^FO215,1078^A0N,20,20^FD{mobile}^FS
^FO540,710^BQN,2,6^FDQA,{visitorToken}^FS
^FO530,995^A0N,24,24^FDID:^FS
^FO575,985^GB170,42,2,B^FS
^FO590,995^A0N,20,20^FD{visitorToken}^FS
^XZ";

            bool isPrintSuccess = false;
            await Task.Run(() =>
            {
                try
                {
                    isPrintSuccess = _printerAdapter.SendStringToPrinter(ZebraPrinterName, zplData);
                }
                catch (Exception ex)
                {
                    isPrintSuccess = false;
                    System.Diagnostics.Debug.WriteLine($"[Printer Error] Native print thread crashed: {ex.Message}");
                }
            });

            return isPrintSuccess;
        }
    }
}