# Modo offline-first (backend)

El backend expone una **API de sincronización** dedicada además del CRUD REST histórico. Los clientes (PWA) mantienen una copia local y reconcilian cambios mediante pull/push por lotes.

## Entidades sincronizables

Todas heredan de `EntidadSincronizable` (`Api/Core/Entidades/EntidadSincronizable.cs`):

- No genera tabla propia en EF Core; agrega columnas a cada tabla hija.
- Marcador explícito de participación en sync.

| Entidad | Notas |
|---------|--------|
| `Carpeta` | Upsert, delete → tombstone |
| `Escrito` | Upsert (incl. papelera), delete → tombstone |
| `Habito` | Upsert, delete |
| `RegistroHabito` | Clave natural `(HabitoId, Fecha)` |
| `ListaObjetivo` | Pull; creación implícita vía items |
| `ItemObjetivo` | Upsert por `(ListaTipo, ListaClavePeriodo)` |

Columnas añadidas por la base:

- **`ClientId`** (`Guid`): identidad estable cliente/servidor.
- **`Version`** (`long`): contador monotónico server-side (cursor del change-feed).
- **`ActualizadoEn`** (`DateTime` UTC): desempate LWW.

## Endpoints (`SyncController`)

### `GET /api/Sync/cambios?desde={cursor}`

Change-feed incremental. Devuelve `SyncPullDTO`:

- Entidades con `Version > desde`
- `Eliminados`: tombstones (`ClientId`, `TipoEntidad`, `Version`)
- `Cursor`: máximo `Version` visto (el cliente lo persiste en IndexedDB)

Roles: `Administrador`, `Consulta`.

### `POST /api/Sync/aplicar`

Recibe `SyncPushDTO`:

```json
{
  "deviceId": "uuid-del-dispositivo",
  "operaciones": [
    {
      "clientOpId": "uuid-idempotente",
      "entityType": "escrito",
      "operation": "upsert",
      "clientEntityId": "guid-entidad",
      "baseVersion": 3,
      "clientTimestamp": "2026-07-01T12:00:00Z",
      "payload": { ... }
    }
  ]
}
```

Respuesta: `SyncPushResultDTO` con un resultado por operación (`aplicado`, `rechazado`, `duplicado`) y opcionalmente `serverId` / `version`.

Roles: solo `Administrador` (mutaciones).

## Lógica principal (`SyncCore`)

### Pull

1. Consulta entidades sincronizables con `Version > cursor`.
2. Consulta `Tombstones` en el mismo rango.
3. Calcula el nuevo cursor (máximo `Version` retornado).

### Push

1. Ordena operaciones (dependencias: carpetas antes que escritos).
2. **Idempotencia**: si `ClientOpId` ya está en `SyncOpLog`, responde `duplicado`.
3. **Upsert**: busca por `ClientId`; crea o actualiza.
4. **Delete**: borra entidad y registra tombstone.
5. **LWW**: compara `baseVersion` + `clientTimestamp` con el estado server; si pierde el cliente → `rechazado`.

### Resolución de conflictos (last-write-wins)

- Si `baseVersion` coincide con la del servidor → se aplica (sin conflicto concurrente).
- Si hay cambio concurrente y el timestamp del cliente es anterior → `rechazado`.
- El cliente debe hacer pull y alinear local.

## Persistencia auxiliar

| Tabla / concepto | Uso |
|------------------|-----|
| `Tombstone` | Borrados replicables al pull |
| `SyncOpLog` | Idempotencia por `ClientOpId` |
| `ContadorSync` | Versión monotónica (compatible con tests InMemory; no `rowversion`) |

En `AppDbContext.SaveChangesAsync`:

- Asigna `Version` y `ActualizadoEn` a entidades sincronizables modificadas.
- Genera tombstones en deletes de entidades sync.
- Protege `ClientId` de sobrescritura accidental en updates.

## Payloads de push (resumen)

| entityType | operation | Payload clave |
|------------|-----------|----------------|
| `carpeta` | upsert / delete | título, criterio, posición… |
| `escrito` | upsert / delete | título, cuerpo, `carpetaClientId`, papelera |
| `habito` | upsert / delete | nombre, tipo, activo, posición |
| `registroHabito` | upsert | `habitoClientId`, `fecha`, valores |
| `itemObjetivo` | upsert / delete | texto, completado, posición, `listaTipo`, `listaClavePeriodo` |

Los nombres de `entityType` en JSON son **camelCase** (`registroHabito`, `itemObjetivo`).

## REST vs Sync

El CRUD REST (`/api/Carpeta`, `/api/Escrito`, etc.) sigue existiendo para:

- Operaciones **solo online** (mover escritos, reordenar carpetas, administrar hábitos vía UI legacy).
- Compatibilidad y herramientas.

La PWA offline-first usa **sync** para el grueso de mutaciones; algunas acciones online invalidan cache y llaman `pedirSync()` en el cliente para alinear Dexie.

## Tests de integración

`Api.TestsDeIntegracion/SyncTests.cs`:

- Estampado de `ClientId` y `Version` al crear.
- Pull con cursor incremental.
- Push create offline con dependencia carpeta → escrito por `ClientId`.
- Idempotencia por `ClientOpId`.
- LWW rechaza cambio viejo y acepta nuevo.
- Delete genera tombstone visible en pull.

Ejecutar: `dotnet test` desde `noctiluca-be/Api`.

## Migraciones

Las columnas `ClientId`, `Version`, `ActualizadoEn` y tablas auxiliares se agregaron vía EF migrations. Aplicar en deploy con `dotnet ef database update`.

## Documentación relacionada

- Frontend offline: [noctiluca-fe/docs/modo-offline.md](../noctiluca-fe/docs/modo-offline.md)
- Contexto general: [docs/CONTEXTO-NOCTILUCA.md](../docs/CONTEXTO-NOCTILUCA.md)
