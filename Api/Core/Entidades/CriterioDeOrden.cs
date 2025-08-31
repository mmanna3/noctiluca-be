using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class CriterioDeOrden : Entidad
{
    [Required, MaxLength(50)]
    public string Criterio { get; set; } = string.Empty;

    public ICollection<Carpeta> Carpetas { get; set; } = new List<Carpeta>();
}