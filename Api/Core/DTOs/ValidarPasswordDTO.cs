using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ValidarPasswordDTO
{
    [Required]
    public required string Password { get; set; }
}
