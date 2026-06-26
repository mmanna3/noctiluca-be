using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using AutoMapper;

namespace Api.Core.Servicios;

public class ObjetivoCore : IObjetivoCore
{
    private readonly IBDVirtual _bd;
    private readonly IListaObjetivoRepo _listaRepo;
    private readonly IItemObjetivoRepo _itemRepo;
    private readonly IMapper _mapper;

    public ObjetivoCore(
        IBDVirtual bd,
        IListaObjetivoRepo listaRepo,
        IItemObjetivoRepo itemRepo,
        IMapper mapper)
    {
        _bd = bd;
        _listaRepo = listaRepo;
        _itemRepo = itemRepo;
        _mapper = mapper;
    }

    public async Task<ListaObjetivoDTO> ObtenerOCrearListaDia(DateTime fecha)
    {
        var lista = await ObtenerOCrearLista(TipoListaObjetivoEnum.Dia, fecha.Date);
        return MapearLista(lista);
    }

    public async Task<ListaObjetivoDTO?> ObtenerLista(TipoListaObjetivoEnum tipo, string clavePeriodo)
    {
        var lista = await _listaRepo.ObtenerPorTipoYClave(tipo, clavePeriodo);
        return lista == null ? null : MapearLista(lista);
    }

    public async Task<ListaObjetivoDTO> ObtenerListaPorId(int id)
    {
        var lista = await _listaRepo.ObtenerPorIdConItems(id);
        if (lista == null)
            throw new ExcepcionControlada("No existe la lista de objetivos");

        return MapearLista(lista);
    }

