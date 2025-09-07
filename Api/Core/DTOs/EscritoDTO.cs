using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class EscritoDTO : DTO
{
    public string? Titulo { get; set; }
    public string? Cuerpo { get; set; }
    public DateTime? FechaHoraCreacion { get; set; }
    public DateTime? FechaHoraEdicion { get; set; }
    public int CarpetaId { get; set; }
    public string? CarpetaTitulo { get; set; }
}