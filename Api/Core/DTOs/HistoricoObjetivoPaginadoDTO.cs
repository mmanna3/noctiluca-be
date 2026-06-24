namespace Api.Core.DTOs;

public class HistoricoObjetivoPaginadoDTO
{
    public ICollection<HistoricoObjetivoDTO> Items { get; set; } = new List<HistoricoObjetivoDTO>();

    public int Pagina { get; set; }

    public int Tamano { get; set; }

    public int Total { get; set; }
}
