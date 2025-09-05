using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class CarpetaCore : ABMCore<ICarpetaRepo, Carpeta, CarpetaDTO>, ICarpetaCore
{
    public CarpetaCore(IBDVirtual bd, ICarpetaRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
    
    public override async Task Eliminar(int id)
    {
        var carpeta = await Repo.ObtenerPorId(id);
        if (carpeta == null)
            throw new ExcepcionControlada("No existe la carpeta a eliminar");
        
        if (carpeta.Escritos.Count != 0)
            throw new ExcepcionControlada("Para eliminar la carpeta, eliminá los escritos primero");
        
        Repo.Eliminar(carpeta);
        await BDVirtual.GuardarCambios();
    }
    
    public async Task ActualizarCriterioDeOrden(int carpetaId, int criterioDeOrdenId)
    {        
        var carpeta = await Repo.ObtenerPorIdConTracking(carpetaId);
        if (carpeta == null)
            throw new ExcepcionControlada("No existe la carpeta");
        
        carpeta.CriterioDeOrdenId = criterioDeOrdenId;
        await BDVirtual.GuardarCambios();
    }
    
    public async Task ActualizarPosiciones(ActualizarPosicionesDTO dto)
    {
        foreach (var posicionCarpeta in dto.Posiciones)
        {
            var carpeta = await Repo.ObtenerPorIdConTracking(posicionCarpeta.IdDeCarpeta);
            if (carpeta == null)
                throw new ExcepcionControlada($"No existe la carpeta con ID {posicionCarpeta.IdDeCarpeta}");
            
            carpeta.Posicion = posicionCarpeta.Posicion;
        }
        
        await BDVirtual.GuardarCambios();
    }
}