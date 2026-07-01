namespace Api.Core.Entidades;

/// <summary>
/// Clase base para las entidades que participan de la sincronización offline-first.
///
/// No genera una tabla propia: al ser abstracta y no estar mapeada como un
/// <c>DbSet</c>, EF Core simplemente agrega sus columnas (<see cref="ClientId"/>,
/// <see cref="Version"/> y <see cref="ActualizadoEn"/>) a la tabla de cada entidad
/// que herede de ella. Sirve además como marcador explícito: si una entidad hereda
/// de acá, participa del sync.
/// </summary>
public abstract class EntidadSincronizable : Entidad
{
    /// <summary>
    /// Identidad estable generada por el cliente (o por el servidor en su defecto).
    /// Es la clave canónica usada por el frontend y por la sincronización, de modo
    /// que un alta hecha offline no dependa del <c>Id</c> autoincremental del servidor.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Versión monotónica asignada por el servidor en cada alta/modificación.
    /// Actúa como cursor del change-feed (pull "desde" un valor) y como token de
    /// concurrencia para la resolución last-write-wins.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Momento (UTC) de la última modificación server-side. Se usa como desempate
    /// en la resolución de conflictos y para mostrar en la UI.
    /// </summary>
    public DateTime ActualizadoEn { get; set; }
}
