namespace Domain.HomePlanner.Models.SaaS.Interfaces;

public interface IDeletableEntity
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedByUsuarioId { get; set; }
}
