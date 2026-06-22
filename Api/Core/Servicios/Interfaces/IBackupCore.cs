namespace Api.Core.Servicios.Interfaces;

public interface IBackupCore
{
    Task<string> GuardarBackupBaseDeDatosEnDisco();
    void ValidarCantidadArchivosEnCarpetaBackup();
    string ObtenerRutaBackupBdEnDisco();
    void LimpiarBackupsLocales();
}
