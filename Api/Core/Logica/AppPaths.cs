namespace Api.Core.Logica;

public abstract class AppPaths
{
    protected readonly IWebHostEnvironment Env;

    public string CarpetaTemporalBackupBaseDeDatosAbsolute { get; }

    protected AppPaths(IWebHostEnvironment env)
    {
        Env = env;
        CarpetaTemporalBackupBaseDeDatosAbsolute = BackupAbsoluteOf("ZenSchemaBackup");
    }

    public abstract string BackupAbsoluteOf(string fileNameWithExtension);
    public abstract string BackupAbsolute();
}

public class AppPathsWebApp : AppPaths
{
    public AppPathsWebApp(IWebHostEnvironment env) : base(env)
    {
    }

    public override string BackupAbsoluteOf(string fileNameWithExtension) =>
        Path.Combine(Env.ContentRootPath, "App_Data", fileNameWithExtension);

    public override string BackupAbsolute() =>
        Path.Combine(Env.ContentRootPath, "App_Data");
}
