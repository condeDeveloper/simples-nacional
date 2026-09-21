using System.Globalization;

namespace Simples.Core;

/// <summary>
/// Formatação de número no padrão brasileiro, independente da máquina.
/// </summary>
/// <remarks>
/// Deixar o <c>ToString</c> usar a cultura corrente parece inofensivo até o
/// código sair do notebook: no Windows em português sai "9.360,00" e num
/// servidor Linux sai "9,360.00". Para um cálculo de imposto brasileiro, a
/// segunda forma é simplesmente errada. O formato é montado à mão em vez de
/// pedir a cultura pt-BR porque ela não existe quando a aplicação roda com
/// globalização invariante.
/// </remarks>
public static class Formato
{
    /// <summary>Separador de milhar com ponto e decimal com vírgula.</summary>
    public static NumberFormatInfo Brasileiro { get; } = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
        NumberNegativePattern = 1,
        PercentDecimalSeparator = ",",
        PercentGroupSeparator = ".",
        PercentPositivePattern = 1,
        PercentNegativePattern = 1,
    };

    /// <summary>Um valor em reais, com duas casas.</summary>
    public static string Moeda(decimal valor) => valor.ToString("N2", Brasileiro);

    /// <summary>Um número com a quantidade de casas informada.</summary>
    public static string Numero(decimal valor, int casas = 2) => valor.ToString($"N{casas}", Brasileiro);

    /// <summary>Um percentual já em pontos percentuais, como 11,20 para 11,20%.</summary>
    public static string Percentual(decimal pontos, int casas = 2) => $"{Numero(pontos, casas)}%";

    /// <summary>Uma razão entre zero e um mostrada como percentual.</summary>
    public static string Razao(decimal razao, int casas = 2) => Percentual(razao * 100, casas);
}
