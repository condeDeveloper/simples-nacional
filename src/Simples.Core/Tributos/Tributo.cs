namespace Simples.Core.Tributos;

/// <summary>
/// Os tributos que o DAS recolhe de uma vez só.
/// </summary>
/// <remarks>
/// A guia é uma, mas o dinheiro se reparte entre União, estado e município em
/// percentuais que mudam de faixa para faixa. É por isso que o Simples é
/// "unificado" no pagamento e não na apuração.
/// </remarks>
public enum Tributo
{
    /// <summary>Imposto de Renda da Pessoa Jurídica.</summary>
    Irpj,

    /// <summary>Contribuição Social sobre o Lucro Líquido.</summary>
    Csll,

    /// <summary>Contribuição para o Financiamento da Seguridade Social.</summary>
    Cofins,

    /// <summary>Programa de Integração Social.</summary>
    PisPasep,

    /// <summary>Contribuição Patronal Previdenciária.</summary>
    Cpp,

    /// <summary>Imposto sobre Produtos Industrializados.</summary>
    Ipi,

    /// <summary>Imposto sobre Circulação de Mercadorias e Serviços.</summary>
    Icms,

    /// <summary>Imposto Sobre Serviços.</summary>
    Iss,
}

/// <summary>Quem fica com cada tributo.</summary>
public static class Tributos
{
    /// <summary>O ente federativo que recebe o tributo.</summary>
    public static string Destino(Tributo tributo) => tributo switch
    {
        Tributo.Icms => "estado",
        Tributo.Iss => "município",
        _ => "união",
    };

    /// <summary>Nome do tributo como ele aparece no extrato do DAS.</summary>
    public static string Nome(Tributo tributo) => tributo switch
    {
        Tributo.Irpj => "IRPJ",
        Tributo.Csll => "CSLL",
        Tributo.Cofins => "COFINS",
        Tributo.PisPasep => "PIS/Pasep",
        Tributo.Cpp => "CPP",
        Tributo.Ipi => "IPI",
        Tributo.Icms => "ICMS",
        Tributo.Iss => "ISS",
        _ => tributo.ToString(),
    };

    /// <summary>
    /// Indica se o tributo sai do DAS quando a empresa passa do sublimite
    /// estadual. ICMS e ISS voltam a ser recolhidos por fora.
    /// </summary>
    public static bool SaiNoSublimite(Tributo tributo) => tributo is Tributo.Icms or Tributo.Iss;
}
