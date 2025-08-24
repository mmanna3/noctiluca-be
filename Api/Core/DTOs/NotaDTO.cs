using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class NotaDTO : DTO
{
    [Required]
    public required string Titulo { get; set; }
    public string? Cuerpo { get; set; }
    public DateTime FechaHora { get; set; }
    public int CarpetaId { get; set; }
    public string? CarpetaTitulo { get; set; }
}