namespace Api.Core.DTOs;

public class RegistroHabitoDTO : DTO
{
    public int HabitoId { get; set; }

    public DateTime Fecha { get; set; }

    public bool? ValorBooleano { get; set; }

    public int? ValorNumerico { get; set; }
}
