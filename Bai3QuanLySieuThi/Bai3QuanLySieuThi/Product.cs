using System.Text.Json.Serialization;

namespace Bai3QuanLySieuThi;   // đổi cho khớp namespace trong Program.cs của bạn

public class Product
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImagePath { get; set; } = "";

    [JsonIgnore]
    public string ImageFileName => string.IsNullOrEmpty(ImagePath) ? "" : Path.GetFileName(ImagePath);

    [JsonIgnore]
    public decimal TotalValue => Price * Quantity;
}
