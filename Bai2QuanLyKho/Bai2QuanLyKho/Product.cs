using System.Text.Json.Serialization;

namespace Bai2QuanLyKho;   // đổi cho khớp namespace trong Program.cs của bạn

public class Product
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    // Chỉ lưu ĐƯỜNG DẪN ảnh, không lưu ảnh vào object (đúng yêu cầu đề)
    public string ImagePath { get; set; } = "";

    // Hai thuộc tính chỉ để hiển thị trên Grid, không ghi ra JSON
    [JsonIgnore]
    public string ImageFileName => string.IsNullOrEmpty(ImagePath) ? "" : Path.GetFileName(ImagePath);

    [JsonIgnore]
    public decimal TotalValue => Price * Quantity;
}
