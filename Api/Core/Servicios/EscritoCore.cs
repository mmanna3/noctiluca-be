using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class EscritoCore : ABMCore<IEscritoRepo, Escrito, EscritoDTO>, IEscritoCore
{
    private readonly ICarpetaRepo _carpetaRepo;

    public EscritoCore(IBDVirtual bd, IEscritoRepo repo, IMapper mapper, ICarpetaRepo carpetaRepo) : base(bd, repo, mapper)
    {
        _carpetaRepo = carpetaRepo;
    }

    protected override async Task<Escrito> AntesDeCrear(EscritoDTO dto, Escrito entidad)
    {
        // Si no hay título o está vacío, asignar fecha y hora actual
        if (string.IsNullOrWhiteSpace(dto.Titulo))
        {
            var fechaHora = DateTime.Now;
            entidad.Titulo = fechaHora.ToString("dd-MM-yy HH:mm");
        }
        else
        {
            // Si hay título, usarlo
            entidad.Titulo = dto.Titulo;
        }

        // Establecer fecha de creación
        entidad.FechaHoraCreacion = DateTime.Now;

        return await base.AntesDeCrear(dto, entidad);
    }

    public async Task MoverACarpeta(MoverEscritosDTO dto)
    {
        var carpetaDestino = await _carpetaRepo.ObtenerPorId(dto.CarpetaDestinoId);
        if (carpetaDestino == null)
            throw new ExcepcionControlada("La carpeta destino no existe");

        foreach (var escritoId in dto.EscritoIds)
        {
            var escrito = await Repo.ObtenerPorIdConTracking(escritoId);
            if (escrito == null)
                throw new ExcepcionControlada($"El escrito con id {escritoId} no existe");

            escrito.CarpetaId = dto.CarpetaDestinoId;
        }

        await BDVirtual.GuardarCambios();
    }
}
