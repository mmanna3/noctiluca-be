using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class PapeleraCore : IPapeleraCore
{
    private readonly IEscritoRepo _escritoRepo;

    public PapeleraCore(IEscritoRepo escritoRepo)
    {
        _escritoRepo = escritoRepo;
    }

    public async Task<IEnumerable<EscritoDTO>> ObtenerEscritosEnPapelera()
    {
        var escritos = await _escritoRepo.Listar();
        var escritosEnPapelera = escritos.Where(e => e.EstaEnPapelera).ToList();
        
        return escritosEnPapelera.Select(e => new EscritoDTO
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Cuerpo = e.Cuerpo,
            FechaHoraCreacion = e.FechaHoraCreacion,
            FechaHoraEdicion = e.FechaHoraEdicion,
            CarpetaId = e.CarpetaId,
            CarpetaTitulo = e.Carpeta?.Titulo
        });
    }
}
