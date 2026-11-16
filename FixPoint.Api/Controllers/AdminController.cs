using FixPoint.Api.Data;
using FixPoint.Api.DTOs;
using FixPoint.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixPoint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Доступ только для Администратора
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // --- УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ ---

    // 1. Получить список всех пользователей
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.Id, u.Email, u.FullName, u.Role, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }

    // 2. Изменить роль пользователя
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("Пользователь не найден");

        // Защита: нельзя просто так взять и удалить последнего админа (опционально)
        if (user.Role == "Admin" && dto.NewRole != "Admin")
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
            if (adminCount <= 1) return BadRequest("Нельзя забрать права у последнего администратора");
        }

        user.Role = dto.NewRole;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Роль пользователя изменена на {dto.NewRole}" });
    }

    // 3. Удалить пользователя
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("Пользователь не найден");
        
        // В реальной системе тут было бы мягкое удаление (IsDeleted = true)
        // Но для MVP просто удаляем (осторожно с внешними ключами в БД!)
        try 
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Пользователь удален" });
        }
        catch (DbUpdateException)
        {
            return BadRequest("Нельзя удалить пользователя, у которого есть активные заявки");
        }
    }

    // --- УПРАВЛЕНИЕ УСЛУГАМИ (СПРАВОЧНИК) ---

    // 4. Создать услугу
    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
    {
        var service = new RepairService
        {
            Name = dto.Name,
            BasePrice = dto.BasePrice
        };
        _context.RepairServices.Add(service);
        await _context.SaveChangesAsync();
        return Ok(service);
    }

    // 5. Изменить услугу
    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] CreateServiceDto dto)
    {
        var service = await _context.RepairServices.FindAsync(id);
        if (service == null) return NotFound("Услуга не найдена");

        service.Name = dto.Name;
        service.BasePrice = dto.BasePrice;
        await _context.SaveChangesAsync();
        return Ok(service);
    }

    // 6. Удалить услугу
    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.RepairServices.FindAsync(id);
        if (service == null) return NotFound("Услуга не найдена");

        _context.RepairServices.Remove(service);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Услуга удалена" });
    }
}