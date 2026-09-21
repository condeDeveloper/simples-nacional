using Simples.Core.Anexos;

namespace Simples.Core.Calculo;

/// <summary>O que se precisa saber para apurar o DAS de um mês.</summary>
public sealed record PedidoDeApuracao
{
    /// <summary>Anexo da atividade. Nos anexos III e V o fator R pode trocá-lo.</summary>
    public required Anexo Anexo { get; init; }

    /// <summary>Receita bruta dos últimos doze meses.</summary>
    public required decimal Rbt12 { get; init; }

    /// <summary>Receita do mês que está sendo apurado.</summary>
    public required decimal ReceitaDoMes { get; init; }

    /// <summary>Folha de pagamento dos últimos doze meses, base do fator R.</summary>
    public decimal FolhaDe12Meses { get; init; }

    /// <summary>Competência apurada.</summary>
    public DateOnly Competencia { get; init; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Monta o pedido a partir do histórico de receitas mensais.</summary>
    public static PedidoDeApuracao DeHistorico(
        Anexo anexo,
        IReadOnlyCollection<decimal> receitasMensais,
        decimal receitaDoMes,
        decimal folhaDe12Meses = 0) => new()
        {
            Anexo = anexo,
            Rbt12 = Limites.Rbt12Proporcional(receitasMensais),
            ReceitaDoMes = receitaDoMes,
            FolhaDe12Meses = folhaDe12Meses,
        };

    /// <summary>Confere os valores e devolve o próprio pedido.</summary>
    public PedidoDeApuracao Validar()
    {
        if (Rbt12 < 0)
            throw new ArgumentOutOfRangeException(nameof(Rbt12), "A receita dos doze meses não pode ser negativa.");

        if (ReceitaDoMes < 0)
            throw new ArgumentOutOfRangeException(nameof(ReceitaDoMes), "A receita do mês não pode ser negativa.");

        if (FolhaDe12Meses < 0)
            throw new ArgumentOutOfRangeException(nameof(FolhaDe12Meses), "A folha não pode ser negativa.");

        if (Limites.PassouDoTeto(Rbt12))
            throw new InvalidOperationException(
                $"Receita de {Rbt12:N2} passa do teto de {Limites.TetoAnual:N2}: a empresa está fora do Simples.");

        return this;
    }
}
