namespace Api.Core.Entidades;

/// <summary>
/// Contador monotónico global del change-feed. Se usa una sola fila (Id = 1).
/// En cada <c>SaveChanges</c> que toca entidades sincronizables se incrementa y se
/// estampa el nuevo valor en cada entidad modificada, garantizando versiones
/// estrictamente crecientes y únicas sin depender de <c>rowversion</c> (que no
/// soporta el provider InMemory usado en los tests).
/// </summary>
public class ContadorSync : Entidad
{
    public long UltimoValor { get; set; }
}
