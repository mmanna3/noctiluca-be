# Configuración de GitHub Actions para Noctiluca-BE

Este documento explica cómo configurar los secrets en GitHub para el deploy y el backup automático.

## 1. Crear Environment

1. Ve al repositorio **noctiluca-be** en GitHub.
2. **Settings** → **Environments**.
3. Crea el environment `production`.

## 2. Secrets del deploy (noctiluca-be)

En **Settings** → **Environments** → `production` → **Environment secrets**:

| Secret | Descripción |
|--------|-------------|
| `DB_SERVER` | Servidor SQL Server |
| `DB_NAME` | Nombre de la base de datos |
| `DB_USERNAME` | Usuario SQL |
| `DB_PASSWORD` | Contraseña SQL |
| `CLAVE_SECRETA_JWT` | Clave JWT (mín. 64 caracteres) |
| `BACKUP_API_KEY` | Clave para endpoints de backup (`openssl rand -hex 32`) |
| `DEPLOY_PUBLISH_URL` | URL Web Deploy (ej. `https://dominio:8172`) |
| `MS_DEPLOY_SITE` | Nombre del sitio IIS |
| `DEPLOY_USERNAME` | Usuario Web Deploy |
| `DEPLOY_PASSWORD` | Contraseña Web Deploy |

Migrar los valores desde AppVeyor (Environment → encrypted variables en appveyor.com).

## 3. Secrets del backup (noctiluca-shell)

En el repo **noctiluca-shell**, mismo environment `production`:

| Tipo | Nombre | Descripción |
|------|--------|-------------|
| Variable | `API_URL` | URL base de producción sin trailing slash |
| Secret | `BACKUP_API_KEY` | **El mismo valor** que en noctiluca-be |

## 4. Setup one-time: Google Drive en el servidor

Las credenciales de Google **no van en GitHub**. Se crean una sola vez en el servidor Plesk.

### 4.1 Google Cloud Console

1. Crear app OAuth (o reutilizar la de liga con redirect URI distinto).
2. Habilitar Google Drive API.
3. Agregar redirect URI: `https://TU_DOMINIO/api/backup/google-drive-callback`

### 4.2 Carpeta en Drive

1. Crear carpeta `noctiluca-backups` (o similar).
2. Copiar el ID de la carpeta desde la URL de Drive.

### 4.3 Archivo en el servidor

Crear `App_Data/google-drive-credenciales.dat` en el servidor (sobrevive deploys porque WebDeploy omite `App_Data`):

```json
{
  "client_id": "...",
  "client_secret": "...",
  "refresh_token": "",
  "id_carpeta_destino": "ID_CARPETA_DRIVE"
}
```

### 4.4 Obtener refresh_token

1. Descomentar temporalmente en `BackupController.cs` los endpoints `google-drive-autorizar` y `google-drive-callback`.
2. Deploy.
3. Visitar `https://TU_DOMINIO/api/backup/google-drive-autorizar` y autorizar.
4. Verificar que el refresh token quedó guardado en el archivo.
5. Volver a comentar los endpoints y redeploy.

### 4.5 Probar backup

1. Configurar secrets/variables en GitHub (secciones 2 y 3).
2. En **noctiluca-shell** → Actions → **Backup diario a Google Drive** → **Run workflow**.
3. Revisar logs de cada paso `curl`.

## 5. AppVeyor

El deploy migró a GitHub Actions. Desactivar el proyecto en [appveyor.com](https://ci.appveyor.com) para evitar deploys duplicados.

## 6. Retención de backups

- **Drive**: máximo 3 días de backups (`backup-bd-*.zip`).
- **Disco local** (`App_Data/backup/`): zips temporales; se limpian al final de cada run.
