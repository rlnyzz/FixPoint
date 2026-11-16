namespace FixPoint.Api.DTOs;

public class UpdateRoleDto
{
    public string NewRole { get; set; } = string.Empty; // Client, Master, Manager, Admin
}

public class CreateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
}