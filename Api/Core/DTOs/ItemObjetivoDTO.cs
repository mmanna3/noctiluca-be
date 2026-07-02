using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class ItemObjetivoDTO : DTO
{
    [Required]
    public required string Texto { get; set; }

    public bool Completado { get; set; }

    public int Posicion { get; set; }

    public DateTime? FechaCompletado { get; set; }

    /// <summary>Tipo y clave de la lista dueña (se completan en el pull de sync).</summary>
    public TipoListaObjetivoEnum? ListaTipo { get; set; }
    public string? ListaClavePeriodo { get; set; }

    public Guid ClientId { get; set; }
    public long Version { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
