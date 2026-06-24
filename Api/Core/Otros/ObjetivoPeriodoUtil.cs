using System.Globalization;
using Api.Core.Enums;

namespace Api.Core.Otros;

public static class ObjetivoPeriodoUtil
{
    public const int LimiteRecomendadoDia = 7;

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

    public static string ObtenerClavePeriodo(TipoListaObjetivoEnum tipo, DateTime fecha) =>
        tipo switch
        {
            TipoListaObjetivoEnum.Dia => ClaveDia(fecha),
            TipoListaObjetivoEnum.Semana => ClaveSemana(fecha),
            TipoListaObjetivoEnum.Mes => ClaveMes(fecha),
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
}
