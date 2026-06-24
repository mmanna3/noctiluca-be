using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.Entidades;

public class ListaObjetivo : Entidad
{
    public TipoListaObjetivoEnum Tipo { get; set; }

    [Required, MaxLength(20)]
    public required string ClavePeriodo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<ItemObjetivo> Items { get; set; } = new List<ItemObjetivo>();
}
