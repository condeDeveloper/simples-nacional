using Simples.Core.Anexos;
using Simples.Core.Tributos;

namespace Simples.Tests;

public class TabelasTests
{
    [Theory]
    [InlineData(Anexo.I)]
    [InlineData(Anexo.II)]
    [InlineData(Anexo.III)]
    [InlineData(Anexo.IV)]
    [InlineData(Anexo.V)]
    public void Todo_anexo_tem_seis_faixas(Anexo anexo)
    {
        Tabelas.Do(anexo).Should().HaveCount(6);
    }

    [Theory]
    [InlineData(Anexo.I)]
    [InlineData(Anexo.II)]
    [InlineData(Anexo.III)]
    [InlineData(Anexo.IV)]
    [InlineData(Anexo.V)]
    public void A_reparticao_de_toda_faixa_soma_cem_por_cento(Anexo anexo)
    {
        foreach (var faixa in Tabelas.Do(anexo))
        {
            faixa.Reparticao.Values.Sum().Should().BeApproximately(100m, 0.01m);
        }
    }

    [Theory]
    [InlineData(Anexo.I)]
    [InlineData(Anexo.II)]
    [InlineData(Anexo.III)]
    [InlineData(Anexo.IV)]
    [InlineData(Anexo.V)]
    public void Os_tetos_das_faixas_sao_crescentes(Anexo anexo)
    {
        Tabelas.Do(anexo).Select(faixa => faixa.AteRbt12).Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(Anexo.I)]
    [InlineData(Anexo.II)]
    [InlineData(Anexo.III)]
    [InlineData(Anexo.IV)]
    [InlineData(Anexo.V)]
    public void A_ultima_faixa_vai_ate_o_teto_do_simples(Anexo anexo)
    {
        Tabelas.Do(anexo)[^1].AteRbt12.Should().Be(4_800_000m);
    }

    [Theory]
    [InlineData(Anexo.I)]
    [InlineData(Anexo.II)]
    [InlineData(Anexo.III)]
    [InlineData(Anexo.IV)]
    [InlineData(Anexo.V)]
    public void A_sexta_faixa_nao_tem_icms_nem_iss(Anexo anexo)
    {
        // Acima do sublimite os dois saem do DAS; a tabela reflete isso.
        var sexta = Tabelas.Do(anexo)[^1];

        sexta.Reparticao.Should().NotContainKey(Tributo.Icms);
        sexta.Reparticao.Should().NotContainKey(Tributo.Iss);
    }

    [Fact]
    public void O_comercio_reparte_icms_e_nao_iss()
    {
        var primeira = Tabelas.I[0];

        primeira.Reparticao.Should().ContainKey(Tributo.Icms);
        primeira.Reparticao.Should().NotContainKey(Tributo.Iss);
    }

    [Fact]
    public void A_industria_e_a_unica_com_ipi()
    {
        Tabelas.II[0].Reparticao.Should().ContainKey(Tributo.Ipi);
        Tabelas.I[0].Reparticao.Should().NotContainKey(Tributo.Ipi);
        Tabelas.III[0].Reparticao.Should().NotContainKey(Tributo.Ipi);
    }

    [Fact]
    public void O_anexo_quatro_nao_tem_cpp_em_faixa_nenhuma()
    {
        // É a marca do Anexo IV: a contribuição patronal é recolhida por fora.
        foreach (var faixa in Tabelas.IV)
        {
            faixa.Reparticao.Should().NotContainKey(Tributo.Cpp);
        }
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(180_000, 1)]
    [InlineData(180_000.01, 2)]
    [InlineData(360_000, 2)]
    [InlineData(700_000, 3)]
    [InlineData(1_800_000, 4)]
    [InlineData(3_600_000, 5)]
    [InlineData(4_800_000, 6)]
    public void Encontra_a_faixa_pela_receita(decimal rbt12, int faixa)
    {
        Tabelas.FaixaDe(Anexo.I, rbt12).Numero.Should().Be(faixa);
    }

    [Fact]
    public void Receita_acima_do_teto_reclama()
    {
        var acao = () => Tabelas.FaixaDe(Anexo.I, 5_000_000m);

        acao.Should().Throw<InvalidOperationException>().WithMessage("*teto*");
    }

    [Fact]
    public void Receita_negativa_reclama()
    {
        var acao = () => Tabelas.FaixaDe(Anexo.I, -1);

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void O_anexo_cinco_e_muito_mais_caro_que_o_tres_na_primeira_faixa()
    {
        // 15,5% contra 6%: é o tamanho do que está em jogo no fator R.
        Tabelas.V[0].Aliquota.Should().Be(15.50m);
        Tabelas.III[0].Aliquota.Should().Be(6.00m);
    }

    [Fact]
    public void A_faixa_se_descreve_de_volta()
    {
        Tabelas.III[1].ToString().Should().Contain("faixa 2").And.Contain("11,20%");
    }

    [Fact]
    public void Anexo_desconhecido_reclama()
    {
        var acao = () => Tabelas.Do((Anexo)99);

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class AnexoTests
{
    [Fact]
    public void So_os_anexos_tres_e_cinco_dependem_do_fator_r()
    {
        Anexos.DependeDoFatorR(Anexo.III).Should().BeTrue();
        Anexos.DependeDoFatorR(Anexo.V).Should().BeTrue();
        Anexos.DependeDoFatorR(Anexo.I).Should().BeFalse();
        Anexos.DependeDoFatorR(Anexo.IV).Should().BeFalse();
    }

    [Fact]
    public void So_o_anexo_quatro_deixa_a_cpp_de_fora()
    {
        Anexos.RecolheCppNoDas(Anexo.IV).Should().BeFalse();
        Anexos.RecolheCppNoDas(Anexo.III).Should().BeTrue();
    }

    [Fact]
    public void Descreve_a_atividade()
    {
        Anexos.Descrever(Anexo.I).Should().Be("comércio");
        Anexos.Descrever(Anexo.II).Should().Be("indústria");
    }

    [Fact]
    public void Mostra_o_numero_em_romano()
    {
        Anexos.Romano(Anexo.IV).Should().Be("IV");
    }
}

public class TributoTests
{
    [Fact]
    public void Sabe_quem_recebe_cada_tributo()
    {
        Tributos.Destino(Tributo.Icms).Should().Be("estado");
        Tributos.Destino(Tributo.Iss).Should().Be("município");
        Tributos.Destino(Tributo.Irpj).Should().Be("união");
    }

    [Fact]
    public void Sabe_quem_sai_no_sublimite()
    {
        Tributos.SaiNoSublimite(Tributo.Icms).Should().BeTrue();
        Tributos.SaiNoSublimite(Tributo.Iss).Should().BeTrue();
        Tributos.SaiNoSublimite(Tributo.Cpp).Should().BeFalse();
    }

    [Fact]
    public void Tem_o_nome_do_extrato()
    {
        Tributos.Nome(Tributo.PisPasep).Should().Be("PIS/Pasep");
        Tributos.Nome(Tributo.Cofins).Should().Be("COFINS");
    }
}
