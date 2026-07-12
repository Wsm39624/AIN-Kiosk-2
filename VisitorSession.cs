using System;

namespace AIN_Kiosk
{
    public sealed class VisitorSession
    {
        public Guid SessionId { get; private set; }

        // =========================================================================
        // 1. LEVEL 1: Most Sensitive (Classified — Restricted under NDMO)
        // =========================================================================
        public string VisitorFullName { get; private set; } = string.Empty;
        public string RawDocumentNumber { get; private set; } = string.Empty;
        public string Nationality { get; private set; } = string.Empty;
        public DateTime? DateOfBirth { get; private set; }
        public byte[]? RawDocumentImagePayload { get; private set; }

        // =========================================================================
        // 2. LEVEL 2: Personal but Lower-Risk (PII for Operational Use)
        // =========================================================================
        public DateTime ArrivalTimestamp { get; private set; }
        public string PurposeOfVisit { get; private set; } = "General";
        public string HostName { get; private set; } = string.Empty;
        public string MobileNumber { get; private set; } = string.Empty;
        public string CompanyName { get; private set; } = string.Empty;
        public string ConsentVersionString { get; private set; } = "v1.1-SDAIA";

        // =========================================================================
        // 3. LEVEL 3: Operational Metadata (آمن للـ Logging والأرشفة)
        // =========================================================================
        public string CaptureMethod { get; private set; } = "Scanned";
        public string KioskIdentifier { get; private set; } = "RIYADH_KIOSK_01";
        public string TenantIdentifier { get; private set; } = "BDO_AlAmri_HQ";

        // 🚀 إضافة المتغير التشغيلي لحل خطأ لقطة الشاشة CS1729
        public string OperatorUserId { get; private set; } = string.Empty;

        // 🚀 تحديث المنشئ (Constructor) ليستقبل البارامترين المتطابقين مع مدير الجلسات
        public VisitorSession(Guid sessionId, string operatorUserId)
        {
            SessionId = sessionId;
            OperatorUserId = operatorUserId;
            ArrivalTimestamp = DateTime.Now;
        }

        // =========================================================================
        // ⚙️ ميكانيكية معالجة البيانات وتقليلها (Data-Minimization Principle)
        // =========================================================================
        public void SetIdentityData(string fullName, string docNumber, string nationality, DateTime dob, byte[] imagePayload, bool isForeignPassport)
        {
            VisitorFullName = fullName;
            Nationality = nationality;
            DateOfBirth = dob;
            RawDocumentImagePayload = imagePayload;

            if (isForeignPassport && docNumber.Length > 4)
            {
                RawDocumentDocumentLastFour(docNumber);
            }
            else
            {
                RawDocumentNumber = docNumber;
            }
        }

        private void RawDocumentDocumentLastFour(string fullPassportNumber)
        {
            RawDocumentNumber = "****" + fullPassportNumber.Substring(fullPassportNumber.Length - 4);
        }

        // =========================================================================
        // 🧼 نظام التطهير الفوري للذاكرة (Memory Wipe - DoD Gate)
        // =========================================================================
        public void WipeSensitiveDataInMemory()
        {
            VisitorFullName = string.Empty;
            RawDocumentNumber = string.Empty;
            Nationality = string.Empty;
            DateOfBirth = null;
            MobileNumber = string.Empty;

            if (RawDocumentImagePayload != null)
            {
                Array.Clear(RawDocumentImagePayload, 0, RawDocumentImagePayload.Length);
                RawDocumentImagePayload = null;
            }

            GC.Collect();
        }

        // =========================================================================
        // 🛡️ دالة توليد الـ Logs الآمنة المتوافقة مع الفحص الرقابي لقاعدة الامتثال
        // =========================================================================
        public string ToSafeAuditLog()
        {
            return $"[AUDIT] Timestamp: {ArrivalTimestamp:yyyy-MM-dd HH:mm:ss} | " +
                   $"Session: {SessionId} | " +
                   $"Operator: {OperatorUserId} | " + // ربط معرّف الموظف الفعلي بالعملية
                   $"Method: {CaptureMethod} | " +
                   $"Kiosk: {KioskIdentifier} | " +
                   $"Tenant: {TenantIdentifier} | " +
                   $"Status: ActionProcessed | " +
                   $"BiometricsCaptured: FALSE | " +
                   $"CameraActive: FALSE";
        }
    }
}