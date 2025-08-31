using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Carpeta : Entidad
{
    [MaxLength(100)]
    public required string Titulo { get; set; }
    public virtual ICollection<Escrito> Escritos { get; set; } = null!;
    
    public bool RequiereAutenticacion { get; set; }
    
    public int CriterioDeOrdenId { get; set; }
    public virtual CriterioDeOrden CriterioDeOrden { get; set; }
}