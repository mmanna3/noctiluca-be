using Api.Core.Enums;

namespace Api.Core.DTOs;

public class HistoricoObjetivoDTO : DTO
{
    public TipoListaObjetivoEnum Tipo { get; set; }

    public required string ClavePeriodo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int CantidadItems { get; set; }

    public int CantidadCompletados { get; set; }
}
