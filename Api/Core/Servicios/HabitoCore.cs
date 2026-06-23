using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using AutoMapper;

namespace Api.Core.Servicios;

public class HabitoCore : ABMCore<IHabitoRepo, Habito, HabitoDTO>, IHabitoCore
{
    private const int MaxHabitosActivos = 5;
    private readonly IRegistroHabitoRepo _registroRepo;

    public HabitoCore(
        IBDVirtual bd,
        IHabitoRepo repo,
        IRegistroHabitoRepo registroRepo,
        IMapper mapper) : base(bd, repo, mapper)
    {
        _registroRepo = registroRepo;
    }

    protected override HabitoDTO AntesDeObtenerPorId(Habito entidad, HabitoDTO dto)
    {
        dto.CantidadRegistros = entidad.Registros?.Count ?? 0;
        return dto;
    }

    public override async Task<IEnumerable<HabitoDTO>> Listar()
    {
        var entidades = await Repo.Listar();
        var dtos = new List<HabitoDTO>();
        foreach (var entidad in entidades)
        {
            var dto = Mapper.Map<HabitoDTO>(entidad);
            dto.CantidadRegistros = await Repo.ContarRegistros(entidad.Id);
            dtos.Add(dto);
        }
        return dtos;
    }

    protected override async Task<Habito> AntesDeCrear(HabitoDTO dto, Habito entidad)
    {
        ValidarMetaMinutos(dto);
        if (dto.Activo)
            await ValidarLimiteActivos(null);

        if (dto.MetaMinutos == null && dto.Tipo == TipoHabitoEnum.Numerico)
            entidad.MetaMinutos = 1;

        return entidad;
    }

    protected override async Task<Habito> AntesDeModificar(
        int id,
        HabitoDTO dto,
        Habito entidadAnterior,
        Habito entidadNueva)
    {
        ValidarMetaMinutos(dto);

        if (dto.Activo && !entidadAnterior.Activo)
            await ValidarLimiteActivos(null);

        if (dto.MetaMinutos == null && dto.Tipo == TipoHabitoEnum.Numerico)
            entidadNueva.MetaMinutos = entidadAnterior.MetaMinutos ?? 1;

        return entidadNueva;
    }

    public override async Task Eliminar(int id)
    {
        var habito = await Repo.ObtenerPorId(id);
        if (habito == null)
            throw new ExcepcionControlada("No existe el hábito a eliminar");

        if (await _registroRepo.ExisteAlgunoParaHabito(id))
            throw new ExcepcionControlada(
                "No se puede eliminar un hábito con registros. Desactivá el hábito en su lugar.");

        Repo.Eliminar(habito);
        await BDVirtual.GuardarCambios();
    }

    public async Task<TrackerDiaDTO> ObtenerTracker(DateTime fecha)
    {
        var fechaNormalizada = fecha.Date;
        var habitos = (await Repo.ListarActivos()).ToList();
        var ids = habitos.Select(h => h.Id).ToList();
        var registros = (await _registroRepo.ListarPorHabitosYFecha(ids, fechaNormalizada)).ToList();

        return new TrackerDiaDTO
        {
            Fecha = fechaNormalizada,
            Habitos = habitos.Select(h =>
            {
                var registro = registros.SingleOrDefault(r => r.HabitoId == h.Id);
                return new HabitoTrackerItemDTO
                {
                    Id = h.Id,
                    Nombre = h.Nombre,
                    Tipo = h.Tipo,
                    MetaMinutos = h.MetaMinutos,
                    RegistroId = registro?.Id,
                    ValorBooleano = registro?.ValorBooleano,
                    ValorNumerico = registro?.ValorNumerico,
                };
            }).ToList(),
        };
    }

    public async Task UpsertRegistro(UpsertRegistroHabitoDTO dto)
    {
        var habito = await Repo.ObtenerPorId(dto.HabitoId);
        if (habito == null)
            throw new ExcepcionControlada("No existe el hábito");

        if (!habito.Activo)
            throw new ExcepcionControlada("El hábito no está activo");

        ValidarValoresRegistro(habito, dto);

        var fecha = dto.Fecha.Date;
        var existente = await _registroRepo.ObtenerPorHabitoYFecha(dto.HabitoId, fecha);

        if (existente == null)
        {
            var nuevo = new RegistroHabito
            {
                Id = 0,
                HabitoId = dto.HabitoId,
                Fecha = fecha,
                ValorBooleano = dto.ValorBooleano,
                ValorNumerico = dto.ValorNumerico,
            };
            _registroRepo.Crear(nuevo);
        }
        else
        {
            existente.ValorBooleano = dto.ValorBooleano;
            existente.ValorNumerico = dto.ValorNumerico;
        }

        await BDVirtual.GuardarCambios();
    }

