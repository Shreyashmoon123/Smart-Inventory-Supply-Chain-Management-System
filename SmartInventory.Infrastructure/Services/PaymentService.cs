
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.Payments;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Enums;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentResponseDto>> GetAllAsync()
    {
        var payments = await _context.Payments
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync();

        return payments.Select(MapToResponse).ToList();
    }

    public async Task<PaymentResponseDto?> GetByIdAsync(int id)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(x => x.PaymentId == id);

        return payment == null ? null : MapToResponse(payment);
    }

    public async Task<List<PaymentResponseDto>> GetBySalesOrderIdAsync(
        int salesOrderId)
    {
        var orderExists = await _context.SalesOrders
            .AnyAsync(x => x.SalesOrderId == salesOrderId);

        if (!orderExists)
            throw new InvalidOperationException("Sales order not found.");

        var payments = await _context.Payments
            .Where(x => x.SalesOrderId == salesOrderId)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync();

        return payments.Select(MapToResponse).ToList();
    }

    public async Task<PaymentSummaryDto> GetSummaryAsync(int salesOrderId)
    {
        var order = await _context.SalesOrders
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId);

        if (order == null)
            throw new InvalidOperationException("Sales order not found.");

        var totalPaid = order.Payments
            .Where(x => x.Status == PaymentStatus.Paid)
            .Sum(x => x.Amount);

        var remainingAmount = Math.Max(
            0, order.TotalAmount - totalPaid);

        var paymentStatus = totalPaid <= 0
            ? "Pending"
            : totalPaid < order.TotalAmount
                ? "PartiallyPaid"
                : "Paid";

        return new PaymentSummaryDto
        {
            SalesOrderId = order.SalesOrderId,
            OrderAmount = order.TotalAmount,
            TotalPaid = totalPaid,
            RemainingAmount = remainingAmount,
            PaymentStatus = paymentStatus,
            Payments = order.Payments
                .OrderByDescending(x => x.PaymentDate)
                .Select(MapToResponse)
                .ToList()
        };
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto)
    {
        if (dto.Amount <= 0)
            throw new InvalidOperationException(
                "Payment amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
            throw new InvalidOperationException(
                "Payment method is required.");

        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(x => x.SalesOrderId == dto.SalesOrderId);

        if (order == null)
            throw new InvalidOperationException("Sales order not found.");

        if (order.Status == SalesOrderStatus.Pending ||
            order.Status == SalesOrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Payment is not allowed for pending or cancelled sales orders.");
        }

        var totalPaid = await _context.Payments
            .Where(x =>
                x.SalesOrderId == dto.SalesOrderId &&
                x.Status == PaymentStatus.Paid)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        if (totalPaid >= order.TotalAmount)
            throw new InvalidOperationException(
                "This sales order is already fully paid.");

        if (dto.Amount > order.TotalAmount - totalPaid)
            throw new InvalidOperationException(
                $"Payment exceeds the remaining amount of {order.TotalAmount - totalPaid}.");

        var payment = new Payment
        {
            SalesOrderId = dto.SalesOrderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod.Trim(),
            TransactionReference = dto.TransactionReference?.Trim(),
            Status = PaymentStatus.Pending,
            PaymentDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto> UpdateStatusAsync(
        int id, string status)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var payment = await _context.Payments
                .Include(x => x.SalesOrder)
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            if (!Enum.TryParse<PaymentStatus>(
                status, true, out var newStatus))
            {
                throw new InvalidOperationException(
                    "Invalid payment status.");
            }

            if (newStatus == PaymentStatus.PartiallyPaid)
                throw new InvalidOperationException(
                    "PartiallyPaid is an order-level summary status, not an individual payment status.");

            var isValidTransition =
                (payment.Status == PaymentStatus.Pending &&
                 (newStatus == PaymentStatus.Paid ||
                  newStatus == PaymentStatus.Failed)) ||
                (payment.Status == PaymentStatus.Paid &&
                 newStatus == PaymentStatus.Refunded);

            if (!isValidTransition)
                throw new InvalidOperationException(
                    $"Cannot change payment status from {payment.Status} to {newStatus}.");

            if (newStatus == PaymentStatus.Paid)
            {
                var otherPaidAmount = await _context.Payments
                    .Where(x =>
                        x.SalesOrderId == payment.SalesOrderId &&
                        x.PaymentId != payment.PaymentId &&
                        x.Status == PaymentStatus.Paid)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var remainingAmount =
                    payment.SalesOrder.TotalAmount - otherPaidAmount;

                if (payment.Amount > remainingAmount)
                    throw new InvalidOperationException(
                        $"Payment exceeds the remaining amount of {remainingAmount}.");
            }

            payment.Status = newStatus;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(payment);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static PaymentResponseDto MapToResponse(Payment payment)
    {
        return new PaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            SalesOrderId = payment.SalesOrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status.ToString(),
            TransactionReference = payment.TransactionReference,
            PaymentDate = payment.PaymentDate
        };
    }
}
