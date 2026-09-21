using Simples.Core.Anexos;
using Simples.Core.Calculo;
using Simples.Core.Tributos;

namespace Simples.Tests;

public class FatorRTests
{
    [Fact]
    public void O_fator_e_a_folha_dividida_pela_receita()
    {
        FatorR.Calcular(90_000m, 300_000m).Should().Be(0.30m);
    }

    [Fact]
    public void Receita_zerada_devolve_zero_em_vez_de_dividir_por_zero()
    {
        FatorR.Calcular(10_000m, 0m).Should().Be(0m);
    }

    [Theory]
    [InlineData(0.28, true)]
    [InlineData(0.30, true)]
    [InlineData(0.2799, false)]
    [InlineData(0, false)]
    public void O_limite_e_vinte_e_oito_por_cento(decimal fator, bool alcanca)
    {
        FatorR.Alcanca(fator).Should().Be(alcanca);
    }

    [Fact]
    public void Folha_alta_leva_ao_anexo_tres()
    {
        FatorR.Aplicar(Anexo.V, 90_000m, 300_000m).Should().Be(Anexo.III);
    }

    [Fact]
    public void Folha_baixa_leva_ao_anexo_cinco()
    {
        FatorR.Aplicar(Anexo.III, 60_000m, 300_000m).Should().Be(Anexo.V);
    }

    [Fact]
    public void Nos_outros_anexos_o_fator_nao_muda_nada()
    {
        FatorR.Aplicar(Anexo.I, 0m, 300_000m).Should().Be(Anexo.I);
        FatorR.Aplicar(Anexo.IV, 0m, 300_000m).Should().Be(Anexo.IV);
    }

    [Fact]
    public void Calcula_quanto_de_folha_ainda_falta()
    {
        // 28% de 300.000 são 84.000; com 60.000 na folha, faltam 24.000.
        FatorR.FolhaQueFalta(60_000m, 300_000m).Should().Be(24_000m);
    }

    [Fact]
    public void Quem_ja_alcancou_nao_precisa_de_mais_folha()
    {
        FatorR.FolhaQueFalta(90_000m, 300_000m).Should().Be(0m);
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(100, -1)]
    public void Valor_negativo_reclama(decimal folha, decimal rbt12)
    {
        var acao = () => FatorR.Calcular(folha, rbt12);

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class LimitesTests
{
    [Fact]
    public void Os_tetos_sao_os_da_lei()
    {
        Limites.TetoAnual.Should().Be(4_800_000m);
        Limites.Sublimite.Should().Be(3_600_000m);
        Limites.TetoMei.Should().Be(81_000m);
    }

    [Theory]
    [InlineData(4_800_000, false)]
    [InlineData(4_800_000.01, true)]
    public void Reconhece_quem_passou_do_teto(decimal rbt12, bool passou)
    {
        Limites.PassouDoTeto(rbt12).Should().Be(passou);
    }

    [Theory]
    [InlineData(3_600_000, false)]
    [InlineData(3_600_000.01, true)]
    public void Reconhece_quem_passou_do_sublimite(decimal rbt12, bool passou)
    {
        Limites.PassouDoSublimite(rbt12).Should().Be(passou);
    }

    [Theory]
    [InlineData(50_000, "MEI")]
    [InlineData(200_000, "microempresa")]
    [InlineData(1_000_000, "empresa de pequeno porte")]
    [InlineData(5_000_000, "fora do Simples Nacional")]
    public void Classifica_o_porte(decimal rbt12, string porte)
    {
        Limites.Porte(rbt12).Should().Be(porte);
    }

    [Fact]
    public void Empresa_nova_tem_a_receita_anualizada()
    {
        // Três meses com média de 100.000 projetam 1.200.000 no ano.
        Limites.Rbt12Proporcional([100_000m, 120_000m, 80_000m]).Should().Be(1_200_000m);
    }

    [Fact]
    public void Com_doze_meses_ou_mais_vale_a_soma_dos_ultimos_doze()
    {
        var quinzeMeses = Enumerable.Repeat(10_000m, 15).ToList();

        Limites.Rbt12Proporcional(quinzeMeses).Should().Be(120_000m);
    }

    [Fact]
    public void Sem_historico_a_receita_e_zero()
    {
        Limites.Rbt12Proporcional([]).Should().Be(0m);
    }

    [Fact]
    public void Receita_mensal_negativa_reclama()
    {
        var acao = () => Limites.Rbt12Proporcional([100m, -1m]);

        acao.Should().Throw<ArgumentException>();
    }
}

public class CalculadoraTests
{
    private readonly Calculadora calculadora = new();

    private static PedidoDeApuracao Pedido(
        Anexo anexo = Anexo.I,
        decimal rbt12 = 300_000m,
        decimal receita = 25_000m,
        decimal folha = 0m) => new()
        {
            Anexo = anexo,
            Rbt12 = rbt12,
            ReceitaDoMes = receita,
            FolhaDe12Meses = folha,
            Competencia = new DateOnly(2026, 9, 1),
        };

    [Fact]
    public void Na_primeira_faixa_a_efetiva_e_a_nominal()
    {
        // Sem parcela a deduzir, as duas coincidem.
        Calculadora.AliquotaEfetivaEmPercentual(Anexo.I, 180_000m).Should().Be(4.0000m);
    }

    [Fact]
    public void A_parcela_a_deduzir_derruba_a_aliquota()
    {
        // Anexo III, faixa 2: nominal 11,20%, efetiva 8,08%.
        Calculadora.AliquotaEfetivaEmPercentual(Anexo.III, 300_000m).Should().Be(8.0800m);
    }

    [Fact]
    public void O_das_e_a_receita_do_mes_vezes_a_efetiva()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.III, 300_000m, 25_000m, folha: 90_000m));

