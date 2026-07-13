using System;

namespace AIN.Visitors.Mrz.Models
{
    public class MrzFields
    {
        public string DocumentType { get; set; }
        public string IssuingState { get; set; }
        public string PrimaryIdentifier { get; set; } // Last Name
        public string SecondaryIdentifier { get; set; } // First Name
        public string DocumentNumber { get; set; }
        public string Nationality { get; set; }
        public string DateOfBirth { get; set; } // Format: YYMMDD
        public string Sex { get; set; } // M, F, or <
        public string DateOfExpiry { get; set; } // Format: YYMMDD
        public string PersonalNumber { get; set; } // Optional Data
//for error
        public bool IsValid { get; set; }
        public MrzErrorCode ErrorCode { get; set; } = MrzErrorCode.None;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}