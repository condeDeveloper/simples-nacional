namespace Simples.Core.Anexos;

/// <summary>Os cinco anexos do Simples Nacional.</summary>
public enum Anexo
{
    /// <summary>Comércio.</summary>
    I = 1,

    /// <summary>Indústria.</summary>
    II = 2,

    /// <summary>Serviços em geral, e os do Anexo V quando o fator R alcança 28%.</summary>
    III = 3,

    /// <summary>Serviços sem CPP no DAS: construção, limpeza, vigilância, advocacia.</summary>
    IV = 4,

    /// <summary>Serviços intelectuais, quando o fator R não alcança 28%.</summary>
    V = 5,
}

/// <summary>Informações sobre os anexos.</summary>
public static class Anexos
{
    /// <summary>A que tipo de atividade o anexo se aplica.</summary>
    public static string Descrever(Anexo anexo) => anexo switch
    {
        Anexo.I => "comércio",
        Anexo.II => "indústria",
        Anexo.III => "serviços em geral",
        Anexo.IV => "serviços sem CPP no DAS",
        Anexo.V => "serviços intelectuais",
        _ => anexo.ToString(),
    };

    /// <summary>
    /// Indica se o anexo entra na regra do fator R, que decide entre o Anexo
    /// III e o Anexo V conforme o peso da folha de pagamento.
    /// </summary>
    public static bool DependeDoFatorR(Anexo anexo) => anexo is Anexo.III or Anexo.V;

    /// <summary>
    /// Indica se a CPP é recolhida dentro do DAS. No Anexo IV ela fica de
    /// fora: a empresa recolhe a contribuição patronal à parte, em GPS.
    /// </summary>
    public static bool RecolheCppNoDas(Anexo anexo) => anexo != Anexo.IV;

    /// <summary>O anexo em algarismos romanos.</summary>
    public static string Romano(Anexo anexo) => anexo switch
    {
        Anexo.I => "I",
        Anexo.II => "II",
        Anexo.III => "III",
        Anexo.IV => "IV",
        Anexo.V => "V",
        _ => anexo.ToString(),
    };
}
