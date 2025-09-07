using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class EscritoCore : ABMCore<IEscritoRepo, Escrito, EscritoDTO>, IEscritoCore
{
    public EscritoCore(IBDVirtual bd, IEscritoRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
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
}