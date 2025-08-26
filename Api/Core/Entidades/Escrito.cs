using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entidades;

public class Escrito : Entidad
{
    [MaxLength(255)]
    public required string Titulo { get; set; }
    
    [MaxLength(5000)]
    public string? Cuerpo { get; set; }
    public DateTime FechaHoraCreacion { get; set; }
    
    public DateTime? FechaHoraEdicion { get; set; }
    
    [ForeignKey("Carpeta")] 
    public required int CarpetaId { get; set; }
    public virtual Carpeta? Carpeta { get; set; }
    
    public bool EstaEnPapelera { get; set; }
}