    public async Task<ResumenSemanalDTO> ObtenerResumenSemanal(DateTime fecha)
    {
        var (inicio, fin) = ObtenerSemana(fecha.Date);
        var habitos = (await Repo.ListarActivos()).ToList();
        var ids = habitos.Select(h => h.Id).ToList();
        var registros = (await _registroRepo.ListarPorHabitosYRango(ids, inicio, fin)).ToList();

        var resumen = new ResumenSemanalDTO
        {
            FechaInicio = inicio,
            FechaFin = fin,
            Habitos = habitos.Select(h => ConstruirResumenHabito(h, inicio, registros)).ToList(),
        };

        return resumen;
    }

    private static (DateTime inicio, DateTime fin) ObtenerSemana(DateTime fecha)
    {
        var diasDesdeLunes = ((int)fecha.DayOfWeek + 6) % 7;
        var inicio = fecha.AddDays(-diasDesdeLunes);
        var fin = inicio.AddDays(6);
        return (inicio, fin);
    }

    private static HabitoResumenDTO ConstruirResumenHabito(
        Habito habito,
        DateTime inicioSemana,
        List<RegistroHabito> registros)
    {
        var detalle = new List<DiaResumenDTO>();
        var diasCumplidos = 0;
        var diasNoCumplidos = 0;
        var diasSinMarcar = 0;
        var totalMinutos = 0;
        var diasConValorNumerico = 0;

        for (var i = 0; i < 7; i++)
        {
            var dia = inicioSemana.AddDays(i);
            var registro = registros.SingleOrDefault(r => r.HabitoId == habito.Id && r.Fecha == dia);
            var estado = EvaluarEstado(habito, registro);

            detalle.Add(new DiaResumenDTO
            {
                Fecha = dia,
                Estado = estado,
                ValorBooleano = registro?.ValorBooleano,
                ValorNumerico = registro?.ValorNumerico,
            });

            switch (estado)
            {
                case "cumplido":
                    diasCumplidos++;
                    break;
                case "no_cumplido":
                    diasNoCumplidos++;
                    break;
                default:
                    diasSinMarcar++;
                    break;
            }

            if (habito.Tipo == TipoHabitoEnum.Numerico && registro?.ValorNumerico != null)
            {
                totalMinutos += registro.ValorNumerico.Value;
                diasConValorNumerico++;
            }
        }

        return new HabitoResumenDTO
        {
            Id = habito.Id,
            Nombre = habito.Nombre,
            Tipo = habito.Tipo,
            MetaMinutos = habito.MetaMinutos,
            DiasCumplidos = diasCumplidos,
            DiasNoCumplidos = diasNoCumplidos,
            DiasSinMarcar = diasSinMarcar,
            TotalMinutos = habito.Tipo == TipoHabitoEnum.Numerico ? totalMinutos : null,
            PromedioMinutos = habito.Tipo == TipoHabitoEnum.Numerico && diasConValorNumerico > 0
                ? Math.Round((double)totalMinutos / diasConValorNumerico, 1)
                : null,
            DetallePorDia = detalle,
        };
    }

    private static string EvaluarEstado(Habito habito, RegistroHabito? registro)
    {
        if (registro == null)
            return "sin_marcar";

        if (habito.Tipo == TipoHabitoEnum.SiNo)
        {
            if (registro.ValorBooleano == true)
                return "cumplido";
            if (registro.ValorBooleano == false)
                return "no_cumplido";
            return "sin_marcar";
        }

        if (registro.ValorNumerico == null)
            return "sin_marcar";

        var meta = habito.MetaMinutos ?? 1;
        return registro.ValorNumerico >= meta ? "cumplido" : "no_cumplido";
    }

    private async Task ValidarLimiteActivos(int? idExcluido)
    {
        var activos = idExcluido.HasValue
            ? await Repo.ContarActivosExcluyendo(idExcluido.Value)
            : await Repo.ContarActivos();

        if (activos >= MaxHabitosActivos)
            throw new ExcepcionControlada($"Solo podés tener {MaxHabitosActivos} hábitos activos a la vez");
    }

    private static void ValidarMetaMinutos(HabitoDTO dto)
    {
        if (dto.Tipo == TipoHabitoEnum.Numerico && dto.MetaMinutos is <= 0)
            throw new ExcepcionControlada("La meta en minutos debe ser mayor a 0");
    }

    private static void ValidarValoresRegistro(Habito habito, UpsertRegistroHabitoDTO dto)
    {
        if (habito.Tipo == TipoHabitoEnum.SiNo)
        {
            if (dto.ValorBooleano == null)
                throw new ExcepcionControlada("Debés indicar si cumpliste el hábito");
            return;
        }

        if (dto.ValorNumerico == null)
            throw new ExcepcionControlada("Debés indicar los minutos");

        if (dto.ValorNumerico < 0)
            throw new ExcepcionControlada("Los minutos no pueden ser negativos");
    }
}