        apuracao.AliquotaEfetivaEmPercentual.Should().Be(8.0800m);
        apuracao.Das.Should().Be(2_020.00m);
    }

    [Fact]
    public void A_aliquota_efetiva_cresce_devagar_dentro_da_faixa()
    {
        // É para isso que serve a parcela a deduzir: nada de salto ao virar
        // a faixa.
        var antes = Calculadora.AliquotaEfetivaEmPercentual(Anexo.I, 360_000m);
        var depois = Calculadora.AliquotaEfetivaEmPercentual(Anexo.I, 360_000.01m);

        (depois - antes).Should().BeLessThan(0.01m);
    }

    [Fact]
    public void Receita_zerada_usa_a_nominal_da_primeira_faixa()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.I, rbt12: 0m, receita: 0m));

        apuracao.AliquotaEfetivaEmPercentual.Should().Be(4.0000m);
        apuracao.Das.Should().Be(0m);
    }

    [Fact]
    public void A_reparticao_soma_exatamente_o_das()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.I, 300_000m, 25_000m));

        apuracao.Parcelas.Sum(parcela => parcela.Valor).Should().Be(apuracao.Das);
    }

    [Fact]
    public void A_sobra_de_arredondamento_vai_para_a_maior_parcela()
    {
        // Com um valor que não divide redondo, a soma ainda tem que fechar.
        var apuracao = calculadora.Apurar(Pedido(Anexo.III, 187_333.33m, 9_876.54m, folha: 99_999m));

        apuracao.Parcelas.Sum(parcela => parcela.Valor).Should().Be(apuracao.Das);
    }

    [Fact]
    public void O_fator_r_troca_o_anexo_quando_a_folha_alcanca()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.V, 300_000m, 25_000m, folha: 90_000m));

        apuracao.AnexoInformado.Should().Be(Anexo.V);
        apuracao.AnexoAplicado.Should().Be(Anexo.III);
        apuracao.OFatorRMudouOAnexo.Should().BeTrue();
        apuracao.FatorR.Should().Be(0.30m);
    }

    [Fact]
    public void Sem_folha_o_servico_intelectual_cai_no_anexo_cinco()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.III, 300_000m, 25_000m, folha: 0m));

        apuracao.AnexoAplicado.Should().Be(Anexo.V);
        apuracao.Das.Should().Be(4_125.00m);
        apuracao.Observacoes.Should().Contain(texto => texto.Contains("Faltam"));
    }

    [Fact]
    public void A_diferenca_entre_os_dois_anexos_e_grande()
    {
        var comparacao = calculadora.CompararFatorR(300_000m, 25_000m);

        comparacao[Anexo.III].Should().Be(2_020.00m);
        comparacao[Anexo.V].Should().Be(4_125.00m);
    }

    [Fact]
    public void Nos_anexos_sem_fator_r_ele_nao_e_calculado()
    {
        calculadora.Apurar(Pedido(Anexo.I)).FatorR.Should().BeNull();
    }

    [Fact]
    public void Acima_do_sublimite_o_icms_sai_do_das()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.I, 4_000_000m, 300_000m));

        apuracao.AcimaDoSublimite.Should().BeTrue();
        apuracao.Parcelas.Should().NotContain(parcela => parcela.Tributo == Tributo.Icms);
        apuracao.Observacoes.Should().Contain(texto => texto.Contains("sublimite"));
    }

    [Fact]
    public void Abaixo_do_sublimite_o_icms_esta_no_das()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.I, 3_000_000m, 100_000m));

        apuracao.AcimaDoSublimite.Should().BeFalse();
        apuracao.Parcelas.Should().Contain(parcela => parcela.Tributo == Tributo.Icms);
    }

    [Fact]
    public void O_anexo_quatro_avisa_que_a_cpp_fica_de_fora()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.IV, 300_000m, 25_000m));

        apuracao.Parcelas.Should().NotContain(parcela => parcela.Tributo == Tributo.Cpp);
        apuracao.Observacoes.Should().Contain(texto => texto.Contains("GPS"));
    }

    [Fact]
    public void Separa_quanto_vai_para_cada_ente()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.I, 180_000m, 15_000m));

        var destinos = apuracao.PorDestino();

        destinos.Should().ContainKeys("união", "estado");
        destinos["estado"].Should().Be(204.00m);
        destinos.Values.Sum().Should().Be(apuracao.Das);
    }

    [Fact]
    public void O_servico_manda_para_o_municipio()
    {
        var apuracao = calculadora.Apurar(Pedido(Anexo.III, 180_000m, 15_000m, folha: 100_000m));

        apuracao.PorDestino().Should().ContainKey("município");
    }

    [Fact]
    public void O_detalhamento_mostra_a_conta()
    {
        var detalhe = calculadora.Apurar(Pedido(Anexo.I, 300_000m, 25_000m)).Detalhar();

        detalhe.Should().Contain("Anexo I").And.Contain("alíquota efetiva").And.Contain("DAS");
    }

    [Fact]
    public void Receita_acima_do_teto_reclama()
    {
        var acao = () => calculadora.Apurar(Pedido(rbt12: 5_000_000m));

        acao.Should().Throw<InvalidOperationException>().WithMessage("*teto*");
    }

    [Theory]
    [InlineData(-1, 1000, 0)]
    [InlineData(1000, -1, 0)]
    [InlineData(1000, 1000, -1)]
    public void Valor_negativo_reclama(decimal rbt12, decimal receita, decimal folha)
    {
        var acao = () => calculadora.Apurar(Pedido(Anexo.I, rbt12, receita, folha));

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Monta_o_pedido_a_partir_do_historico()
    {
        var pedido = PedidoDeApuracao.DeHistorico(Anexo.I, [100_000m, 120_000m, 80_000m], 50_000m);

        pedido.Rbt12.Should().Be(1_200_000m);
        pedido.ReceitaDoMes.Should().Be(50_000m);
    }
}
