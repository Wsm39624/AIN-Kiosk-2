using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using AIN.Visitors.Mrz.Models;
using AIN.Visitors.Mrz.Helpers;

namespace AIN.Visitors.Mrz.Parsers
{
    public class Td3Parser
    {
        public MrzFields Parse(ReadOnlySpan<char> line1, ReadOnlySpan<char> line2)
        {
            var result = new MrzFields { IsValid = false, ErrorCode = MrzErrorCode.None };

            if (line1.Length != 44 || line2.Length != 44)
            {
                result.ErrorCode = MrzErrorCode.InvalidLength;
                return result;
            }

            if (line1[0] != 'P')
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
            
            ReadOnlySpan<char> compositePart1 = line2.Slice(0, 10);
            ReadOnlySpan<char> compositePart2 = line2.Slice(13, 7);
            ReadOnlySpan<char> compositePart3 = line2.Slice(21, 22);
            char compositeCheckDigitChar = line2[43];


            int expectedDocNumCd = CheckDigitHelper.Calculate(docNumberSpan);
            int expectedDobCd = CheckDigitHelper.Calculate(dobSpan);
            int expectedExpiryCd = CheckDigitHelper.Calculate(expirySpan);

            Span<char> compositeSpan = stackalloc char[39];
            compositePart1.CopyTo(compositeSpan.Slice(0, 10));
            compositePart2.CopyTo(compositeSpan.Slice(10, 7));
            compositePart3.CopyTo(compositeSpan.Slice(17, 22));
            
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
            result.IssuingState = line1.Slice(2, 3).ToString();
            
            ReadOnlySpan<char> nameRaw = line1.Slice(5, 39);
            int separatorIndex = nameRaw.IndexOf("<<".AsSpan());
            if (separatorIndex != -1)
            {
                result.PrimaryIdentifier = CleanString(nameRaw.Slice(0, separatorIndex), replaceWithSpace: true);
                result.SecondaryIdentifier = CleanString(nameRaw.Slice(separatorIndex + 2), replaceWithSpace: true);
            }

            result.DocumentNumber = CleanString(docNumberSpan);
            result.Nationality = line2.Slice(10, 3).ToString();
            result.DateOfBirth = dobSpan.ToString(); 
            
            char sexChar = line2[20];
            result.Sex = sexChar == '<' ? "U" : sexChar.ToString();
            
            result.DateOfExpiry = expirySpan.ToString();
            result.PersonalNumber = CleanString(line2.Slice(28, 14));

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
    
    while (end >= 0 && span[end] == '<')
    {
        end--;
    }
    
    if (end < 0) return string.Empty;
    
    ReadOnlySpan<char> trimmed = span.Slice(0, end + 1);
    
    Span<char> buffer = stackalloc char[trimmed.Length];
    int actualLength = 0;
    
    if (!replaceWithSpace)
    {
        foreach (char c in trimmed)
        {
            if (c != '<') 
            {
                buffer[actualLength++] = c;
            }
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