using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class ActualizarPosicionesDTO
{
    [Required]
    public required List<PosicionCarpetaDTO> Posiciones { get; set; }
}

public class PosicionCarpetaDTO
{
    [Required]
    public int IdDeCarpeta { get; set; }
    
    [Required]
    public int Posicion { get; set; }
}
