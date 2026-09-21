using System.Globalization;
using Simples.Core;
using Simples.Core.Anexos;
using Simples.Core.Calculo;

namespace Simples.Tests;

/// <summary>
/// Estes testes trocam a cultura da thread de propósito.
/// </summary>
/// <remarks>
/// O bug que eles cobrem passou despercebido localmente e só apareceu na
/// integração contínua: no Windows em português a tabela saía com "9.360,00" e
/// no Linux com "9,360.00". Rodar a mesma asserção sob duas culturas é o que
/// transforma isso em erro na máquina de quem escreveu, e não três horas
/// depois.
/// </remarks>
public class FormatoTests : IDisposable
{
    private readonly CultureInfo original = CultureInfo.CurrentCulture;

    private static void Sob(string cultura, Action verificar)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
        verificar();
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = original;
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("")]
    public void A_moeda_sai_em_formato_brasileiro_em_qualquer_cultura(string cultura)
    {
        Sob(cultura, () => Formato.Moeda(9_360m).Should().Be("9.360,00"));
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void O_percentual_usa_virgula_decimal(string cultura)
    {
        Sob(cultura, () => Formato.Percentual(11.20m).Should().Be("11,20%"));
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void A_razao_vira_percentual(string cultura)
    {
        Sob(cultura, () => Formato.Razao(0.30m).Should().Be("30,00%"));
    }

    [Fact]
    public void O_numero_negativo_leva_o_sinal_na_frente()
    {
        Formato.Moeda(-1_234.5m).Should().Be("-1.234,50");
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void A_faixa_se_descreve_igual_em_qualquer_cultura(string cultura)
    {
        Sob(cultura, () =>
            Tabelas.III[1].ToString().Should().Be("faixa 2: até 360.000, 11,20% − 9.360,00"));
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void O_detalhamento_sai_igual_em_qualquer_cultura(string cultura)
    {
        Sob(cultura, () =>
        {
            var detalhe = new Calculadora().Apurar(new PedidoDeApuracao
            {
                Anexo = Anexo.III,
                Rbt12 = 300_000m,
                ReceitaDoMes = 25_000m,
                FolhaDe12Meses = 90_000m,
                Competencia = new DateOnly(2026, 9, 1),
            }).Detalhar();

            detalhe.Should().Contain("Competência 09/2026");
            detalhe.Should().Contain("RBT12 300.000,00");
            detalhe.Should().Contain("alíquota efetiva 8,0800%");
            detalhe.Should().Contain("DAS 2.020,00");
            detalhe.Should().NotContain("2,020.00");
        });
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void A_observacao_do_fator_r_sai_igual_em_qualquer_cultura(string cultura)
    {
        Sob(cultura, () =>
        {
            var apuracao = new Calculadora().Apurar(new PedidoDeApuracao
            {
                Anexo = Anexo.V,
                Rbt12 = 300_000m,
                ReceitaDoMes = 25_000m,
                FolhaDe12Meses = 90_000m,
            });

            apuracao.Observacoes.Should().Contain(texto => texto.Contains("30,00%"));
        });
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void A_mensagem_de_teto_sai_igual_em_qualquer_cultura(string cultura)
    {
        Sob(cultura, () =>
        {
            var acao = () => Tabelas.FaixaDe(Anexo.I, 5_000_000m);

            acao.Should().Throw<InvalidOperationException>().WithMessage("*5.000.000,00*");
        });
    }
}
