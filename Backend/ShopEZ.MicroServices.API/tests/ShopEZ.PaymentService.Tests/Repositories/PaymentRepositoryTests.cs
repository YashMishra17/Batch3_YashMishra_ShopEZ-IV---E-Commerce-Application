using Microsoft.EntityFrameworkCore;
using ShopEZ.PaymentService.Data;
using ShopEZ.PaymentService.Models;
using ShopEZ.PaymentService.Repositories;
using ShopEZ.PaymentService.Tests.Helpers;
using Xunit;

namespace ShopEZ.PaymentService.Tests.Repositories
{
    public class PaymentRepositoryTests
    {
        private static PaymentDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new PaymentDbContext(options);
        }

        private static PaymentRepository CreateRepository(PaymentDbContext ctx)
            => new PaymentRepository(ctx);

        // ─────────────────────────────────────────────────────────────────────
        // GetByIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingPayment_ReturnsPayment()
        {
            await using var ctx = CreateContext(nameof(GetByIdAsync_ExistingPayment_ReturnsPayment));
            var payment = PaymentTestData.PaidPayment(paymentId: 0);
            await ctx.Payments.AddAsync(payment);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            Payment? result = await repo.GetByIdAsync(payment.PaymentId);

            Assert.NotNull(result);
            Assert.Equal(payment.PaymentId, result!.PaymentId);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetByIdAsync_NonExistentId_ReturnsNull));
            var repo = CreateRepository(ctx);

            Payment? result = await repo.GetByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsAllFields()
        {
            await using var ctx = CreateContext(nameof(GetByIdAsync_ReturnsAllFields));
            var payment = PaymentTestData.PaidPayment(paymentId: 0);
            await ctx.Payments.AddAsync(payment);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            Payment? result = await repo.GetByIdAsync(payment.PaymentId);

            Assert.NotNull(result);
            Assert.Equal("Paid", result!.Status);
            Assert.Equal("Card", result.Method);
            Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
            Assert.NotNull(result.ProcessedAt);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetByOrderIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByOrderIdAsync_ExistingOrderId_ReturnsPayment()
        {
            await using var ctx = CreateContext(nameof(GetByOrderIdAsync_ExistingOrderId_ReturnsPayment));
            var payment = PaymentTestData.PaidPayment(paymentId: 0, orderId: 10);
            await ctx.Payments.AddAsync(payment);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            Payment? result = await repo.GetByOrderIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal(10, result!.OrderId);
        }

        [Fact]
        public async Task GetByOrderIdAsync_NonExistentOrderId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetByOrderIdAsync_NonExistentOrderId_ReturnsNull));
            var repo = CreateRepository(ctx);

            Payment? result = await repo.GetByOrderIdAsync(9999);

            Assert.Null(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetByUserIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_UserWithTwoPayments_ReturnsBoth()
        {
            await using var ctx = CreateContext(nameof(GetByUserIdAsync_UserWithTwoPayments_ReturnsBoth));
            await ctx.Payments.AddRangeAsync(
                PaymentTestData.PaidPayment(paymentId: 0, orderId: 10, userId: 1),
                PaymentTestData.PaidPayment(paymentId: 0, orderId: 11, userId: 1),
                PaymentTestData.PaidPayment(paymentId: 0, orderId: 12, userId: 2));
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var result = (await repo.GetByUserIdAsync(1)).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.UserId));
        }

        [Fact]
        public async Task GetByUserIdAsync_UserWithNoPayments_ReturnsEmpty()
        {
            await using var ctx = CreateContext(nameof(GetByUserIdAsync_UserWithNoPayments_ReturnsEmpty));
            var repo = CreateRepository(ctx);

            var result = await repo.GetByUserIdAsync(999);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_OrderedByCreatedAtDescending()
        {
            await using var ctx = CreateContext(nameof(GetByUserIdAsync_OrderedByCreatedAtDescending));

            var older = new Payment
            {
                OrderId = 30,
                UserId = 5,
                Amount = 10m,
                Status = "Paid",
                Method = "COD",
                TransactionId = "",
                FailureReason = "",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            var newer = new Payment
            {
                OrderId = 31,
                UserId = 5,
                Amount = 20m,
                Status = "Paid",
                Method = "COD",
                TransactionId = "",
                FailureReason = "",
                CreatedAt = DateTime.UtcNow
            };

            await ctx.Payments.AddRangeAsync(older, newer);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var result = (await repo.GetByUserIdAsync(5)).ToList();

            Assert.True(result[0].CreatedAt >= result[1].CreatedAt);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CreateAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidPayment_ReturnsSavedPaymentWithGeneratedId()
        {
            await using var ctx = CreateContext(nameof(CreateAsync_ValidPayment_ReturnsSavedPaymentWithGeneratedId));
            var repo = CreateRepository(ctx);

            var payment = new Payment
            {
                OrderId = 40,
                UserId = 1,
                Amount = 29.99m,
                Status = "Pending",
                Method = "Card",
                TransactionId = "",
                FailureReason = "",
                CreatedAt = DateTime.UtcNow
            };

            Payment created = await repo.CreateAsync(payment);

            Assert.True(created.PaymentId > 0);
            Assert.Equal(29.99m, created.Amount);
        }

        [Fact]
        public async Task CreateAsync_Payment_IsPersisted()
        {
            await using var ctx = CreateContext(nameof(CreateAsync_Payment_IsPersisted));
            var repo = CreateRepository(ctx);

            var payment = new Payment
            {
                OrderId = 41,
                UserId = 1,
                Amount = 50m,
                Status = "Pending",
                Method = "COD",
                TransactionId = "",
                FailureReason = "",
                CreatedAt = DateTime.UtcNow
            };

            Payment created = await repo.CreateAsync(payment);
            Payment? fetched = await repo.GetByIdAsync(created.PaymentId);

            Assert.NotNull(fetched);
            Assert.Equal(50m, fetched!.Amount);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ExistingPayment_StatusUpdated()
        {
            await using var ctx = CreateContext(nameof(UpdateAsync_ExistingPayment_StatusUpdated));
            var payment = PaymentTestData.PendingPayment(paymentId: 0);
            await ctx.Payments.AddAsync(payment);
            await ctx.SaveChangesAsync();

            payment.Status = "Paid";
            payment.TransactionId = "TXN-NEW123";
            payment.ProcessedAt = DateTime.UtcNow;

            var repo = CreateRepository(ctx);
            Payment? updated = await repo.UpdateAsync(payment);

            Assert.NotNull(updated);
            Assert.Equal("Paid", updated!.Status);
        }

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            await using var ctx = CreateContext(nameof(UpdateAsync_PersistsChanges));
            var payment = PaymentTestData.PendingPayment(paymentId: 0);
            await ctx.Payments.AddAsync(payment);
            await ctx.SaveChangesAsync();

            payment.Status = "Failed";
            payment.FailureReason = "Card declined by issuer.";

            var repo = CreateRepository(ctx);
            await repo.UpdateAsync(payment);

            Payment? fetched = await repo.GetByIdAsync(payment.PaymentId);
            Assert.Equal("Failed", fetched!.Status);
            Assert.Equal("Card declined by issuer.", fetched.FailureReason);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ExistsForOrderAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsForOrderAsync_OrderHasPayment_ReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(ExistsForOrderAsync_OrderHasPayment_ReturnsTrue));
            var payment = PaymentTestData.PaidPayment(paymentId: 0, orderId: 50);
            await ctx.Payments.AddAsync(payment);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            bool exists = await repo.ExistsForOrderAsync(50);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsForOrderAsync_OrderHasNoPayment_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(ExistsForOrderAsync_OrderHasNoPayment_ReturnsFalse));
            var repo = CreateRepository(ctx);

            bool exists = await repo.ExistsForOrderAsync(9999);

            Assert.False(exists);
        }
    }
}