namespace FixPoint.Api.DTOs;

// То, что клиент присылает при создании заявки
public class CreateRequestDto
{
    public string DeviceType { get; set; } = string.Empty; // Например: "Ноутбук"
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// То, что мастер присылает для создания сметы
public class CreateEstimateDto
{
    public decimal TotalPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public int RequestId { get; set; }
}

// То, что возвращается клиенту (смета)
public class EstimateDto
{
    public int Id { get; set; }
    public decimal TotalPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}