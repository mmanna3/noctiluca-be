using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface IEscritoCore : ICoreABM<EscritoDTO>
{
    Task MoverACarpeta(MoverEscritosDTO dto);
}