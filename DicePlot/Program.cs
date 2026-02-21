using DicePlot;
using Spectre.Console;

if (args.Length is not 1)
{
    var ex = new ArgumentException("Expected 1 argument in the form of xdx+x");
    AnsiConsole.WriteException(ex);
    return 1;
}

if (!DiceRoll.TryParseFromString(args[0], out var roll))
{
    var ex = new ArgumentException($"{args[0]} is not a valid dice roll. Input must match xdx+x");
    AnsiConsole.WriteException(ex);
    return 1;
}

Dictionary<int, double> distribution = roll.GenerateDistribution();

if (distribution.Count is 0)
{
    AnsiConsole.MarkupLine($"[red]No distribution data available for {args[0]}.[/]");
    return 1;
}

double max = distribution.Values.Max();

double q1 = max / 4;
double q3 = q1 * 3;

var chart = new BarChart()
           .Label(args[0])
           .AddItems(distribution, pair => new BarChartItem(pair.Key.ToString("N0"), pair.Value, pair.Value < q1 ? Color.Red : pair.Value < q3 ? Color.Yellow : Math.Abs(pair.Value - max) < double.Epsilon ? Color.DarkGreen : Color.Green))
           .UseValueFormatter((value, culture) =>
                              {
                                  double percent = value * 100;

                                  return percent switch
                                  {
                                      >= 1    => percent.ToString("0.##", culture) + "%",
                                      >= 0.01 => percent.ToString("0.####", culture) + " %",
                                      _       => percent.ToString("E", culture) + "%"
                                  };
                              });

AnsiConsole.Write(chart);
return 0;