namespace Shared.Dtos;

public record CreateOrderRequest(string CustomerName, string CustomerEmail, int ProductId, int Quantity, decimal UnitPrice);
public record OrderResponse(int Id, string CustomerName, string CustomerEmail, int ProductId, int Quantity, decimal TotalPrice, DateTime OrderDate, string Status);
