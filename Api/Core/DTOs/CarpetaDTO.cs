using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class CarpetaDTO : DTO
{
    [Required]
    public required string Titulo { get; set; }
    public ICollection<NotaDTO>? Notas { get; set; }
    public int CantidadDeNotas => Notas?.Count ?? 0;
}