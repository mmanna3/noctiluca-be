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
}