using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class Carpeta : Entidad
{
    [MaxLength(100)]
    public required string Titulo { get; set; }
    public virtual ICollection<Nota> Notas { get; set; } = null!;
    
    public bool RequiereAutenticacion { get; set; }
}