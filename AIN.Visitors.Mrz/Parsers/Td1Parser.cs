using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Helpers;
namespace AIN.Visitors.Mrz.Parsers
{
    public class Td1Parser
    {
        public MrzFields Parse(ReadOnlySpan<char> line1, ReadOnlySpan<char> line2, ReadOnlySpan<char> line3)
        {
            var result = new MrzFields { IsValid = false, ErrorCode = MrzErrorCode.None };

            if (line1.Length != 30 || line2.Length != 30 || line3.Length != 30)
            {
                result.ErrorCode = MrzErrorCode.InvalidLength;
                return result;
            }

            if (line1[0] != 'I' && line1[0] != 'A' && line1[0] != 'C')
            {
                result.ErrorCode = MrzErrorCode.InvalidFormat;
                return result;
            }

            if (ContainsLowercase(line1) || ContainsLowercase(line2) || ContainsLowercase(line3))
            {
                result.ErrorCode = MrzErrorCode.InvalidCharacter;
                return result;
            }

            ReadOnlySpan<char> docNumberSpan = line1.Slice(5, 9);
            char docNumCheckDigitChar = line1[14];
            
            ReadOnlySpan<char> dobSpan = line2.Slice(0, 6);
            char dobCheckDigitChar = line2[6];
            
            ReadOnlySpan<char> expirySpan = line2.Slice(8, 6);
            char expiryCheckDigitChar = line2[14];
            
            ReadOnlySpan<char> compPart1 = line1.Slice(5, 25); // DocNum + Check + Optional1
            ReadOnlySpan<char> compPart2 = line2.Slice(0, 7);  // DOB + Check
            ReadOnlySpan<char> compPart3 = line2.Slice(8, 7);  // Expiry + Check
            ReadOnlySpan<char> compPart4 = line2.Slice(18, 11); // Optional2
            char compositeCheckDigitChar = line2[29];

            int expectedDocNumCd = CheckDigitHelper.Calculate(docNumberSpan);
            int expectedDobCd = CheckDigitHelper.Calculate(dobSpan);
            int expectedExpiryCd = CheckDigitHelper.Calculate(expirySpan);

            Span<char> compositeSpan = stackalloc char[50];
            compPart1.CopyTo(compositeSpan.Slice(0, 25));
            compPart2.CopyTo(compositeSpan.Slice(25, 7));
            compPart3.CopyTo(compositeSpan.Slice(32, 7));
            compPart4.CopyTo(compositeSpan.Slice(39, 11));
            
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
            
            result.PersonalNumber = CleanString(line1.Slice(15, 15)); 

            result.DateOfBirth = CleanString(dobSpan);
            char sexChar = line2[7];
            result.Sex = sexChar == '<' ? "U" : sexChar.ToString();
            result.DateOfExpiry = CleanString(expirySpan);
            result.Nationality = CleanString(line2.Slice(15, 3));
            
            ReadOnlySpan<char> nameRaw = line3.Slice(0, 30);
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