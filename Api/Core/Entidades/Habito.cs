using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.Entidades;

public class Habito : EntidadSincronizable
{
    [Required, MaxLength(50)]
    public required string Nombre { get; set; }

    public TipoHabitoEnum Tipo { get; set; }

    public bool Activo { get; set; } = true;

    public int Posicion { get; set; }

    public int? MetaMinutos { get; set; }

    public virtual ICollection<RegistroHabito> Registros { get; set; } = new List<RegistroHabito>();
}
