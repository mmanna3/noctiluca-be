using Api.Core.DTOs;
using Api.Core.Entidades;
using AutoMapper;

namespace Api._Config;

public class MapperConfig : Profile
{
    public MapperConfig()
    {   
        CreateMap<Carpeta, CarpetaDTO>()
            .PreserveReferences().ReverseMap();

        CreateMap<Nota, NotaDTO>()
            .PreserveReferences().ReverseMap();
    }
}