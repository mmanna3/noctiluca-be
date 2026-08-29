using System.Globalization;
using Api.Core.Enums;

namespace Api.Core.Otros;

public static class ObjetivoPeriodoUtil
{
    public const int LimiteRecomendadoDia = 7;

    public const int AniosPorLustro = 5;

    public static string ClaveDia(DateTime fecha) =>
        fecha.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string ClaveSemana(DateTime fecha)
    {
        var year = ISOWeek.GetYear(fecha);
        var week = ISOWeek.GetWeekOfYear(fecha);
        return $"{year}-W{week:D2}";
    }

    public static string ClaveMes(DateTime fecha) =>
        fecha.Date.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public static string ClaveAnio(DateTime fecha) =>
        fecha.Date.ToString("yyyy", CultureInfo.InvariantCulture);

    public static string ClaveLustro(DateTime fecha)
    {
        var anioInicio = AnioInicioLustro(fecha.Year);
        return $"{anioInicio:D4}-{anioInicio + AniosPorLustro - 1:D4}";
    }

    public static string ObtenerClavePeriodo(TipoListaObjetivoEnum tipo, DateTime fecha) =>
        tipo switch
        {
            TipoListaObjetivoEnum.Dia => ClaveDia(fecha),
            TipoListaObjetivoEnum.Semana => ClaveSemana(fecha),
            TipoListaObjetivoEnum.Mes => ClaveMes(fecha),
            TipoListaObjetivoEnum.Anio => ClaveAnio(fecha),
            TipoListaObjetivoEnum.Lustro => ClaveLustro(fecha),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };

    public static (DateTime inicio, DateTime fin) ObtenerRangoPeriodo(
        TipoListaObjetivoEnum tipo,
        DateTime fechaReferencia)
    {
        var fecha = fechaReferencia.Date;

        return tipo switch
        {
            TipoListaObjetivoEnum.Dia => (fecha, fecha),
            TipoListaObjetivoEnum.Semana => ObtenerRangoSemana(fecha),
            TipoListaObjetivoEnum.Mes => ObtenerRangoMes(fecha),
            TipoListaObjetivoEnum.Anio => ObtenerRangoAnio(fecha),
            TipoListaObjetivoEnum.Lustro => ObtenerRangoLustro(fecha),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    public static (DateTime inicio, DateTime fin) ObtenerRangoDesdeClave(
        TipoListaObjetivoEnum tipo,
        string clavePeriodo)
    {
        return tipo switch
        {
            TipoListaObjetivoEnum.Dia => ParsearClaveDia(clavePeriodo),
            TipoListaObjetivoEnum.Semana => ParsearClaveSemana(clavePeriodo),
            TipoListaObjetivoEnum.Mes => ParsearClaveMes(clavePeriodo),
            TipoListaObjetivoEnum.Anio => ParsearClaveAnio(clavePeriodo),
            TipoListaObjetivoEnum.Lustro => ParsearClaveLustro(clavePeriodo),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    private static (DateTime inicio, DateTime fin) ObtenerRangoSemana(DateTime fecha)
    {
        var year = ISOWeek.GetYear(fecha);
        var week = ISOWeek.GetWeekOfYear(fecha);
        var inicio = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        return (inicio, inicio.AddDays(6));
    }

    private static (DateTime inicio, DateTime fin) ObtenerRangoMes(DateTime fecha)
    {
        var inicio = new DateTime(fecha.Year, fecha.Month, 1);
        var fin = inicio.AddMonths(1).AddDays(-1);
        return (inicio, fin);
    }

    private static (DateTime inicio, DateTime fin) ObtenerRangoAnio(DateTime fecha)
    {
        var inicio = new DateTime(fecha.Year, 1, 1);
        var fin = inicio.AddYears(1).AddDays(-1);
        return (inicio, fin);
    }

    private static (DateTime inicio, DateTime fin) ObtenerRangoLustro(DateTime fecha)
    {
        var inicio = new DateTime(AnioInicioLustro(fecha.Year), 1, 1);
        var fin = inicio.AddYears(AniosPorLustro).AddDays(-1);
        return (inicio, fin);
    }

    // Ancla el lustro a bloques fijos de calendario que empiezan en años múltiplos de 5
    // (2025-2029, 2030-2034, ...). Es una ventana fija, no móvil.
    private static int AnioInicioLustro(int anio) => anio - anio % AniosPorLustro;

    private static (DateTime inicio, DateTime fin) ParsearClaveDia(string clave)
    {
        if (!DateTime.TryParseExact(
                clave,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
            throw new ArgumentException("Clave de día inválida", nameof(clave));

        return (fecha, fecha);
    }

    private static (DateTime inicio, DateTime fin) ParsearClaveSemana(string clave)
    {
        var partes = clave.Split("-W", StringSplitOptions.TrimEntries);
        if (partes.Length != 2
            || !int.TryParse(partes[0], out var year)
            || !int.TryParse(partes[1], out var week))
            throw new ArgumentException("Clave de semana inválida", nameof(clave));

        var inicio = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        return (inicio, inicio.AddDays(6));
    }

    private static (DateTime inicio, DateTime fin) ParsearClaveMes(string clave)
    {
        if (!DateTime.TryParseExact(
                clave + "-01",
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var inicio))
            throw new ArgumentException("Clave de mes inválida", nameof(clave));

        var fin = inicio.AddMonths(1).AddDays(-1);
        return (inicio, fin);
    }

    private static (DateTime inicio, DateTime fin) ParsearClaveAnio(string clave)
    {
        if (!int.TryParse(clave, NumberStyles.None, CultureInfo.InvariantCulture, out var anio))
            throw new ArgumentException("Clave de año inválida", nameof(clave));

        var inicio = new DateTime(anio, 1, 1);
        var fin = inicio.AddYears(1).AddDays(-1);
        return (inicio, fin);
    }

    private static (DateTime inicio, DateTime fin) ParsearClaveLustro(string clave)
    {
        var partes = clave.Split('-', StringSplitOptions.TrimEntries);
        if (partes.Length != 2
            || !int.TryParse(partes[0], NumberStyles.None, CultureInfo.InvariantCulture, out var anioInicio)
            || !int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out var anioFin)
            || anioFin != anioInicio + AniosPorLustro - 1)
            throw new ArgumentException("Clave de lustro inválida", nameof(clave));

        var inicio = new DateTime(anioInicio, 1, 1);
        var fin = inicio.AddYears(AniosPorLustro).AddDays(-1);
        return (inicio, fin);
    }
}
