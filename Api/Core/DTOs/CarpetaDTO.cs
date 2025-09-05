using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class CarpetaDTO : DTO
{
    [Required]
    public required string Titulo { get; set; }
    public ICollection<EscritoDTO>? Escritos { get; set; }
    public int CantidadDeEscritos => Escritos?.Count ?? 0;
    
    public bool RequiereAutenticacion { get; set; }
    
    public int Posicion { get; set; }
    
    public CriterioDeOrdenEnum CriterioDeOrden { get; set; }
}