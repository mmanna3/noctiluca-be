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
            .PreserveReferences();

        CreateMap<Escrito, EscritoDTO>()
            .ForMember(dest => dest.CarpetaTitulo, x => x.MapFrom(src => src.Carpeta!.Titulo))
            .PreserveReferences();
        
        CreateMap<EscritoDTO, Escrito>()
            .ForMember(dest => dest.Titulo, x => x.Ignore()) // Lo manejamos manualmente en AntesDeCrear
            .PreserveReferences();
    }
}