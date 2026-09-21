using Simples.Core.Anexos;

namespace Simples.Core.Calculo;

/// <summary>
/// Apura o DAS de uma competência.
/// </summary>
/// <remarks>
/// A conta toda cabe em uma linha — <c>(RBT12 × alíquota − parcela a deduzir)
/// ÷ RBT12</c> — e é justamente por parecer simples que ela é feita errado. O
/// erro clássico é aplicar a alíquota nominal da tabela direto sobre a receita
/// do mês, ignorando a dedução: na segunda faixa do Anexo III isso cobra 11,2%
/// onde o correto é algo entre 6% e 8,6%.
/// </remarks>
public sealed class Calculadora
{
    /// <summary>Apura um mês.</summary>
    public Apuracao Apurar(PedidoDeApuracao pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        pedido.Validar();

        var observacoes = new List<string>();
        var anexo = pedido.Anexo;
        decimal? fatorR = null;

        if (Anexos.Anexos.DependeDoFatorR(pedido.Anexo))
        {
            fatorR = FatorR.Calcular(pedido.FolhaDe12Meses, pedido.Rbt12);
            anexo = FatorR.Aplicar(pedido.Anexo, pedido.FolhaDe12Meses, pedido.Rbt12);

            if (anexo != pedido.Anexo)
            {
                observacoes.Add(
                    $"O fator R de {Formato.Razao(fatorR.Value)} " +
                    $"{(FatorR.Alcanca(fatorR.Value) ? "alcançou" : "não alcançou")} " +
                    $"os {Formato.Razao(FatorR.Limite, 0)}: a apuração usa o Anexo {Anexos.Anexos.Romano(anexo)}.");
            }

            if (!FatorR.Alcanca(fatorR.Value) && pedido.Rbt12 > 0)
            {
                var falta = FatorR.FolhaQueFalta(pedido.FolhaDe12Meses, pedido.Rbt12);
                observacoes.Add($"Faltam {Formato.Moeda(falta)} de folha em doze meses para cair no Anexo III.");
            }
        }

        var faixa = Tabelas.FaixaDe(anexo, pedido.Rbt12);
        var aliquotaEfetiva = AliquotaEfetiva(pedido.Rbt12, faixa);
        var das = Math.Round(pedido.ReceitaDoMes * aliquotaEfetiva, 2, MidpointRounding.AwayFromZero);

        var parcelas = faixa.Reparticao
            .OrderBy(par => par.Key)
            .Select(par => new Parcela(
                par.Key,
                par.Value,
                Math.Round(das * par.Value / 100m, 2, MidpointRounding.AwayFromZero)))
            .ToList();

        Ajustar(parcelas, das);

        if (Limites.PassouDoSublimite(pedido.Rbt12))
        {
            observacoes.Add(
                $"RBT12 acima do sublimite de {Formato.Moeda(Limites.Sublimite)}: ICMS e ISS saem do DAS e são " +
                "recolhidos pelo regime normal, no estado e no município.");
        }

        if (!Anexos.Anexos.RecolheCppNoDas(anexo))
        {
            observacoes.Add("No Anexo IV a CPP fica fora do DAS: a contribuição patronal é recolhida em GPS.");
        }

        return new Apuracao
        {
            Competencia = pedido.Competencia,
            AnexoInformado = pedido.Anexo,
            AnexoAplicado = anexo,
            Faixa = faixa,
            Rbt12 = pedido.Rbt12,
            ReceitaDoMes = pedido.ReceitaDoMes,
            FatorR = fatorR,
            AliquotaEfetiva = aliquotaEfetiva,
            Das = das,
            Parcelas = parcelas,
            Observacoes = observacoes,
        };
    }

    /// <summary>
    /// A alíquota que realmente se paga. Com receita zerada a fórmula
    /// dividiria por zero; nesse caso vale a alíquota nominal da primeira
    /// faixa, que é onde a empresa está.
    /// </summary>
    public static decimal AliquotaEfetiva(decimal rbt12, Faixa faixa)
    {
        ArgumentNullException.ThrowIfNull(faixa);

        if (rbt12 <= 0)
        {
            return faixa.Aliquota / 100m;
        }

        var efetiva = ((rbt12 * (faixa.Aliquota / 100m)) - faixa.ParcelaADeduzir) / rbt12;
        return Math.Round(Math.Max(0, efetiva), 8, MidpointRounding.AwayFromZero);
    }

    /// <summary>A alíquota efetiva de um anexo para uma receita, em percentual.</summary>
    public static decimal AliquotaEfetivaEmPercentual(Anexo anexo, decimal rbt12)
        => Math.Round(AliquotaEfetiva(rbt12, Tabelas.FaixaDe(anexo, rbt12)) * 100, 4, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Compara o que a empresa pagaria em cada anexo sujeito ao fator R.
    /// Serve para mostrar o tamanho da diferença antes de decidir pró-labore.
    /// </summary>
    public IReadOnlyDictionary<Anexo, decimal> CompararFatorR(decimal rbt12, decimal receitaDoMes)
    {
        var comparacao = new Dictionary<Anexo, decimal>();

        foreach (var anexo in new[] { Anexo.III, Anexo.V })
        {
            var faixa = Tabelas.FaixaDe(anexo, rbt12);
            comparacao[anexo] = Math.Round(
                receitaDoMes * AliquotaEfetiva(rbt12, faixa), 2, MidpointRounding.AwayFromZero);
        }

        return comparacao;
    }

    /// <summary>
    /// Joga a diferença de arredondamento na maior parcela.
    /// </summary>
    /// <remarks>
    /// Arredondar cada tributo separadamente faz a soma não fechar com o DAS
    /// por um ou dois centavos. Como a guia é uma só, a sobra precisa ir para
    /// algum lugar, e o critério usual é a parcela de maior valor.
    /// </remarks>
    private static void Ajustar(List<Parcela> parcelas, decimal das)
    {
        if (parcelas.Count == 0)
        {
            return;
        }

        var diferenca = das - parcelas.Sum(parcela => parcela.Valor);

        if (diferenca == 0)
        {
            return;
        }

        var maior = parcelas.IndexOf(parcelas.MaxBy(parcela => parcela.Valor)!);
        parcelas[maior] = parcelas[maior] with { Valor = parcelas[maior].Valor + diferenca };
    }
}
