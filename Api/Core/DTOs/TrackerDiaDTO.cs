using Api.Core.Enums;

namespace Api.Core.DTOs;

public class TrackerDiaDTO
{
    public DateTime Fecha { get; set; }

    public List<HabitoTrackerItemDTO> Habitos { get; set; } = new();
}

public class HabitoTrackerItemDTO
{
    public int Id { get; set; }

    public required string Nombre { get; set; }

    public TipoHabitoEnum Tipo { get; set; }

    public int? MetaMinutos { get; set; }

    public int? RegistroId { get; set; }

    public bool? ValorBooleano { get; set; }

    public int? ValorNumerico { get; set; }
}
