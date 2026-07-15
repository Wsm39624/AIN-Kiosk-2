using System;
using System.Collections.Generic;
using System.Text;

namespace AIN_Kiosk.Services
{
    public class KioskConfigurationProvider : AIN_Kiosk.IKioskConfigurationProvider
    {
        // هذه القيم سيتم جلبها مستقبلاً من الـ Backend (ABP.io)
        public string TenantName => "EBTCO / BDO Al-Amri";
        public string PrivacyContact => "privacy@ain.ebtco.com.sa";
        public bool ShowFacilityLocation => false; // مخفي حسب توجيهات خالد ما لم يُطلب تفعيله
        public string FacilityName => "Riyadh HQ";
    }
}
