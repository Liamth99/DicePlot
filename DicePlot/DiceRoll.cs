using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DicePlot;

public partial class DiceRoll
{
    [GeneratedRegex(@"(?<diceRolls>\d+)d(?<diceSize>\d+)(?<offset>(\+|-)\d+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex DiceRegex { get; }

    public int DiceRolls { get; init; }
    public int DiceSize  { get; init; }
    public int Offset    { get; init; }

    public DiceRoll(int diceRolls, int diceSize, int offset)
    {
        DiceRolls = diceRolls;
        DiceSize  = diceSize;
        Offset    = offset;
    }

    public static bool TryParseFromString(string input, [NotNullWhen(true)] out DiceRoll? diceRoll)
    {
        var match = DiceRegex.Match(input);

        if (!match.Success)
        {
            diceRoll = null;
            return false;
        }

        diceRoll = new DiceRoll(int.Parse(match.Groups["diceRolls"].Value), int.Parse(match.Groups["diceSize"].Value), match.Groups["offset"].Value is "" ? 0 : int.Parse(match.Groups["offset"].Value));
        return true;
    }

    public Dictionary<int, double> GenerateDistribution()
    {
        var distribution = new Dictionary<int, double>
        {
            [0] = 1D,
        };

        for (int roll = 0; roll < DiceRolls; roll++)
        {
            var next = new Dictionary<int, double>();

            foreach (var (result, prob) in distribution)
            {
                for (int side = 1; side <= DiceSize; side++)
                {
                    int newResult = result + side;

                    if (!next.TryAdd(newResult, prob))
                        next[newResult] += prob;
                }
            }

            distribution = next;
        }

        double totalPossibilities = Math.Pow(DiceSize, DiceRolls);

        foreach (var key in distribution.Keys)
        {
            distribution[key] /= totalPossibilities;
        }

        if (Offset is not 0)
        {
            distribution = distribution
               .ToDictionary(kvp => kvp.Key + Offset, kvp => kvp.Value);
        }

        return distribution;
    }
}