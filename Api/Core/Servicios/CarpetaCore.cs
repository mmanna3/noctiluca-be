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
    
    protected override async Task<Carpeta> AntesDeCrear(CarpetaDTO dto, Carpeta entidad)
    {
        if (dto.CarpetaPadreId.HasValue)
        {
            var carpetaPadre = await Repo.ObtenerPorId(dto.CarpetaPadreId.Value);
            if (carpetaPadre == null)
                throw new ExcepcionControlada("No existe la carpeta padre");

            if (carpetaPadre.CarpetaPadreId.HasValue)
                throw new ExcepcionControlada("No se puede crear una carpeta dentro de una subcarpeta (máximo 2 niveles de profundidad)");
        }

        return entidad;
    }

    public override async Task Eliminar(int id)
    {
        var carpeta = await Repo.ObtenerPorId(id);
        if (carpeta == null)
            throw new ExcepcionControlada("No existe la carpeta a eliminar");

        if (carpeta.Escritos.Count != 0)
            throw new ExcepcionControlada("Para eliminar la carpeta, eliminá los escritos primero");

        if (carpeta.SubCarpetas.Count != 0)
            throw new ExcepcionControlada("Para eliminar la carpeta, eliminá las subcarpetas primero");

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