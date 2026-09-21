using Simples.Core.Tributos;

namespace Simples.Core.Anexos;

/// <summary>
/// Uma faixa da tabela do anexo.
/// </summary>
/// <remarks>
/// A alíquota da tabela não é a que se paga: ela vem acompanhada de uma
/// parcela a deduzir justamente para que a passagem de uma faixa para a outra
/// seja suave. Sem a dedução, faturar um real a mais faria o imposto saltar.
/// </remarks>
public sealed record Faixa
{
    /// <summary>Número da faixa, de 1 a 6.</summary>
    public required int Numero { get; init; }

    /// <summary>Teto da receita dos últimos doze meses para esta faixa.</summary>
    public required decimal AteRbt12 { get; init; }

    /// <summary>Alíquota nominal da tabela.</summary>
    public required decimal Aliquota { get; init; }

    /// <summary>Parcela a deduzir.</summary>
    public required decimal ParcelaADeduzir { get; init; }

    /// <summary>Como o valor se reparte entre os tributos, em percentual.</summary>
    public required IReadOnlyDictionary<Tributo, decimal> Reparticao { get; init; }

    /// <summary>Indica se a receita cabe nesta faixa.</summary>
    public bool Cabe(decimal rbt12) => rbt12 <= AteRbt12;

    /// <summary>Os tributos que esta faixa reparte.</summary>
    public IEnumerable<Tributo> TributosDaFaixa => Reparticao.Keys;

    /// <summary>Confere que a repartição soma cem por cento.</summary>
    public Faixa Validar()
    {
        var soma = Reparticao.Values.Sum();

        if (Math.Abs(soma - 100m) > 0.01m)
        {
            throw new InvalidOperationException(
                $"A repartição da faixa {Numero} soma {soma:0.00}%, deveria somar 100%.");
        }

        return this;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"faixa {Numero}: até {AteRbt12:N0}, {Aliquota:0.00}% − {ParcelaADeduzir:N2}";
}
