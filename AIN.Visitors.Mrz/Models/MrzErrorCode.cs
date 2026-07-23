namespace AIN.Visitors.Mrz.Models
{
    public enum MrzErrorCode
    {
        None = 0,
        InvalidLength,
        InvalidFormat,
        CheckDigitError,
        DocumentNumberCheckDigitFailure,
        BirthDateCheckDigitFailure,
        ExpiryDateCheckDigitFailure,
        OptionalDataCheckDigitFailure,
        CompositeCheckDigitFailure,
        ImpossibleBirthDate,
        ImpossibleExpiryDate,
        ExpiredDocument,
        InvalidSexMarker,
        UnknownCountryCode,
        IllegalCharacter,
        AmbiguousCentury,
        UnsupportedFormat,
        InternalParserFailure
    }
}