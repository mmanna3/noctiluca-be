using Api.Core.DTOs;
using Api.Core.Entidades;
using AutoMapper;

namespace Api._Config;

public class MapperConfig : Profile
{
    public MapperConfig()
    {   
        CreateMap<Carpeta, CarpetaDTO>()
            .ForMember(dest => dest.CriterioDeOrden, x => x.MapFrom(src => src.CriterioDeOrdenId))
            .PreserveReferences();
            
        CreateMap<CarpetaDTO, Carpeta>()
            .ForMember(dest => dest.CriterioDeOrdenId, x => x.MapFrom(src => (int)src.CriterioDeOrden))
            .ForMember(dest => dest.CriterioDeOrden, x => x.Ignore())
            .ForMember(dest => dest.CarpetaPadre, x => x.Ignore())
            .ForMember(dest => dest.SubCarpetas, x => x.Ignore())
            .PreserveReferences();

        CreateMap<Escrito, EscritoDTO>()
            .ForMember(dest => dest.CarpetaTitulo, x => x.MapFrom(src => src.Carpeta!.Titulo))
            .PreserveReferences();
        
        CreateMap<EscritoDTO, Escrito>()
            .PreserveReferences();

        CreateMap<Habito, HabitoDTO>()
            .ForMember(dest => dest.CantidadRegistros, x => x.Ignore())
            .PreserveReferences();

        CreateMap<HabitoDTO, Habito>()
            .ForMember(dest => dest.Registros, x => x.Ignore())
            .PreserveReferences();

        CreateMap<RegistroHabito, RegistroHabitoDTO>()
            .PreserveReferences();

        CreateMap<RegistroHabitoDTO, RegistroHabito>()
            .ForMember(dest => dest.Habito, x => x.Ignore())
            .PreserveReferences();

        CreateMap<ListaObjetivo, ListaObjetivoDTO>()
            .ForMember(dest => dest.Items, x => x.Ignore())
            .ForMember(dest => dest.AdvertenciaLimite, x => x.Ignore())
            .PreserveReferences();

        CreateMap<ItemObjetivo, ItemObjetivoDTO>()
            .PreserveReferences();
    }
}