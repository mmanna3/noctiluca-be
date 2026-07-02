using System.ComponentModel.DataAnnotations;
using Api.Core.Enums;

namespace Api.Core.Entidades;

public class Carpeta : EntidadSincronizable
{
    [MaxLength(100)]
    public required string Titulo { get; set; }
    public virtual ICollection<Escrito> Escritos { get; set; } = null!;

    public bool RequiereAutenticacion { get; set; }

    public int Posicion { get; set; }

    public int CriterioDeOrdenId { get; set; }
    public virtual CriterioDeOrden CriterioDeOrden { get; set; } = null!;

    public int? CarpetaPadreId { get; set; }
    public virtual Carpeta? CarpetaPadre { get; set; }
    public virtual ICollection<Carpeta> SubCarpetas { get; set; } = new List<Carpeta>();

    public bool EsSistema { get; set; }

    public PropositoCarpetaEnum? PropositoCarpeta { get; set; }
}