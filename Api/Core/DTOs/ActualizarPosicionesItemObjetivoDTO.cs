using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ActualizarPosicionesItemObjetivoDTO
{
    [Required]
    public required List<PosicionItemObjetivoDTO> Posiciones { get; set; }
}

public class PosicionItemObjetivoDTO
{
    [Required]
    public int IdDeItem { get; set; }

    [Required]
    public int Posicion { get; set; }
}
