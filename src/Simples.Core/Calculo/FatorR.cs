using Simples.Core.Anexos;

namespace Simples.Core.Calculo;

/// <summary>
/// O fator R: o peso da folha de pagamento sobre a receita.
/// </summary>
/// <remarks>
/// É a regra que decide se um prestador de serviço intelectual paga pelo Anexo
/// III ou pelo Anexo V, e a diferença é enorme — 6% contra 15,5% na primeira
/// faixa. A lógica é que quem emprega já contribui pela folha, então paga menos
/// no DAS. A fronteira é 28%, e é por isso que tanto escritório discute
/// pró-labore em dezembro.
/// </remarks>
public static class FatorR
{
    /// <summary>A fronteira entre o Anexo III e o Anexo V.</summary>
    public const decimal Limite = 0.28m;

    /// <summary>
    /// A razão entre a folha dos últimos doze meses e a receita do mesmo
    /// período. Receita zerada devolve zero em vez de dividir por zero.
    /// </summary>
    public static decimal Calcular(decimal folhaDe12Meses, decimal rbt12)
    {
        if (folhaDe12Meses < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(folhaDe12Meses), "A folha não pode ser negativa.");
        }

        if (rbt12 < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rbt12), "A receita não pode ser negativa.");
        }

        if (rbt12 == 0)
        {
            return 0m;
        }

        return Math.Round(folhaDe12Meses / rbt12, 6, MidpointRounding.AwayFromZero);
    }

    /// <summary>Indica se o fator alcança os 28% que levam ao Anexo III.</summary>
    public static bool Alcanca(decimal fator) => fator >= Limite;

    /// <summary>
    /// O anexo que vale de fato. Só faz sentido para as atividades sujeitas à
    /// regra; nos demais anexos o informado é devolvido sem mudança.
    /// </summary>
    public static Anexo Aplicar(Anexo anexoInformado, decimal folhaDe12Meses, decimal rbt12)
    {
        if (!Anexos.Anexos.DependeDoFatorR(anexoInformado))
        {
            return anexoInformado;
        }

        return Alcanca(Calcular(folhaDe12Meses, rbt12)) ? Anexo.III : Anexo.V;
    }

    /// <summary>
    /// Quanto de folha ainda falta para alcançar os 28%. Devolve zero quando
    /// o fator já alcançou.
    /// </summary>
    public static decimal FolhaQueFalta(decimal folhaDe12Meses, decimal rbt12)
    {
        var necessaria = rbt12 * Limite;
        return Math.Max(0, Math.Round(necessaria - folhaDe12Meses, 2, MidpointRounding.AwayFromZero));
    }
}
