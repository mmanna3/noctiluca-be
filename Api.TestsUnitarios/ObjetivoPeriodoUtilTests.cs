using Api.Core.Enums;
using Api.Core.Otros;
using Xunit;

namespace Api.TestsUnitarios;

public class ObjetivoPeriodoUtilTests
{
    [Fact]
    public void ClaveDia_FormatoCorrecto()
    {
        var fecha = new DateTime(2026, 6, 24);
        Assert.Equal("2026-06-24", ObjetivoPeriodoUtil.ClaveDia(fecha));
    }

    [Fact]
    public void ClaveMes_FormatoCorrecto()
    {
        var fecha = new DateTime(2026, 6, 15);
        Assert.Equal("2026-06", ObjetivoPeriodoUtil.ClaveMes(fecha));
    }

    [Fact]
    public void ObtenerRangoPeriodo_Dia_MismoDia()
    {
        var fecha = new DateTime(2026, 6, 24);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoPeriodo(TipoListaObjetivoEnum.Dia, fecha);
        Assert.Equal(fecha.Date, inicio);
        Assert.Equal(fecha.Date, fin);
    }

    [Fact]
    public void ObtenerRangoPeriodo_Semana_LunesADomingo()
    {
        var miercoles = new DateTime(2026, 6, 24);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoPeriodo(
            TipoListaObjetivoEnum.Semana,
            miercoles);

        Assert.Equal(DayOfWeek.Monday, inicio.DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, fin.DayOfWeek);
        Assert.Equal(6, (fin - inicio).Days);
    }

    [Fact]
    public void ObtenerRangoDesdeClave_Semana_CoincideConClaveGenerada()
    {
        var fecha = new DateTime(2026, 6, 24);
        var clave = ObjetivoPeriodoUtil.ClaveSemana(fecha);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoDesdeClave(
            TipoListaObjetivoEnum.Semana,
            clave);

        Assert.True(fecha.Date >= inicio);
        Assert.True(fecha.Date <= fin);
    }

    [Fact]
    public void LimiteRecomendadoDia_EsSiete()
    {
        Assert.Equal(7, ObjetivoPeriodoUtil.LimiteRecomendadoDia);
    }
}
