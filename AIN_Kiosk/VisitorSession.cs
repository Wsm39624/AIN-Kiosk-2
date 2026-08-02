using System;

namespace AIN_Kiosk
{
    public sealed class VisitorSession
    {
        public Guid SessionId { get; private set; }

        // Level 1: Restricted / Sensitive Identity Information
        public string VisitorFullName { get; private set; } = string.Empty;
        public string RawDocumentNumber { get; private set; } = string.Empty;
        public string Nationality { get; private set; } = string.Empty;
        public DateTime? DateOfBirth { get; private set; }
        public byte[]? RawDocumentImagePayload { get; private set; }

        // Level 2: Operational PII
        public DateTime ArrivalTimestamp { get; private set; }
        public string PurposeOfVisit { get; private set; } = "General";
        public string HostName { get; private set; } = string.Empty;
        public string MobileNumber { get; private set; } = string.Empty;
        public string CompanyName { get; private set; } = string.Empty;
        public string ConsentVersionString { get; private set; } = "v1.1-SDAIA";

        // Level 3: Operational Metadata
        public string CaptureMethod { get; private set; } = "Scanned";
        public string KioskIdentifier { get; private set; } = "RIYADH_KIOSK_01";
        public string TenantIdentifier { get; private set; } = "BDO_AlAmri_HQ";
        public string OperatorUserId { get; private set; } = string.Empty;

        public VisitorSession(Guid sessionId, string operatorUserId)
        {
            SessionId = sessionId;
            OperatorUserId = operatorUserId;
            ArrivalTimestamp = DateTime.Now;
        }

        // Populates identity details enforcing doc number truncation for foreign credentials
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

        // Item 16: Clears session variables and zeroes mutable byte buffers.
        // String memory management relies on standard .NET runtime garbage collection behavior.
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
        }

        // Item 17: Formats operational audit log using only active runtime metadata, removing static flag claims
        public string ToSafeAuditLog()
        {
            return $"[AUDIT] Timestamp: {ArrivalTimestamp:yyyy-MM-dd HH:mm:ss} | " +
                   $"Session: {SessionId} | " +
                   $"Operator: {OperatorUserId} | " +
                   $"Method: {CaptureMethod} | " +
                   $"Kiosk: {KioskIdentifier} | " +
                   $"Tenant: {TenantIdentifier}";
        }
    }
}