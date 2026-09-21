namespace Simples.Core.Calculo;

/// <summary>
/// Os tetos que definem quem pode ficar no Simples e o que entra no DAS.
/// </summary>
public static class Limites
{
    /// <summary>Teto de receita bruta anual para permanecer no Simples.</summary>
    public const decimal TetoAnual = 4_800_000m;

    /// <summary>
    /// Sublimite para ICMS e ISS. Acima dele os dois saem do DAS e voltam a
    /// ser recolhidos pelo regime normal, no estado e no município.
    /// </summary>
    public const decimal Sublimite = 3_600_000m;

    /// <summary>Teto de receita do microempreendedor individual.</summary>
    public const decimal TetoMei = 81_000m;

    /// <summary>Teto da microempresa.</summary>
    public const decimal TetoMicroempresa = 360_000m;

    /// <summary>Indica se a receita passou do teto do Simples.</summary>
    public static bool PassouDoTeto(decimal rbt12) => rbt12 > TetoAnual;

    /// <summary>Indica se a receita passou do sublimite de ICMS e ISS.</summary>
    public static bool PassouDoSublimite(decimal rbt12) => rbt12 > Sublimite;

    /// <summary>Indica se a receita cabe no MEI.</summary>
    public static bool CabeNoMei(decimal rbt12) => rbt12 <= TetoMei;

    /// <summary>Quanto a receita passou do teto, ou zero.</summary>
    public static decimal ExcessoDoTeto(decimal rbt12) => Math.Max(0, rbt12 - TetoAnual);

    /// <summary>O porte da empresa pela receita dos últimos doze meses.</summary>
    public static string Porte(decimal rbt12) => rbt12 switch
    {
        <= TetoMei => "MEI",
        <= TetoMicroempresa => "microempresa",
        <= TetoAnual => "empresa de pequeno porte",
        _ => "fora do Simples Nacional",
    };

    /// <summary>
    /// A receita dos últimos doze meses de quem abriu há pouco.
    /// </summary>
    /// <remarks>
    /// Empresa nova não tem doze meses de histórico, então a lei manda usar a
    /// média dos meses já corridos multiplicada por doze. Sem isso, faturar
    /// bem no primeiro mês cairia na faixa mais barata o ano inteiro.
    /// </remarks>
    public static decimal Rbt12Proporcional(IReadOnlyCollection<decimal> receitasMensais)
    {
        ArgumentNullException.ThrowIfNull(receitasMensais);

        if (receitasMensais.Count == 0)
        {
            return 0m;
        }

        if (receitasMensais.Any(receita => receita < 0))
        {
            throw new ArgumentException("Receita mensal não pode ser negativa.", nameof(receitasMensais));
        }

        if (receitasMensais.Count >= 12)
        {
            return receitasMensais.TakeLast(12).Sum();
        }

        var media = receitasMensais.Sum() / receitasMensais.Count;
        return Math.Round(media * 12, 2, MidpointRounding.AwayFromZero);
    }
}
