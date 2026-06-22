namespace Api.Core.Logica;

public class FechaUtils
{
    private const string FormatoFechaBackupDisco = "yyyy-MM-dd-HH-mm";

    /// <summary>Formato para nombres de backup en disco: yyyy-MM-dd-HH-mm (GMT-3).</summary>
    public static string AhoraEnArgentinaFormatoBackupDisco =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfoArg()).ToString(FormatoFechaBackupDisco);

    private static TimeZoneInfo TimeZoneInfoArg()
    {
        var p = (int)Environment.OSVersion.Platform;

        if (p is 4 or 6 or 128)
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");

        return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
    }
}
