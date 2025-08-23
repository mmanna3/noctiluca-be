using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class NotaCore : ABMCore<INotaRepo, Nota, NotaDTO>, INotaCore
{
    public NotaCore(IBDVirtual bd, INotaRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}