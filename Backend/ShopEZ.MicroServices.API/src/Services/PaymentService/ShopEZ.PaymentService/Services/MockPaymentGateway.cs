namespace ShopEZ.PaymentService.Services
{
    public static class MockPaymentGateway
    {
        public static (bool Success, string TransactionId, string FailureReason)
            Charge(string cardNumber, decimal amount, string method)
        {
            if (!string.Equals(method, "Card", StringComparison.OrdinalIgnoreCase))
                return (true, GenerateTransactionId(), string.Empty);

            string last4 = cardNumber.Replace(" ", "").Replace("-", "");
            last4 = last4.Length >= 4 ? last4[^4..] : last4;

            if (last4 == "0000")
                return (false, string.Empty, "Card declined by issuer.");

            if (last4 == "9999")
                return (false, string.Empty,
                    "Payment gateway timeout. Please try again.");

            bool approved = new Random().NextDouble() < 0.95;

            return approved
                ? (true, GenerateTransactionId(), string.Empty)
                : (false, string.Empty, "Insufficient funds.");
        }

        private static string GenerateTransactionId()
            => $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];
    }
}