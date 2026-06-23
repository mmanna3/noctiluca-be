using Api.Core.Enums;

namespace Api.Core.DTOs;

public class ResumenSemanalDTO
{
    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public List<HabitoResumenDTO> Habitos { get; set; } = new();
}

public class HabitoResumenDTO
{
    public int Id { get; set; }

    public required string Nombre { get; set; }

    public TipoHabitoEnum Tipo { get; set; }

    public int? MetaMinutos { get; set; }

    public int DiasCumplidos { get; set; }

    public int DiasNoCumplidos { get; set; }

    public int DiasSinMarcar { get; set; }

    public int? TotalMinutos { get; set; }

    public double? PromedioMinutos { get; set; }

    public List<DiaResumenDTO> DetallePorDia { get; set; } = new();
}

public class DiaResumenDTO
{
    public DateTime Fecha { get; set; }

    public string Estado { get; set; } = "sin_marcar";

    public bool? ValorBooleano { get; set; }

    public int? ValorNumerico { get; set; }
}
