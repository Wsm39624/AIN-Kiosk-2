using System;

namespace AIN_Kiosk.Services
{
    // Defines field requirement status for UI form controls
    public enum FieldRequirement
    {
        Required,
        Optional,
        Conditional,
        Hidden
    }

    // Local mock provider supplying development prototype configuration values
    public class LocalMockKioskConfigurationProvider : IKioskConfigurationProvider
    {
        public string TenantName => "Emerging Business Technologies CO. (EBTCO)";
        public string PrivacyContact => "privacy@ain.ebtco.com.sa";
        public bool ShowFacilityLocation => false;
        public string FacilityName => "Riyadh HQ";

        // Dynamic email field requirement policy enforced by workflows
        public FieldRequirement EmailRequirement => FieldRequirement.Required;

        // Prototype supervisor PIN for local development testing
        public string SupervisorDemoPin => "1234";
    }
}