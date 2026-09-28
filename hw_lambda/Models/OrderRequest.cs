using System.ComponentModel.DataAnnotations;

namespace hw_lambda.Models;
public enum DeliveryMethod
{
    Pickup = 0,
    Courier = 1,
    Express = 2
}

public class OrderRequest
{
    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Customer name must contain 3–100 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must contain 2–100 characters.")]
    public string ProductName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;

    [EnumDataType(typeof(DeliveryMethod), ErrorMessage = "Select a valid delivery method.")]
    public DeliveryMethod DeliveryMethod { get; set; }
}
