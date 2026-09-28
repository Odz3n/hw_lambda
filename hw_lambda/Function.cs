using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using hw_lambda.Models;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace hw_lambda;

public class Function
{
    
    public Task FunctionHandler(SQSEvent sQSEvent, ILambdaContext context)
    {
        foreach (var record in sQSEvent.Records)
        {
            try
            {
                var order = JsonSerializer.Deserialize<OrderRequest>(
                    record.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (order is null)
                    throw new InvalidOperationException("Order message is empty.");

                Validate(order);

                var calculation = CalculatePrice(order);

                context.Logger.LogLine(
                    FormatOrderLog(order, calculation, record.MessageId));

            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Failed to process message {record.MessageId}: {ex}");
                throw;
            }
        }
        return Task.CompletedTask;
    }
    private void Validate(OrderRequest order)
    {
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            throw new ArgumentException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(order.ProductName))
            throw new ArgumentException("Product name is required.");

        if (order.Price <= 0)
            throw new ArgumentOutOfRangeException(nameof(order.Price));

        if (order.Quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(order.Quantity));

        if (!Enum.IsDefined(order.DeliveryMethod))
            throw new ArgumentOutOfRangeException(nameof(order.DeliveryMethod));
    }
    private OrderCalculation CalculatePrice(OrderRequest order)
    {
        // Raw price
        var subtotal = order.Price * order.Quantity;

        // Specific discount rule xD
        var discountPercentage = subtotal >= 100m ? 10m : 0m;

        var discount = decimal.Round(subtotal * discountPercentage / 100m, 2, MidpointRounding.AwayFromZero);

        var deliveryPrice = order.DeliveryMethod switch
        {
            DeliveryMethod.Pickup => 0m,
            DeliveryMethod.Courier => 5m,
            DeliveryMethod.Express => 12m,
            _ => throw new ArgumentOutOfRangeException(
                nameof(order.DeliveryMethod))
        };

        var total = subtotal - discount + deliveryPrice;

        // DTO
        return new OrderCalculation
        {
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            Discount = discount,
            DeliveryPrice = deliveryPrice,
            Total = decimal.Round(total, 2, MidpointRounding.AwayFromZero)
        };
    }
    private string FormatOrderLog(
        OrderRequest order,
        OrderCalculation calculation,
        string messageId)
    {
        return $"""
        Order processed successfully
        Message ID: {messageId}
        Customer: {order.CustomerName}
        Product: {order.ProductName}
        Unit price: {order.Price:F2} EUR
        Quantity: {order.Quantity}
        Subtotal: {calculation.Subtotal:F2} EUR
        Discount: {calculation.Discount:F2} EUR ({calculation.DiscountPercentage}%)
        Delivery method: {order.DeliveryMethod}
        Delivery price: {calculation.DeliveryPrice:F2} EUR
        Final total: {calculation.Total:F2} EUR
        """;
    }
}