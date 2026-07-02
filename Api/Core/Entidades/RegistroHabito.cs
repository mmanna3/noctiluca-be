using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entidades;

public class RegistroHabito : EntidadSincronizable
{
    public int HabitoId { get; set; }

    [ForeignKey(nameof(HabitoId))]
    public virtual Habito Habito { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public bool? ValorBooleano { get; set; }

    public int? ValorNumerico { get; set; }
}
