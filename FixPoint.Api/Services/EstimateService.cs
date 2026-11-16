using System.Security.Cryptography;
using System.Text;
using FixPoint.Api.Data;
using FixPoint.Api.DTOs;
using FixPoint.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FixPoint.Api.Services;

public class EstimateService
{
    private readonly AppDbContext _context;

    public EstimateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateEstimateAsync(CreateEstimateDto dto, int masterId)
    {
        var request = await _context.RepairRequests.FindAsync(dto.RequestId);
        if (request == null) throw new Exception("Заявка не найдена");
        if (request.MasterId != masterId) throw new Exception("Вы не назначены на эту заявку");

        // Генерируем случайный токен
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = HashToken(rawToken);

        var estimate = new RepairEstimate
        {
            RequestId = dto.RequestId,
            TotalPrice = dto.TotalPrice,
            Description = dto.Description,
            Version = 1,
            Status = "Sent",
            ApprovalTokenHash = tokenHash,
            TokenExpiresAt = DateTime.UtcNow.AddDays(3)
        };

        _context.RepairEstimates.Add(estimate);
        
        request.Status = "AwaitingApproval";
        request.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        // В реальном проекте здесь мы бы отправили Email.
        // Пока просто возвращаем токен для теста.
        return rawToken; 
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}