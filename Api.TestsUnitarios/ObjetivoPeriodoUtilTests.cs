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
    public void ClaveAnio_FormatoCorrecto()
    {
        var fecha = new DateTime(2026, 6, 15);
        Assert.Equal("2026", ObjetivoPeriodoUtil.ClaveAnio(fecha));
    }

    [Theory]
    [InlineData(2026, "2025-2029")]
    [InlineData(2025, "2025-2029")]
    [InlineData(2029, "2025-2029")]
    [InlineData(2030, "2030-2034")]
    [InlineData(2024, "2020-2024")]
    public void ClaveLustro_BloqueFijoAncladoEnMultiplosDeCinco(int anio, string claveEsperada)
    {
        var fecha = new DateTime(anio, 3, 10);
        Assert.Equal(claveEsperada, ObjetivoPeriodoUtil.ClaveLustro(fecha));
    }

    [Fact]
    public void ObtenerRangoPeriodo_Anio_UnoDeEneroAlTreintaYUnoDeDiciembre()
    {
        var fecha = new DateTime(2026, 6, 24);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoPeriodo(TipoListaObjetivoEnum.Anio, fecha);
        Assert.Equal(new DateTime(2026, 1, 1), inicio);
        Assert.Equal(new DateTime(2026, 12, 31), fin);
    }

    [Fact]
    public void ObtenerRangoPeriodo_Lustro_CincoAniosCalendario()
    {
        var fecha = new DateTime(2026, 6, 24);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoPeriodo(TipoListaObjetivoEnum.Lustro, fecha);
        Assert.Equal(new DateTime(2025, 1, 1), inicio);
        Assert.Equal(new DateTime(2029, 12, 31), fin);
    }

    [Fact]
    public void ObtenerRangoDesdeClave_Anio_CoincideConClaveGenerada()
    {
        var fecha = new DateTime(2026, 6, 24);
        var clave = ObjetivoPeriodoUtil.ClaveAnio(fecha);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoDesdeClave(TipoListaObjetivoEnum.Anio, clave);

        Assert.True(fecha.Date >= inicio);
        Assert.True(fecha.Date <= fin);
    }

    [Fact]
    public void ObtenerRangoDesdeClave_Lustro_CoincideConClaveGenerada()
    {
        var fecha = new DateTime(2026, 6, 24);
        var clave = ObjetivoPeriodoUtil.ClaveLustro(fecha);
        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoDesdeClave(TipoListaObjetivoEnum.Lustro, clave);

        Assert.True(fecha.Date >= inicio);
        Assert.True(fecha.Date <= fin);
        Assert.Equal(new DateTime(2025, 1, 1), inicio);
        Assert.Equal(new DateTime(2029, 12, 31), fin);
    }

    [Fact]
    public void ObtenerRangoDesdeClave_Lustro_ClaveInvalida_Lanza()
    {
        Assert.Throws<ArgumentException>(() =>
            ObjetivoPeriodoUtil.ObtenerRangoDesdeClave(TipoListaObjetivoEnum.Lustro, "2025-2030"));
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
