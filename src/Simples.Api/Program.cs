using Microsoft.AspNetCore.Mvc;
using Simples.Core.Anexos;
using Simples.Core.Calculo;
using Simples.Core.Tributos;

var construtor = WebApplication.CreateBuilder(args);

construtor.Services.AddEndpointsApiExplorer();
construtor.Services.AddSwaggerGen();
construtor.Services.AddSingleton(_ => new Calculadora());

var aplicacao = construtor.Build();

if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger();
    aplicacao.UseSwaggerUI();
}

aplicacao.MapGet("/saude", () => Results.Ok(new { estado = "ok" }))
    .WithName("Saude")
    .WithTags("Serviço");

aplicacao.MapGet("/anexos", () => Results.Ok(
        Enum.GetValues<Anexo>().Select(anexo => new
        {
            anexo = Anexos.Romano(anexo),
            atividade = Anexos.Descrever(anexo),
            dependeDoFatorR = Anexos.DependeDoFatorR(anexo),
            cppNoDas = Anexos.RecolheCppNoDas(anexo),
        })))
    .WithName("Anexos")
    .WithTags("Tabelas");

aplicacao.MapGet("/anexos/{anexo}/faixas", (Anexo anexo) => Results.Ok(
        Tabelas.Do(anexo).Select(faixa => new
        {
            faixa.Numero,
            faixa.AteRbt12,
            faixa.Aliquota,
            faixa.ParcelaADeduzir,
            reparticao = faixa.Reparticao.ToDictionary(par => Tributos.Nome(par.Key), par => par.Value),
        })))
    .WithName("Faixas")
    .WithTags("Tabelas");

aplicacao.MapGet("/limites", () => Results.Ok(new
    {
        tetoAnual = Limites.TetoAnual,
        sublimite = Limites.Sublimite,
        tetoMei = Limites.TetoMei,
        tetoMicroempresa = Limites.TetoMicroempresa,
        limiteDoFatorR = FatorR.Limite,
    }))
    .WithName("Limites")
    .WithTags("Tabelas");

aplicacao.MapGet("/fator-r", (decimal folha, decimal rbt12) =>
    {
        if (folha < 0 || rbt12 < 0)
        {
            return Problema("Valores inválidos", "Folha e receita não podem ser negativas.");
        }

        var fator = FatorR.Calcular(folha, rbt12);

        return Results.Ok(new
        {
            fatorR = fator,
            emPercentual = Math.Round(fator * 100, 4),
            alcancaOLimite = FatorR.Alcanca(fator),
            anexo = Anexos.Romano(FatorR.Aplicar(Anexo.V, folha, rbt12)),
            folhaQueFalta = FatorR.FolhaQueFalta(folha, rbt12),
        });
    })
    .WithName("FatorR")
    .WithTags("Cálculo");

aplicacao.MapPost("/apuracoes", ([FromBody] PedidoEmJson pedido, Calculadora calculadora) =>
    {
        try
        {
            return Results.Ok(Resposta.De(calculadora.Apurar(pedido.ParaNucleo())));
        }
        catch (Exception erro) when (erro is ArgumentException or InvalidOperationException)
        {
            return Problema("Não foi possível apurar", erro.Message);
        }
    })
    .WithName("Apurar")
    .WithTags("Cálculo");

aplicacao.MapGet("/comparacoes", (decimal rbt12, decimal receitaDoMes, Calculadora calculadora) =>
    {
        if (rbt12 < 0 || receitaDoMes < 0 || Limites.PassouDoTeto(rbt12))
        {
            return Problema("Valores inválidos", "Confira a receita: ela precisa ser positiva e caber no teto.");
        }

        var comparacao = calculadora.CompararFatorR(rbt12, receitaDoMes);

        return Results.Ok(new
        {
            anexoIII = comparacao[Anexo.III],
            anexoV = comparacao[Anexo.V],
            diferenca = Math.Round(comparacao[Anexo.V] - comparacao[Anexo.III], 2),
        });
    })
    .WithName("Comparar")
    .WithTags("Cálculo");

aplicacao.Run();

static IResult Problema(string titulo, string detalhe) => Results.BadRequest(new ProblemDetails
{
    Title = titulo,
    Detail = detalhe,
    Status = StatusCodes.Status400BadRequest,
});

/// <summary>O pedido de apuração como ele chega pela API.</summary>
/// <param name="Anexo">Número do anexo, de 1 a 5.</param>
/// <param name="Rbt12">Receita bruta dos últimos doze meses.</param>
/// <param name="ReceitaDoMes">Receita da competência.</param>
/// <param name="FolhaDe12Meses">Folha dos últimos doze meses.</param>
/// <param name="Competencia">Mês apurado.</param>
public sealed record PedidoEmJson(
    int Anexo,
    decimal Rbt12,
    decimal ReceitaDoMes,
    decimal FolhaDe12Meses = 0,
    DateOnly? Competencia = null)
{
    /// <summary>Converte para o modelo do núcleo.</summary>
    public PedidoDeApuracao ParaNucleo()
    {
        if (!Enum.IsDefined(typeof(Anexo), Anexo))
        {
            throw new ArgumentException($"Anexo {Anexo} não existe: informe de 1 a 5.", nameof(Anexo));
        }

        return new PedidoDeApuracao
        {
            Anexo = (Anexo)Anexo,
            Rbt12 = Rbt12,
            ReceitaDoMes = ReceitaDoMes,
            FolhaDe12Meses = FolhaDe12Meses,
            Competencia = Competencia ?? DateOnly.FromDateTime(DateTime.Today),
        };
    }
}

/// <summary>A apuração como a API devolve.</summary>
public sealed record Resposta(
    string Competencia,
    string AnexoInformado,
    string AnexoAplicado,
    bool OFatorRMudouOAnexo,
    decimal? FatorR,
    int Faixa,
    decimal Rbt12,
    decimal ReceitaDoMes,
    decimal AliquotaNominal,
    decimal ParcelaADeduzir,
    decimal AliquotaEfetiva,
    decimal Das,
    bool AcimaDoSublimite,
    IReadOnlyList<object> Parcelas,
    IReadOnlyDictionary<string, decimal> PorDestino,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>Monta a resposta a partir da apuração.</summary>
    public static Resposta De(Apuracao apuracao) => new(
        apuracao.Competencia.ToString("yyyy-MM"),
        Anexos.Romano(apuracao.AnexoInformado),
        Anexos.Romano(apuracao.AnexoAplicado),
        apuracao.OFatorRMudouOAnexo,
        apuracao.FatorR,
        apuracao.Faixa.Numero,
        apuracao.Rbt12,
        apuracao.ReceitaDoMes,
        apuracao.Faixa.Aliquota,
        apuracao.Faixa.ParcelaADeduzir,
        apuracao.AliquotaEfetivaEmPercentual,
        apuracao.Das,
        apuracao.AcimaDoSublimite,
        apuracao.Parcelas
            .Select(parcela => (object)new
            {
                tributo = Tributos.Nome(parcela.Tributo),
                parcela.Percentual,
                parcela.Valor,
                parcela.Destino,
            })
            .ToList(),
        apuracao.PorDestino(),
        apuracao.Observacoes);
}

/// <summary>Exposta para que os testes de integração possam subir a API.</summary>
public partial class Program;
