using System.Security.Claims;
using FixPoint.Api.Data;
using FixPoint.Api.DTOs;
using FixPoint.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixPoint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RequestsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Клиент создает заявку (без изменений)
    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var device = new Device
        {
            OwnerId = userId,
            Type = dto.DeviceType,
            Brand = dto.Brand,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber
        };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var request = new RepairRequest
        {
            ClientId = userId,
            DeviceId = device.Id,
            Description = dto.Description,
            Status = "New"
        };
        _context.RepairRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Заявка создана", requestId = request.Id });
    }

    // 2. Клиент смотрит свои заявки (без изменений)
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var requests = await _context.RepairRequests
            .Include(r => r.Device)
            .Include(r => r.Estimates)
            .Where(r => r.ClientId == userId)
            .ToListAsync();

        return Ok(requests);
    }

    // 3. НОВЫЙ МЕТОД: Менеджер назначает мастера
    public class AssignMasterDto
    {
        public int MasterId { get; set; }
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Manager")] // Доступ только менеджеру!
    public async Task<IActionResult> AssignMaster(int id, [FromBody] AssignMasterDto dto)
    {
        var request = await _context.RepairRequests.FindAsync(id);
        if (request == null) return NotFound("Заявка не найдена");
        
        if (request.Status != "New") 
            return BadRequest("Можно назначать мастера только на новые заявки (статус New)");

        // Проверяем, что переданный ID действительно принадлежит мастеру
        var masterExists = await _context.Users.AnyAsync(u => u.Id == dto.MasterId && u.Role == "Master");
        if (!masterExists) 
            return BadRequest("Пользователь с таким ID не найден или не является мастером");

        request.MasterId = dto.MasterId;
        request.Status = "Assigned";
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Мастер успешно назначен на заявку" });
    }
}