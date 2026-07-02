namespace Api.Core.Entidades;

/// <summary>
/// Registro de operaciones de push ya procesadas, para garantizar idempotencia:
/// si el cliente reintenta un push con el mismo <see cref="ClientOpId"/> (por un
/// timeout de red, por ejemplo), el servidor devuelve el resultado previo en vez
/// de volver a aplicar la operación.
/// </summary>
public class SyncOpLog : Entidad
{
    public Guid ClientOpId { get; set; }

    public DateTime ProcesadoEn { get; set; }

    public string? ResultadoJson { get; set; }
}
