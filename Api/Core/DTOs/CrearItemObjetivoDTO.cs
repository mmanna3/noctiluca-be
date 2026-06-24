using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.DTOs;

public class CrearItemObjetivoDTO
{
    public int? ListaObjetivoId { get; set; }

    public TipoListaObjetivoEnum? Tipo { get; set; }

    public string? ClavePeriodo { get; set; }

    [Required, MaxLength(200)]
    public required string Texto { get; set; }
}
