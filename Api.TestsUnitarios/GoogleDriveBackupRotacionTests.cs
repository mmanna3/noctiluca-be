using Api.Core.Servicios;
using Moq;
using Xunit;

namespace Api.TestsUnitarios;

public class GoogleDriveBackupRotacionTests
{
    [Fact]
    public void CuatroDiasDistintos_BorraSoloElDiaMasAntiguo()
    {
        var nombres = new[]
        {
            "backup-bd-2026-03-28-02-18.zip",
            "backup-bd-2026-03-29-02-18.zip",
            "backup-bd-2026-03-30-02-30.zip",
            "backup-bd-2026-03-31-02-16.zip"
        };
        var archivos = nombres.Select((n, i) => ($"id-{i}", n)).ToList();

        var r = GoogleDriveBackupRotacion.Calcular(archivos, maxDias: 3);

        Assert.Equal(4, r.DiasDetectados);
        Assert.Single(r.IdsABorrar);
        Assert.Single(r.ArchivosBorrados);
        Assert.Contains("backup-bd-2026-03-28-02-18.zip", r.ArchivosBorrados);
        Assert.Equal("id-0", r.IdsABorrar[0]);
    }

    [Fact]
    public void TresDiasDistintos_NoBorraNada()
    {
        var nombres = new[]
        {
            "backup-bd-2026-03-29-02-18.zip",
            "backup-bd-2026-03-30-02-30.zip",
            "backup-bd-2026-03-31-02-16.zip"
        };
        var archivos = nombres.Select((n, i) => ($"id-{i}", n)).ToList();

        var r = GoogleDriveBackupRotacion.Calcular(archivos, maxDias: 3);

        Assert.Equal(3, r.DiasDetectados);
        Assert.Empty(r.IdsABorrar);
        Assert.Empty(r.ArchivosBorrados);
    }

    [Fact]
    public void MismoDia_MinutosDistintos_CuentaUnSoloDia()
    {
        var archivos = new List<(string, string)>
        {
            ("a", "backup-bd-2026-03-31-02-16.zip"),
            ("b", "backup-bd-2026-03-31-02-17.zip")
        };

        var r = GoogleDriveBackupRotacion.Calcular(archivos, maxDias: 3);

        Assert.Equal(1, r.DiasDetectados);
        Assert.Empty(r.IdsABorrar);
    }

    [Fact]
    public void ExtraerFechaDesdeNombreBackup_DevuelveSoloYyyyMmDd()
    {
        Assert.Equal("2026-03-28", GoogleDriveBackupRotacion.ExtraerFechaDesdeNombreBackup("backup-bd-2026-03-28-02-18.zip"));
        Assert.Null(GoogleDriveBackupRotacion.ExtraerFechaDesdeNombreBackup("backup-imagenes-2026-03-28-01-59.zip"));
        Assert.Null(GoogleDriveBackupRotacion.ExtraerFechaDesdeNombreBackup("otro.zip"));
    }

    [Fact]
    public void BackupsEnDrive_ListaTodosLosNombresOrdenados()
    {
        var archivos = new List<(string, string)>
        {
            ("z", "zebra.zip"),
            ("a", "backup-bd-2026-03-31-02-16.zip")
        };

        var r = GoogleDriveBackupRotacion.Calcular(archivos);

        Assert.Equal(new[] { "backup-bd-2026-03-31-02-16.zip", "zebra.zip" }, r.BackupsEnDrive);
    }

    [Fact]
    public void SimulacionDelete_IgualCantidadQueIdsABorrar()
    {
        var nombres = new[]
        {
            "backup-bd-2026-03-28-02-18.zip",
            "backup-bd-2026-03-29-02-18.zip",
            "backup-bd-2026-03-30-02-30.zip",
            "backup-bd-2026-03-31-02-16.zip"
        };
        var archivos = nombres.Select((n, i) => ($"id-{i}", n)).ToList();
        var plan = GoogleDriveBackupRotacion.Calcular(archivos, maxDias: 3);

        var deleter = new Mock<IDeleteArchivoDriveParaTest>();
        foreach (var id in plan.IdsABorrar)
            deleter.Object.Eliminar(id);

        deleter.Verify(d => d.Eliminar(It.IsAny<string>()), Times.Exactly(plan.IdsABorrar.Count));
        deleter.Verify(d => d.Eliminar("id-0"), Times.Once());
    }
}

public interface IDeleteArchivoDriveParaTest
{
    void Eliminar(string id);
}
