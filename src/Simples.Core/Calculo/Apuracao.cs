using Simples.Core.Anexos;
using Simples.Core.Tributos;

namespace Simples.Core.Calculo;

/// <summary>Quanto de um tributo saiu do DAS.</summary>
/// <param name="Tributo">O tributo.</param>
/// <param name="Percentual">A fatia dele na repartição da faixa.</param>
/// <param name="Valor">O valor em reais.</param>
public sealed record Parcela(Tributo Tributo, decimal Percentual, decimal Valor)
{
    /// <summary>Quem recebe o dinheiro.</summary>
    public string Destino => Tributos.Tributos.Destino(Tributo);

    /// <inheritdoc />
    public override string ToString()
        => $"{Tributos.Tributos.Nome(Tributo)}: {Formato.Moeda(Valor)} " +
           $"({Formato.Percentual(Percentual)}, {Destino})";
}

/// <summary>
/// O resultado da apuração de um mês: a alíquota que realmente se paga, o
/// valor do DAS e para onde cada real vai.
/// </summary>
public sealed record Apuracao
{
    /// <summary>Competência apurada.</summary>
    public required DateOnly Competencia { get; init; }

    /// <summary>Anexo informado no pedido.</summary>
    public required Anexo AnexoInformado { get; init; }

    /// <summary>Anexo usado de fato, que o fator R pode ter trocado.</summary>
    public required Anexo AnexoAplicado { get; init; }

    /// <summary>Faixa em que a receita caiu.</summary>
    public required Faixa Faixa { get; init; }

    /// <summary>Receita dos últimos doze meses.</summary>
    public required decimal Rbt12 { get; init; }

    /// <summary>Receita do mês.</summary>
    public required decimal ReceitaDoMes { get; init; }

    /// <summary>Fator R, quando ele foi calculado.</summary>
    public decimal? FatorR { get; init; }

    /// <summary>A alíquota que sai depois da parcela a deduzir.</summary>
    public required decimal AliquotaEfetiva { get; init; }

    /// <summary>O valor do DAS.</summary>
    public required decimal Das { get; init; }

    /// <summary>Como o DAS se reparte entre os tributos.</summary>
    public required IReadOnlyList<Parcela> Parcelas { get; init; }

    /// <summary>Indica se ICMS e ISS saíram do DAS por causa do sublimite.</summary>
    public bool AcimaDoSublimite => Limites.PassouDoSublimite(Rbt12);

    /// <summary>Observações sobre a apuração.</summary>
    public IReadOnlyList<string> Observacoes { get; init; } = [];

    /// <summary>Indica se o fator R mudou o anexo.</summary>
    public bool OFatorRMudouOAnexo => AnexoInformado != AnexoAplicado;

    /// <summary>A alíquota efetiva em percentual, arredondada para leitura.</summary>
    public decimal AliquotaEfetivaEmPercentual => Math.Round(AliquotaEfetiva * 100, 4, MidpointRounding.AwayFromZero);

    /// <summary>Quanto do DAS foi para cada ente federativo.</summary>
    public IReadOnlyDictionary<string, decimal> PorDestino()
        => Parcelas
            .GroupBy(parcela => parcela.Destino)
            .ToDictionary(grupo => grupo.Key, grupo => Math.Round(grupo.Sum(p => p.Valor), 2, MidpointRounding.AwayFromZero));

    /// <summary>A apuração aberta, pronta para mostrar.</summary>
    public string Detalhar()
    {
        var linhas = new List<string>
        {
            $"Competência {Competencia:MM/yyyy} — Anexo {Anexos.Anexos.Romano(AnexoAplicado)} ({Anexos.Anexos.Descrever(AnexoAplicado)})",
            $"  RBT12 {Formato.Moeda(Rbt12)} — {Faixa}",
            $"  receita do mês {Formato.Moeda(ReceitaDoMes)}",
            $"  alíquota efetiva {Formato.Percentual(AliquotaEfetivaEmPercentual, 4)}",
            $"  DAS {Formato.Moeda(Das)}",
        };

        foreach (var parcela in Parcelas)
        {
            linhas.Add($"    {parcela}");
        }

        foreach (var observacao in Observacoes)
        {
            linhas.Add($"  * {observacao}");
        }

        return string.Join(Environment.NewLine, linhas);
    }
}
