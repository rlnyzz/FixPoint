using System.Security.Claims;
using FixPoint.Api.Data;
using FixPoint.Api.DTOs;
using FixPoint.Api.Models;
using FixPoint.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FixPoint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master")]
public class MasterController : ControllerBase
{
    private readonly EstimateService _estimateService;
    private readonly AppDbContext _context;

    // Внедряем и сервис, и контекст базы данных
    public MasterController(EstimateService estimateService, AppDbContext context)
    {
        _estimateService = estimateService;
        _context = context;
    }

    // 1. Создание сметы мастером
    [HttpPost("estimates")]
    public async Task<IActionResult> CreateEstimate([FromBody] CreateEstimateDto dto)
    {
        try
        {
            var masterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var rawToken = await _estimateService.CreateEstimateAsync(dto, masterId);
            
            return Ok(new { 
                message = "Смета создана. Отправьте ссылку клиенту.",
                publicToken = rawToken,
                publicLink = $"/api/Public/estimate/{rawToken}"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 2. Ответ мастера на обратную связь по смете
    [HttpPost("estimates/{id}/reply")]
    public async Task<IActionResult> ReplyToFeedback(int id, [FromBody] CreateFeedbackDto dto)
    {
        var masterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var estimate = await _context.RepairEstimates.FindAsync(id);
        if (estimate == null) return NotFound("Смета не найдена");

        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest("Сообщение не может быть пустым");

        var feedback = new EstimateFeedback
        {
            EstimateId = id,
            AuthorId = masterId,
            AuthorRole = "Master",
            Type = "MasterReply",
            Message = dto.Message
        };

        _context.EstimateFeedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ответ отправлен клиенту" });
    }
}