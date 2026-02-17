namespace Api.Core.DTOs;

public class MoverEscritosDTO
{
    public required List<int> EscritoIds { get; set; }
    public required int CarpetaDestinoId { get; set; }
}
