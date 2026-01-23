namespace MicaService.Domain.Entities;

public sealed class Section
{
    public string SectId { get; set; } = string.Empty;
    public string? SectName { get; set; }
    public string? DeptId { get; set; }
    public string? MgrEmpId { get; set; }
}
