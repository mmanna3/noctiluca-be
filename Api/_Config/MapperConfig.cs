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
            .PreserveReferences().ReverseMap();

        CreateMap<Escrito, EscritoDTO>()
            .ForMember(dest => dest.CarpetaTitulo, x => x.MapFrom(src => src.Carpeta!.Titulo))
            .PreserveReferences();
        
        CreateMap<EscritoDTO, Escrito>()
            .PreserveReferences();
    }
}