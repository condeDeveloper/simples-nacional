using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Simples.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> fabrica;

    public ApiTests(WebApplicationFactory<Program> fabrica)
    {
        this.fabrica = fabrica;
    }

    private static object Pedido(int anexo = 1, decimal rbt12 = 300_000, decimal receita = 25_000, decimal folha = 0)
        => new { anexo, rbt12, receitaDoMes = receita, folhaDe12Meses = folha, competencia = "2026-09-01" };

    [Fact]
    public async Task Responde_a_verificacao_de_saude()
    {
        (await fabrica.CreateClient().GetAsync("/saude")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Lista_os_anexos()
    {
        var corpo = await fabrica.CreateClient().GetFromJsonAsync<JsonElement>("/anexos");

        corpo.GetArrayLength().Should().Be(5);
    }

    [Fact]
    public async Task Lista_as_faixas_de_um_anexo()
    {
        var corpo = await fabrica.CreateClient().GetFromJsonAsync<JsonElement>("/anexos/III/faixas");

        corpo.GetArrayLength().Should().Be(6);
        corpo[0].GetProperty("aliquota").GetDecimal().Should().Be(6.00m);
    }

    [Fact]
    public async Task Devolve_os_limites()
    {
        var corpo = await fabrica.CreateClient().GetFromJsonAsync<JsonElement>("/limites");

        corpo.GetProperty("tetoAnual").GetDecimal().Should().Be(4_800_000m);
        corpo.GetProperty("sublimite").GetDecimal().Should().Be(3_600_000m);
    }

    [Fact]
    public async Task Calcula_o_fator_r()
    {
        var corpo = await fabrica.CreateClient()
            .GetFromJsonAsync<JsonElement>("/fator-r?folha=90000&rbt12=300000");

        corpo.GetProperty("alcancaOLimite").GetBoolean().Should().BeTrue();
        corpo.GetProperty("anexo").GetString().Should().Be("III");
    }

    [Fact]
    public async Task O_fator_r_aponta_quanto_falta_de_folha()
    {
        var corpo = await fabrica.CreateClient()
            .GetFromJsonAsync<JsonElement>("/fator-r?folha=60000&rbt12=300000");

        corpo.GetProperty("anexo").GetString().Should().Be("V");
        corpo.GetProperty("folhaQueFalta").GetDecimal().Should().Be(24_000m);
    }

    [Fact]
    public async Task Fator_r_com_valor_negativo_reclama()
    {
        var resposta = await fabrica.CreateClient().GetAsync("/fator-r?folha=-1&rbt12=300000");

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Apura_o_das()
    {
        var resposta = await fabrica.CreateClient().PostAsJsonAsync("/apuracoes", Pedido(3, 300_000, 25_000, 90_000));
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        corpo.GetProperty("aliquotaEfetiva").GetDecimal().Should().Be(8.08m);
        corpo.GetProperty("das").GetDecimal().Should().Be(2_020.00m);
        corpo.GetProperty("faixa").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task A_apuracao_traz_a_reparticao_e_os_destinos()
    {
        var resposta = await fabrica.CreateClient().PostAsJsonAsync("/apuracoes", Pedido(1, 180_000, 15_000));
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        corpo.GetProperty("parcelas").GetArrayLength().Should().Be(6);
        corpo.GetProperty("porDestino").GetProperty("estado").GetDecimal().Should().Be(204.00m);
    }

    [Fact]
    public async Task A_apuracao_avisa_quando_o_fator_r_trocou_o_anexo()
    {
        var resposta = await fabrica.CreateClient().PostAsJsonAsync("/apuracoes", Pedido(5, 300_000, 25_000, 90_000));
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        corpo.GetProperty("anexoInformado").GetString().Should().Be("V");
        corpo.GetProperty("anexoAplicado").GetString().Should().Be("III");
        corpo.GetProperty("oFatorRMudouOAnexo").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Apuracao_acima_do_teto_reclama()
    {
        var resposta = await fabrica.CreateClient().PostAsJsonAsync("/apuracoes", Pedido(1, 5_000_000));

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Anexo_inexistente_reclama()
    {
        var resposta = await fabrica.CreateClient().PostAsJsonAsync("/apuracoes", Pedido(anexo: 9));

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Compara_os_dois_anexos_do_fator_r()
    {
        var corpo = await fabrica.CreateClient()
            .GetFromJsonAsync<JsonElement>("/comparacoes?rbt12=300000&receitaDoMes=25000");

        corpo.GetProperty("anexoIII").GetDecimal().Should().Be(2_020.00m);
        corpo.GetProperty("anexoV").GetDecimal().Should().Be(4_125.00m);
        corpo.GetProperty("diferenca").GetDecimal().Should().Be(2_105.00m);
    }

    [Fact]
    public async Task Comparacao_com_receita_invalida_reclama()
    {
        var resposta = await fabrica.CreateClient().GetAsync("/comparacoes?rbt12=9000000&receitaDoMes=1000");

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
