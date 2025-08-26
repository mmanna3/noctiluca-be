using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;

namespace Api.Core.Servicios;

public class PapeleraCore : IPapeleraCore
{
    private readonly IEscritoRepo _escritoRepo;
    private readonly IBDVirtual _bdVirtual;
    public PapeleraCore(IBDVirtual bd, IEscritoRepo escritoRepo)
    {
        _escritoRepo = escritoRepo;
        _bdVirtual = bd;
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

    public async Task<bool> PonerEnPapelera(int escritoId)
    {
        var escrito = await _escritoRepo.ObtenerPorId(escritoId);
        if (escrito == null)
            return false;

        var escritoModificado = new Escrito
        {
            Id = escrito.Id,
            Titulo = escrito.Titulo,
            Cuerpo = escrito.Cuerpo,
            FechaHoraCreacion = escrito.FechaHoraCreacion,
            FechaHoraEdicion = DateTime.Now,
            CarpetaId = escrito.CarpetaId,
            EstaEnPapelera = true
        };

        _escritoRepo.Modificar(escrito, escritoModificado);
        await _bdVirtual.GuardarCambios();
        return true;
    }
}
