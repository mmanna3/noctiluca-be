namespace Api.Core.Entidades;

/// <summary>
/// Lápida de un borrado permanente. Como las entidades se eliminan físicamente,
/// el pull "desde" un cursor no tendría forma de avisar a otros dispositivos que
/// algo dejó de existir. El tombstone conserva el <see cref="ClientId"/> y una
/// <see cref="Version"/> del change-feed para que el cliente aplique la baja local.
/// </summary>
public class Tombstone : Entidad
{
    public required string TipoEntidad { get; set; }

    public Guid ClientId { get; set; }

    public long Version { get; set; }

    public DateTime EliminadoEn { get; set; }
}