    public async Task<HistoricoObjetivoPaginadoDTO> ObtenerHistorico(
        TipoListaObjetivoEnum tipo,
        int pagina,
        int tamano)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamano < 1 || tamano > 100)
            tamano = 20;

        var (listas, total) = await _listaRepo.ListarHistorico(tipo, pagina, tamano);

        return new HistoricoObjetivoPaginadoDTO
        {
            Pagina = pagina,
            Tamano = tamano,
            Total = total,
            Items = listas.Select(lista => new HistoricoObjetivoDTO
            {
                Id = lista.Id,
                Tipo = lista.Tipo,
                ClavePeriodo = lista.ClavePeriodo,
                FechaInicio = lista.FechaInicio,
                FechaFin = lista.FechaFin,
                CantidadItems = lista.Items.Count,
                CantidadCompletados = lista.Items.Count(i => i.Completado),
            }).ToList(),
        };
    }

    public async Task<ItemObjetivoDTO> CrearItem(CrearItemObjetivoDTO dto)
    {
        var texto = dto.Texto?.Trim() ?? "";
        if (!string.IsNullOrEmpty(texto))
            ValidarTexto(texto);

        var lista = await ResolverListaParaItem(dto);
        var posicion = lista.Items.Any() ? lista.Items.Max(i => i.Posicion) + 1 : 0;

        if (dto.Posicion.HasValue)
        {
            posicion = dto.Posicion.Value;
            foreach (var existente in lista.Items.Where(i => i.Posicion >= posicion))
                existente.Posicion++;
        }

        var item = new ItemObjetivo
        {
            Id = 0,
            ListaObjetivoId = lista.Id,
            Texto = texto,
            Completado = false,
            Posicion = posicion,
        };

        _itemRepo.Crear(item);
        await _bd.GuardarCambios();

        return _mapper.Map<ItemObjetivoDTO>(item);
    }

    public async Task<ItemObjetivoDTO> EditarItem(int id, EditarItemObjetivoDTO dto)
    {
        ValidarTexto(dto.Texto);

        var item = await _itemRepo.ObtenerPorIdConTracking(id);
        if (item == null)
            throw new ExcepcionControlada("No existe el ítem");

        item.Texto = dto.Texto.Trim();
        await _bd.GuardarCambios();

        return _mapper.Map<ItemObjetivoDTO>(item);
    }

    public async Task<ItemObjetivoDTO> ToggleCompletado(int id)
    {
        var item = await _itemRepo.ObtenerPorIdConTracking(id);
        if (item == null)
            throw new ExcepcionControlada("No existe el ítem");

        item.Completado = !item.Completado;
        item.FechaCompletado = item.Completado ? DateTime.Now : null;
        await _bd.GuardarCambios();

        return _mapper.Map<ItemObjetivoDTO>(item);
    }

    public async Task EliminarItem(int id)
    {
        var item = await _itemRepo.ObtenerPorIdConTracking(id);
        if (item == null)
            throw new ExcepcionControlada("No existe el ítem");

        _itemRepo.Eliminar(item);
        await _bd.GuardarCambios();
    }

    public async Task ActualizarPosicionesItem(ActualizarPosicionesItemObjetivoDTO dto)
    {
        foreach (var posicionItem in dto.Posiciones)
        {
            var item = await _itemRepo.ObtenerPorIdConTracking(posicionItem.IdDeItem);
            if (item == null)
                throw new ExcepcionControlada($"No existe el ítem con ID {posicionItem.IdDeItem}");

            item.Posicion = posicionItem.Posicion;
        }

        await _bd.GuardarCambios();
    }

    private async Task<ListaObjetivo> ResolverListaParaItem(CrearItemObjetivoDTO dto)
    {
        if (dto.ListaObjetivoId.HasValue)
        {
            var lista = await _listaRepo.ObtenerPorIdConItemsConTracking(dto.ListaObjetivoId.Value);
            if (lista == null)
                throw new ExcepcionControlada("No existe la lista de objetivos");

            return lista;
        }

        if (dto.Tipo == null || string.IsNullOrWhiteSpace(dto.ClavePeriodo))
            throw new ExcepcionControlada("Debés indicar la lista o el período");

        var existente = await _listaRepo.ObtenerPorTipoYClave(dto.Tipo.Value, dto.ClavePeriodo);
        if (existente != null)
        {
            var conTracking = await _listaRepo.ObtenerPorIdConItemsConTracking(existente.Id);
            return conTracking!;
        }

        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoDesdeClave(dto.Tipo.Value, dto.ClavePeriodo);
        var nueva = new ListaObjetivo
        {
            Id = 0,
            Tipo = dto.Tipo.Value,
            ClavePeriodo = dto.ClavePeriodo,
            FechaInicio = inicio,
            FechaFin = fin,
            FechaCreacion = DateTime.Now,
        };

        _listaRepo.Crear(nueva);
        await _bd.GuardarCambios();

        var creada = await _listaRepo.ObtenerPorIdConItemsConTracking(nueva.Id);
        return creada!;
    }

    private async Task<ListaObjetivo> ObtenerOCrearLista(TipoListaObjetivoEnum tipo, DateTime fecha)
    {
        var clave = ObjetivoPeriodoUtil.ObtenerClavePeriodo(tipo, fecha);
        var existente = await _listaRepo.ObtenerPorTipoYClave(tipo, clave);
        if (existente != null)
            return existente;

        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoPeriodo(tipo, fecha);
        var nueva = new ListaObjetivo
        {
            Id = 0,
            Tipo = tipo,
            ClavePeriodo = clave,
            FechaInicio = inicio,
            FechaFin = fin,
            FechaCreacion = DateTime.Now,
        };

        _listaRepo.Crear(nueva);
        await _bd.GuardarCambios();

        return (await _listaRepo.ObtenerPorTipoYClave(tipo, clave))!;
    }

    private ListaObjetivoDTO MapearLista(ListaObjetivo lista)
    {
        var dto = _mapper.Map<ListaObjetivoDTO>(lista);
        dto.Items = lista.Items
            .OrderBy(i => i.Posicion)
            .ThenBy(i => i.Id)
            .Select(i => _mapper.Map<ItemObjetivoDTO>(i))
            .ToList();

        if (lista.Tipo == TipoListaObjetivoEnum.Dia && dto.Items.Count > ObjetivoPeriodoUtil.LimiteRecomendadoDia)
        {
            dto.AdvertenciaLimite =
                $"Más de {ObjetivoPeriodoUtil.LimiteRecomendadoDia} objetivos — considerá priorizar";
        }

        return dto;
    }

    private static void ValidarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ExcepcionControlada("El texto del objetivo no puede estar vacío");
    }
}
