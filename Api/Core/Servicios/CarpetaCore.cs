using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class CarpetaCore : ABMCore<ICarpetaRepo, Carpeta, CarpetaDTO>, ICarpetaCore
{
    public CarpetaCore(IBDVirtual bd, ICarpetaRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}