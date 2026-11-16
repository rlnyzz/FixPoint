namespace FixPoint.Api.Models;

public class Device
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // Ноутбук, Телефон и т.д.
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}

public class RepairRequest
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    
    public int? MasterId { get; set; } // Может быть null, пока не назначен
    public User? Master { get; set; }

    public string Status { get; set; } = "New"; // New, Assigned, InProgress и т.д.
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства для связей (1 ко многим)
    public ICollection<RepairEstimate> Estimates { get; set; } = new List<RepairEstimate>();
}

public class RepairEstimate
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public RepairRequest Request { get; set; } = null!;
    
    public decimal TotalPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; } = 1; // Для версионирования смет
    public string Status { get; set; } = "Sent"; // Sent, Approved, Rejected, Superseded
    
    // Поля для публичной ссылки
    public string? ApprovalTokenHash { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class EstimateFeedback
{
    public int Id { get; set; }
    public int EstimateId { get; set; }
    public RepairEstimate Estimate { get; set; } = null!;
    
    // Кто написал (может быть null, если писал гость по токену)
    public int? AuthorId { get; set; }
    public User? Author { get; set; } 
    
    public string AuthorRole { get; set; } = "Client"; // Client, Master
    public string Type { get; set; } = "Question"; // Question, Comment, RevisionRequest, MasterReply
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}