using System.ComponentModel.DataAnnotations;

namespace Api.Core.Entidades;

public class ItemObjetivo : Entidad
{
    public int ListaObjetivoId { get; set; }
    public virtual ListaObjetivo ListaObjetivo { get; set; } = null!;

    [Required, MaxLength(200)]
    public required string Texto { get; set; }

    public bool Completado { get; set; }

    public int Posicion { get; set; }

    public DateTime? FechaCompletado { get; set; }
}
