using Simples.Core.Tributos;

namespace Simples.Core.Anexos;

/// <summary>
/// As tabelas dos cinco anexos, como estão na Lei Complementar 123/2006 com a
/// redação da Lei Complementar 155/2016, em vigor desde 2018.
/// </summary>
/// <remarks>
/// Repare no salto da sexta faixa: a alíquota nominal dispara e a parcela a
/// deduzir também, porque o ICMS e o ISS saem da repartição — acima do
/// sublimite eles são recolhidos por fora do Simples.
/// </remarks>
public static class Tabelas
{
    private const decimal F1 = 180_000m;
    private const decimal F2 = 360_000m;
    private const decimal F3 = 720_000m;
    private const decimal F4 = 1_800_000m;
    private const decimal F5 = 3_600_000m;
    private const decimal F6 = 4_800_000m;

    /// <summary>Anexo I — comércio.</summary>
    public static IReadOnlyList<Faixa> I { get; } =
    [
        Montar(1, F1, 4.00m, 0m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.74m), (Tributo.PisPasep, 2.76m), (Tributo.Cpp, 41.50m), (Tributo.Icms, 34.00m)),
        Montar(2, F2, 7.30m, 5_940m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.74m), (Tributo.PisPasep, 2.76m), (Tributo.Cpp, 41.50m), (Tributo.Icms, 34.00m)),
        Montar(3, F3, 9.50m, 13_860m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.74m), (Tributo.PisPasep, 2.76m), (Tributo.Cpp, 42.00m), (Tributo.Icms, 33.50m)),
        Montar(4, F4, 10.70m, 22_500m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.74m), (Tributo.PisPasep, 2.76m), (Tributo.Cpp, 42.00m), (Tributo.Icms, 33.50m)),
        Montar(5, F5, 14.30m, 87_300m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.74m), (Tributo.PisPasep, 2.76m), (Tributo.Cpp, 42.00m), (Tributo.Icms, 33.50m)),
        Montar(6, F6, 19.00m, 378_000m, (Tributo.Irpj, 13.50m), (Tributo.Csll, 10.00m), (Tributo.Cofins, 28.27m), (Tributo.PisPasep, 6.13m), (Tributo.Cpp, 42.10m)),
    ];

    /// <summary>Anexo II — indústria.</summary>
    public static IReadOnlyList<Faixa> II { get; } =
    [
        Montar(1, F1, 4.50m, 0m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 11.51m), (Tributo.PisPasep, 2.49m), (Tributo.Cpp, 37.50m), (Tributo.Ipi, 7.50m), (Tributo.Icms, 32.00m)),
        Montar(2, F2, 7.80m, 5_940m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 11.51m), (Tributo.PisPasep, 2.49m), (Tributo.Cpp, 37.50m), (Tributo.Ipi, 7.50m), (Tributo.Icms, 32.00m)),
        Montar(3, F3, 10.00m, 13_860m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 11.51m), (Tributo.PisPasep, 2.49m), (Tributo.Cpp, 37.50m), (Tributo.Ipi, 7.50m), (Tributo.Icms, 32.00m)),
        Montar(4, F4, 11.20m, 22_500m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 11.51m), (Tributo.PisPasep, 2.49m), (Tributo.Cpp, 37.50m), (Tributo.Ipi, 7.50m), (Tributo.Icms, 32.00m)),
        Montar(5, F5, 14.70m, 85_500m, (Tributo.Irpj, 5.50m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 11.51m), (Tributo.PisPasep, 2.49m), (Tributo.Cpp, 37.50m), (Tributo.Ipi, 7.50m), (Tributo.Icms, 32.00m)),
        Montar(6, F6, 30.00m, 720_000m, (Tributo.Irpj, 8.50m), (Tributo.Csll, 7.50m), (Tributo.Cofins, 20.96m), (Tributo.PisPasep, 4.54m), (Tributo.Cpp, 23.50m), (Tributo.Ipi, 35.00m)),
    ];

    /// <summary>Anexo III — serviços em geral.</summary>
    public static IReadOnlyList<Faixa> III { get; } =
    [
        Montar(1, F1, 6.00m, 0m, (Tributo.Irpj, 4.00m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.82m), (Tributo.PisPasep, 2.78m), (Tributo.Cpp, 43.40m), (Tributo.Iss, 33.50m)),
        Montar(2, F2, 11.20m, 9_360m, (Tributo.Irpj, 4.00m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 14.05m), (Tributo.PisPasep, 3.05m), (Tributo.Cpp, 43.40m), (Tributo.Iss, 32.00m)),
        Montar(3, F3, 13.50m, 17_640m, (Tributo.Irpj, 4.00m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 13.64m), (Tributo.PisPasep, 2.96m), (Tributo.Cpp, 43.40m), (Tributo.Iss, 32.50m)),
        Montar(4, F4, 16.00m, 35_640m, (Tributo.Irpj, 4.00m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 13.64m), (Tributo.PisPasep, 2.96m), (Tributo.Cpp, 43.40m), (Tributo.Iss, 32.50m)),
        Montar(5, F5, 21.00m, 125_640m, (Tributo.Irpj, 4.00m), (Tributo.Csll, 3.50m), (Tributo.Cofins, 12.82m), (Tributo.PisPasep, 2.78m), (Tributo.Cpp, 43.40m), (Tributo.Iss, 33.50m)),
        Montar(6, F6, 33.00m, 648_000m, (Tributo.Irpj, 35.00m), (Tributo.Csll, 15.00m), (Tributo.Cofins, 16.03m), (Tributo.PisPasep, 3.47m), (Tributo.Cpp, 30.50m)),
    ];

    /// <summary>Anexo IV — serviços sem CPP no DAS.</summary>
    public static IReadOnlyList<Faixa> IV { get; } =
    [
        Montar(1, F1, 4.50m, 0m, (Tributo.Irpj, 18.80m), (Tributo.Csll, 15.20m), (Tributo.Cofins, 17.67m), (Tributo.PisPasep, 3.83m), (Tributo.Iss, 44.50m)),
        Montar(2, F2, 9.00m, 8_100m, (Tributo.Irpj, 19.80m), (Tributo.Csll, 15.20m), (Tributo.Cofins, 20.55m), (Tributo.PisPasep, 4.45m), (Tributo.Iss, 40.00m)),
        Montar(3, F3, 10.20m, 12_420m, (Tributo.Irpj, 20.80m), (Tributo.Csll, 15.20m), (Tributo.Cofins, 19.73m), (Tributo.PisPasep, 4.27m), (Tributo.Iss, 40.00m)),
        Montar(4, F4, 14.00m, 39_780m, (Tributo.Irpj, 17.80m), (Tributo.Csll, 19.20m), (Tributo.Cofins, 18.90m), (Tributo.PisPasep, 4.10m), (Tributo.Iss, 40.00m)),
        Montar(5, F5, 22.00m, 183_780m, (Tributo.Irpj, 18.80m), (Tributo.Csll, 19.20m), (Tributo.Cofins, 18.08m), (Tributo.PisPasep, 3.92m), (Tributo.Iss, 40.00m)),
        Montar(6, F6, 33.00m, 828_000m, (Tributo.Irpj, 53.50m), (Tributo.Csll, 21.50m), (Tributo.Cofins, 20.55m), (Tributo.PisPasep, 4.45m)),
    ];

    /// <summary>Anexo V — serviços intelectuais.</summary>
    public static IReadOnlyList<Faixa> V { get; } =
    [
        Montar(1, F1, 15.50m, 0m, (Tributo.Irpj, 25.00m), (Tributo.Csll, 15.00m), (Tributo.Cofins, 14.10m), (Tributo.PisPasep, 3.05m), (Tributo.Cpp, 28.85m), (Tributo.Iss, 14.00m)),
        Montar(2, F2, 18.00m, 4_500m, (Tributo.Irpj, 23.00m), (Tributo.Csll, 15.00m), (Tributo.Cofins, 14.10m), (Tributo.PisPasep, 3.05m), (Tributo.Cpp, 27.85m), (Tributo.Iss, 17.00m)),
        Montar(3, F3, 19.50m, 9_900m, (Tributo.Irpj, 24.00m), (Tributo.Csll, 15.00m), (Tributo.Cofins, 14.92m), (Tributo.PisPasep, 3.23m), (Tributo.Cpp, 23.85m), (Tributo.Iss, 19.00m)),
        Montar(4, F4, 20.50m, 17_100m, (Tributo.Irpj, 21.00m), (Tributo.Csll, 15.00m), (Tributo.Cofins, 15.74m), (Tributo.PisPasep, 3.41m), (Tributo.Cpp, 23.85m), (Tributo.Iss, 21.00m)),
        Montar(5, F5, 23.00m, 62_100m, (Tributo.Irpj, 23.00m), (Tributo.Csll, 12.50m), (Tributo.Cofins, 14.10m), (Tributo.PisPasep, 3.05m), (Tributo.Cpp, 23.85m), (Tributo.Iss, 23.50m)),
        Montar(6, F6, 30.50m, 540_000m, (Tributo.Irpj, 35.00m), (Tributo.Csll, 15.50m), (Tributo.Cofins, 16.44m), (Tributo.PisPasep, 3.56m), (Tributo.Cpp, 29.50m)),
    ];

    /// <summary>A tabela de um anexo.</summary>
    public static IReadOnlyList<Faixa> Do(Anexo anexo) => anexo switch
    {
        Anexo.I => I,
        Anexo.II => II,
        Anexo.III => III,
        Anexo.IV => IV,
        Anexo.V => V,
        _ => throw new ArgumentOutOfRangeException(nameof(anexo), anexo, "Anexo desconhecido."),
    };

    /// <summary>A faixa em que a receita dos últimos doze meses cai.</summary>
    public static Faixa FaixaDe(Anexo anexo, decimal rbt12)
    {
        if (rbt12 < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rbt12), "A receita não pode ser negativa.");
        }

        foreach (var faixa in Do(anexo))
        {
            if (faixa.Cabe(rbt12))
            {
                return faixa;
            }
        }

        throw new InvalidOperationException(
            $"Receita de {rbt12:N2} passa do teto do Simples Nacional.");
    }

    private static Faixa Montar(
        int numero,
        decimal ate,
        decimal aliquota,
        decimal deduzir,
        params (Tributo Tributo, decimal Percentual)[] reparticao) => new Faixa
        {
            Numero = numero,
            AteRbt12 = ate,
            Aliquota = aliquota,
            ParcelaADeduzir = deduzir,
            Reparticao = reparticao.ToDictionary(par => par.Tributo, par => par.Percentual),
        }.Validar();
}
