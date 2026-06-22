using System.Text;
using Api.Api.Authorization;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BackupController : ControllerBase
{
    private readonly IBackupCore _backupCore;
    private readonly IGoogleDriveCore _googleDriveCore;

    public BackupController(IBackupCore backupCore, IGoogleDriveCore googleDriveCore)
    {
        _backupCore = backupCore;
        _googleDriveCore = googleDriveCore;
    }

    [HttpGet("guardar-backup-bd-en-disco")]
    [AutorizarConApiKey]
    public async Task<IActionResult> GuardarBackupBdEnDisco()
    {
        var ruta = await _backupCore.GuardarBackupBaseDeDatosEnDisco();
        return Ok(new { ruta });
    }

    [HttpGet("validar-archivos-locales")]
    [AutorizarConApiKey]
    public IActionResult ValidarArchivosLocales()
    {
        _backupCore.ValidarCantidadArchivosEnCarpetaBackup();
        return Ok();
    }

    [HttpGet("rotar-backups-en-drive")]
    [AutorizarConApiKey]
    public async Task<IActionResult> RotarBackupsEnDrive()
    {
        var r = await _googleDriveCore.RotarBackupsEnDrive();

        var texto = new StringBuilder();
        texto.AppendLine("Backups que había en el Drive:");
        foreach (var nombre in r.BackupsEnDrive)
            texto.AppendLine(nombre);

        if (r.DiasDetectados > 3 && r.ArchivosBorrados.Count > 0)
        {
            texto.AppendLine();
            texto.AppendLine("Como son más de 3 días, se borraron:");
            foreach (var nombre in r.ArchivosBorrados)
                texto.AppendLine(nombre);
        }

        return Ok(new
        {
            textoResumen = texto.ToString().TrimEnd(),
            backupsQueHabiaEnElDrive = r.BackupsEnDrive,
            diasDetectados = r.DiasDetectados,
            archivosBorrados = r.ArchivosBorrados
        });
    }

    [HttpGet("subir-backup-bd-a-drive")]
    [AutorizarConApiKey]
    public async Task<IActionResult> SubirBackupBdADrive()
    {
        var ruta = _backupCore.ObtenerRutaBackupBdEnDisco();
        var id = await _googleDriveCore.SubirArchivo(ruta, Path.GetFileName(ruta));
        return Ok(new { id });
    }

    [HttpDelete("limpiar-backups-locales")]
    [AutorizarConApiKey]
    public IActionResult LimpiarBackupsLocales()
    {
        _backupCore.LimpiarBackupsLocales();
        return Ok();
    }

    // Comentado porque SOLO FUE NECESARIO UNA SOLA VEZ.
    //
    // [HttpGet("google-drive-autorizar")]
    // public IActionResult GoogleDriveAutorizar()
    // {
    //     var redirectUri = $"{Request.Scheme}://{Request.Host}/api/backup/google-drive-callback";
    //     var url = _googleDriveCore.ObtenerUrlDeAutorizacion(redirectUri);
    //     return Redirect(url);
    // }
    //
    // [HttpGet("google-drive-callback")]
    // public async Task<IActionResult> GoogleDriveCallback([FromQuery] string code)
    // {
    //     var redirectUri = $"{Request.Scheme}://{Request.Host}/api/backup/google-drive-callback";
    //     await _googleDriveCore.GuardarRefreshToken(code, redirectUri);
    //     return Ok("Refresh token guardado correctamente. Ya podés usar el backup.");
    // }
}
