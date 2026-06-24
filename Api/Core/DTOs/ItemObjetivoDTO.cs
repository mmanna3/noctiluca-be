using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ItemObjetivoDTO : DTO
{
    [Required]
    public required string Texto { get; set; }

    public bool Completado { get; set; }

    public int Posicion { get; set; }

    public DateTime? FechaCompletado { get; set; }
}
