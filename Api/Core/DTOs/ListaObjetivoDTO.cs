using Api.Core.Enums;

namespace Api.Core.DTOs;

public class ListaObjetivoDTO : DTO
{
    public TipoListaObjetivoEnum Tipo { get; set; }

    public required string ClavePeriodo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<ItemObjetivoDTO> Items { get; set; } = new List<ItemObjetivoDTO>();

    public string? AdvertenciaLimite { get; set; }

    public Guid ClientId { get; set; }
    public long Version { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
