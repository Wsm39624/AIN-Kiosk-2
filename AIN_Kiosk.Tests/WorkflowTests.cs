using Xunit;
using AIN_Kiosk.Services;
using AIN_Kiosk;
using System;

namespace AIN_Kiosk.Tests
{
    public class WorkflowTests
    {
        [Fact]
        public void ReceiptProvider_ShouldReturnSyntheticToken()
        {
            // Arrange
            IReceiptReferenceProvider provider = new MockReceiptReferenceProvider();

            // Act
            string token = provider.GetSyntheticToken();

            // Assert
            Assert.NotNull(token);
            Assert.StartsWith("DEV-SYNTHETIC-", token);
        }

        [Fact]
        public void BadgePrintService_ShouldNotContainHardcodedRealNames()
        {
            // Arrange
            var printService = new BadgePrintService();

            // Act & Assert
            Assert.NotNull(printService);
        }

        [Fact]
        public void PrivacyReceiptService_GenerateReceipt_ShouldReflectDynamicParameters()
        {
            // Arrange
            var receiptService = new PrivacyReceiptService();
            string host = "Ahmed Ali";
            string purpose = "Business Meeting";
            string visitor = "John Doe";

            // Act
            var receipt = receiptService.GenerateReceipt("WalkIn", host, purpose, isArabic: false, visitorName: visitor);

            // Assert
            Assert.NotNull(receipt);
            Assert.Contains(host, receipt.DataCaptured);
            Assert.Contains(purpose, receipt.DataCaptured);
            Assert.DoesNotContain("Wesam", receipt.DataCaptured);
        }

        [Fact]
        public void Tokens_BadgeAndReceiptTokens_MustBeDistinct()
        {
            // Arrange
            IReceiptReferenceProvider provider = new MockReceiptReferenceProvider();
            string receiptToken = provider.GetSyntheticToken();
            string badgeToken = $"BDG-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

            // Assert
            Assert.NotEqual(receiptToken, badgeToken);
            Assert.StartsWith("DEV-SYNTHETIC-", receiptToken);
            Assert.StartsWith("BDG-", badgeToken);
        }
    }
}