namespace Api.Core.DTOs;

public class RegistroHabitoDTO : DTO
{
    public int HabitoId { get; set; }

    public DateTime Fecha { get; set; }

    public bool? ValorBooleano { get; set; }

    public int? ValorNumerico { get; set; }

    /// <summary>ClientId del hábito dueño (se completa en el pull de sync).</summary>
    public Guid? HabitoClientId { get; set; }

    public Guid ClientId { get; set; }
    public long Version { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
