using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class EditarItemObjetivoDTO
{
    [Required, MaxLength(200)]
    public required string Texto { get; set; }
}
