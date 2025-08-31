using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface ICarpetaCore : ICoreABM<CarpetaDTO>
{
    Task ActualizarCriterioDeOrden(int carpetaId, int criterioDeOrdenId);
}