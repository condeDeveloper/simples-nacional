# simples-nacional

Calculadora do DAS em C# e .NET 8: os cinco anexos da LC 123/2006, alíquota
efetiva com parcela a deduzir, fator R, sublimite de ICMS e ISS, e a repartição
do imposto entre União, estado e município.

```csharp
var apuracao = new Calculadora().Apurar(new PedidoDeApuracao
{
    Anexo = Anexo.V,            // serviço intelectual
    Rbt12 = 300_000m,
    ReceitaDoMes = 25_000m,
    FolhaDe12Meses = 90_000m,   // fator R de 30%
});

Console.WriteLine(apuracao.Detalhar());
```

```
Competência 09/2026 — Anexo III (serviços em geral)
  RBT12 300.000,00 — faixa 2: até 360.000, 11,20% − 9.360,00
  receita do mês 25.000,00
  alíquota efetiva 8,0800%
  DAS 2.020,00
    IRPJ: 80,80 (4,00%, união)
    CSLL: 70,70 (3,50%, união)
    COFINS: 283,81 (14,05%, união)
    PIS/Pasep: 61,61 (3,05%, união)
    CPP: 876,68 (43,40%, união)
    ISS: 646,40 (32,00%, município)
  * O fator R de 30,00% alcançou os 28%: a apuração usa o Anexo III.
```

## Por que existe

Três erros aparecem em quase toda planilha de Simples que eu já vi:

**1. Usar a alíquota nominal da tabela.** Ela não é a que se paga. A conta é
`(RBT12 × alíquota − parcela a deduzir) ÷ RBT12`, e a dedução existe justamente
para que virar de faixa não dê salto — sem ela, faturar um real a mais
aumentaria o imposto de forma absurda. Na segunda faixa do Anexo III, aplicar a
nominal direto cobra **11,20%** onde o correto é **8,08%**: 39% a mais.

**2. Ignorar o fator R.** Se a folha dos últimos doze meses chega a 28% da
receita, um serviço intelectual sai do Anexo V e vai para o III. Na primeira
faixa isso é 15,5% contra 6%. Com RBT12 de 300 mil e 25 mil de receita no mês,
a diferença é de **R$ 2.105 por mês**.

**3. Esquecer que o DAS é uma guia só, mas não um imposto só.** O dinheiro se
reparte entre oito tributos em percentuais que mudam de faixa para faixa, e
acima do sublimite de R$ 3,6 milhões o ICMS e o ISS **saem** do DAS.

## O que ele calcula

| | |
| --- | --- |
| **Alíquota efetiva** | com parcela a deduzir, por faixa e anexo |
| **DAS** | receita do mês × alíquota efetiva |
| **Repartição** | valor de cada um dos oito tributos, com o ente que recebe |
| **Fator R** | decide entre Anexo III e V, e diz quanto falta de folha para virar |
| **Sublimite** | acima de R$ 3,6 mi, ICMS e ISS saem do DAS |
| **Anexo IV** | avisa que a CPP fica fora do DAS e vai em GPS |
| **Início de atividade** | RBT12 anualizado pela média dos meses já corridos |
| **Porte** | MEI, microempresa ou EPP pela receita |

A soma das parcelas fecha **exatamente** com o DAS: arredondar cada tributo
separadamente sobra um ou dois centavos, e a diferença é jogada na maior
parcela, que é o critério usual.

## Comparando os anexos

```csharp
calculadora.CompararFatorR(rbt12: 300_000m, receitaDoMes: 25_000m);
// { Anexo.III = 2.020,00 ; Anexo.V = 4.125,00 }

FatorR.FolhaQueFalta(folhaDe12Meses: 60_000m, rbt12: 300_000m);
// 24.000,00 — o que falta para alcançar os 28%
```

É a conta que todo escritório refaz em dezembro discutindo pró-labore.

## API

```bash
dotnet run --project src/Simples.Api
```

| Método | Rota | O que faz |
| --- | --- | --- |
| `GET` | `/saude` | verificação de disponibilidade |
| `GET` | `/anexos` | os cinco anexos e suas características |
| `GET` | `/anexos/{anexo}/faixas` | a tabela completa de um anexo |
| `GET` | `/limites` | tetos, sublimite e o limite do fator R |
| `GET` | `/fator-r?folha=&rbt12=` | o fator, o anexo resultante e quanto falta |
| `POST` | `/apuracoes` | apura o DAS de uma competência |
| `GET` | `/comparacoes?rbt12=&receitaDoMes=` | Anexo III contra Anexo V |

## Estrutura

```
Tributos/Tributo.cs         os oito tributos e quem recebe cada um
Anexos/Faixa.cs             uma linha da tabela, com a repartição
Anexos/Anexo.cs             os cinco anexos e suas regras
Anexos/Tabelas.cs           as tabelas da LC 123/2006
Calculo/Limites.cs          tetos, sublimite e RBT12 proporcional
Calculo/FatorR.cs           a regra dos 28%
Calculo/PedidoDeApuracao.cs a entrada
Calculo/Apuracao.cs         o resultado, aberto por tributo
Calculo/Calculadora.cs      a apuração
```

As tabelas se autovalidam na carga do tipo: se a repartição de qualquer faixa
não somar 100%, o programa nem sobe.

## Rodando

```bash
dotnet test
```

110 testes. O núcleo não depende de nada além da BCL.

## ⚠️ Aviso

As tabelas são as da **Lei Complementar 123/2006 com a redação da LC
155/2016**, em vigor desde 2018. Legislação tributária muda: **confira os
valores contra a norma vigente antes de usar isto para recolher imposto de
verdade.** Isto é um projeto de estudo, não um substituto de contador.

## Limites conhecidos

- Sem segregação de receitas: a apuração trata um anexo por vez, e empresa com
  atividades mistas precisa de uma apuração por atividade.
- Sem retenção de ISS na fonte, sem substituição tributária de ICMS e sem
  imunidade ou isenção por produto.
- Sem sublimites estaduais diferenciados — só o nacional de R$ 3,6 milhões.
- Sem cálculo do excesso de receita nem da exclusão por ultrapassar o teto.
- Sem tabela do MEI: o limite de R$ 81 mil aparece só na classificação do porte.

## Licença

MIT.
