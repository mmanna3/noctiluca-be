using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class HabitoDTO : DTO
{
    [Required, MaxLength(50)]
    public required string Nombre { get; set; }

    public TipoHabitoEnum Tipo { get; set; }

    public bool Activo { get; set; } = true;

    public int Posicion { get; set; }

    public int? MetaMinutos { get; set; }

    public int CantidadRegistros { get; set; }
}
