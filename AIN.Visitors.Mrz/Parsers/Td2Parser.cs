using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Helpers;
namespace AIN.Visitors.Mrz.Parsers
{
    public class Td2Parser
    {
        public MrzFields Parse(ReadOnlySpan<char> line1, ReadOnlySpan<char> line2)
        {
            var result = new MrzFields { IsValid = false, ErrorCode = MrzErrorCode.None };

            if (line1.Length != 36 || line2.Length != 36)
            {
                result.ErrorCode = MrzErrorCode.InvalidLength;
                return result;
            }

            if (line1[0] != 'I' && line1[0] != 'A' && line1[0] != 'C')
            {
                result.ErrorCode = MrzErrorCode.InvalidFormat;
                return result;
            }

            if (ContainsLowercase(line1) || ContainsLowercase(line2))
            {
                result.ErrorCode = MrzErrorCode.InvalidCharacter;
                return result;
            }

            ReadOnlySpan<char> docNumberSpan = line2.Slice(0, 9);
            char docNumCheckDigitChar = line2[9];
            
            ReadOnlySpan<char> dobSpan = line2.Slice(13, 6);
            char dobCheckDigitChar = line2[19];
            
            ReadOnlySpan<char> expirySpan = line2.Slice(21, 6);
            char expiryCheckDigitChar = line2[27];
            
            ReadOnlySpan<char> compPart1 = line2.Slice(0, 10); // DocNum + Check
            ReadOnlySpan<char> compPart2 = line2.Slice(13, 7); // DOB + Check
            ReadOnlySpan<char> compPart3 = line2.Slice(21, 7); // Expiry + Check
            ReadOnlySpan<char> compPart4 = line2.Slice(28, 7); // Optional Data
            char compositeCheckDigitChar = line2[35];

            int expectedDocNumCd = CheckDigitHelper.Calculate(docNumberSpan);
            int expectedDobCd = CheckDigitHelper.Calculate(dobSpan);
            int expectedExpiryCd = CheckDigitHelper.Calculate(expirySpan);

            Span<char> compositeSpan = stackalloc char[31];
            compPart1.CopyTo(compositeSpan.Slice(0, 10));
            compPart2.CopyTo(compositeSpan.Slice(10, 7));
            compPart3.CopyTo(compositeSpan.Slice(17, 7));
            compPart4.CopyTo(compositeSpan.Slice(24, 7));
            
            int expectedCompositeCd = CheckDigitHelper.Calculate(compositeSpan);

            CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(compositeSpan));

            bool isValid = ((char)(expectedDocNumCd + '0') == docNumCheckDigitChar) &&
                           ((char)(expectedDobCd + '0') == dobCheckDigitChar) &&
                           ((char)(expectedExpiryCd + '0') == expiryCheckDigitChar) &&
                           ((char)(expectedCompositeCd + '0') == compositeCheckDigitChar);

            if (!isValid)
            {
                result.ErrorCode = MrzErrorCode.CheckDigitError;
                return result;
            }

            result.IsValid = true;
            
            result.DocumentType = CleanString(line1.Slice(0, 2));
            result.IssuingState = CleanString(line1.Slice(2, 3));
            result.DocumentNumber = CleanString(docNumberSpan);
            
            ReadOnlySpan<char> nameRaw = line1.Slice(5, 31);
            int separatorIndex = nameRaw.IndexOf("<<".AsSpan());
            if (separatorIndex != -1)
            {
                result.PrimaryIdentifier = CleanString(nameRaw.Slice(0, separatorIndex), replaceWithSpace: true);
                result.SecondaryIdentifier = CleanString(nameRaw.Slice(separatorIndex + 2), replaceWithSpace: true);
            }
            else
            {
                result.PrimaryIdentifier = CleanString(nameRaw, replaceWithSpace: true);
            }

            result.Nationality = CleanString(line2.Slice(10, 3));
            result.DateOfBirth = CleanString(dobSpan);
            
            char sexChar = line2[20];
            result.Sex = sexChar == '<' ? "U" : sexChar.ToString();
            
            result.DateOfExpiry = CleanString(expirySpan);
            result.PersonalNumber = CleanString(line2.Slice(28, 7));

            return result;
        }

        private bool ContainsLowercase(ReadOnlySpan<char> span)
        {
            foreach (char c in span)
            {
                if (c >= 'a' && c <= 'z') return true;
            }
            return false;
        }

        private string CleanString(ReadOnlySpan<char> span, bool replaceWithSpace = false)
        {
            int end = span.Length - 1;
            while (end >= 0 && span[end] == '<') end--;
            if (end < 0) return string.Empty;
            
            ReadOnlySpan<char> trimmed = span.Slice(0, end + 1);
            Span<char> buffer = stackalloc char[trimmed.Length];
            int actualLength = 0;
            
            if (!replaceWithSpace)
            {
                foreach (char c in trimmed)
                {
                    if (c != '<') buffer[actualLength++] = c;
                }
            }
            else
            {
                foreach (char c in trimmed)
                {
                    buffer[actualLength++] = (c == '<') ? ' ' : c;
                }
            }
            
            return buffer.Slice(0, actualLength).ToString();
        }
    }
}