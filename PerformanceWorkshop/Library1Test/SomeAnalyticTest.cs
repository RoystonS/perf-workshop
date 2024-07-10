using System.Diagnostics;
using Library1;
using Library1.Entities;
using Xunit.Abstractions;

namespace Library1Test;

public class SomeAnalyticTest
{
    private readonly ITestOutputHelper _output;

    public SomeAnalyticTest(ITestOutputHelper output)
    {
        this._output = output;
    }

    [Fact]
    public void Analyse_TopCells_performance()
    {
        var analytic = new SomeAnalytic();

        var overallTime = Stopwatch.StartNew();

        var maxTimeToRun = TimeSpan.FromSeconds(10);
        for (var multiplier = 1.0; multiplier < 200 && overallTime.Elapsed < maxTimeToRun; multiplier *= 1.414)
        {
            var randomData = new RandomData(0);

            var cellCount = (int)(Math.Round(100 * multiplier));
            var eventCount = cellCount * 10;

            var (events, cells) = CreateSampleData(randomData, eventCount, cellCount);

            var stopWatch = Stopwatch.StartNew();
            analytic.ComputeTopCells(SomeAnalytic.CellsToDisplayEnum.All, events, cells);
            var elapsed = stopWatch.Elapsed;

            _output.WriteLine($"{cellCount}\t{elapsed.TotalMilliseconds}");
        }
    }

    //[Fact]
    public void GetTopCells_Performs_well()
    {
        var analytic = new SomeAnalytic();

        var randomData = new RandomData(0);

        var cellCount = 10000;
        var eventCount = cellCount * 10;

        var (events, cells) = CreateSampleData(randomData, eventCount, cellCount);

        var stopWatch = Stopwatch.StartNew();
        analytic.ComputeTopCells(SomeAnalytic.CellsToDisplayEnum.All, events, cells);
        var elapsed = stopWatch.Elapsed;

        _output.WriteLine($"{cellCount}\t{elapsed.TotalMilliseconds}");

        Assert.True(elapsed.TotalMilliseconds < 500);
    }

    private static (List<CaseEventDTO>, List<CaseLocationDTO>) CreateSampleData(RandomData randomData, int eventCount, int cellCount)
    {
        var cells = new List<CaseLocationDTO>(cellCount);
        var events = new List<CaseEventDTO>(eventCount);

        for (var i = 0; i < cellCount; i++)
        {
            var cell = new CaseLocationDTO(randomData.CreateCGI());
            // Put each CGI in twice to ensure the algorithm copes with duplicates.
            cells.Add(cell);
            cells.Add(cell);
        }

        for (var i = 0; i < eventCount; i++)
        {
            var ev = new CaseEventDTO();
            if (randomData.IsChance(0.99))
            {
                ev.StartCGI = randomData.CreateCGI();
            }
            if (randomData.IsChance(0.99))
            {
                ev.EndCGI = randomData.CreateCGI();
            }

            events.Add(new CaseEventDTO { StartCGI = randomData.GetRandom(cells).Cgi, EndCGI = randomData.GetRandom(cells).Cgi });
        }

        return (events, cells);
    }

    internal class RandomData
    {
        private readonly Random _random;

        public RandomData(int seed)
        {
            _random = new Random(seed);
        }

        public T GetRandom<T>(IReadOnlyList<T> items)
        {
            var index = _random.Next(items.Count);
            return items[index];
        }

        public string CreateCGI()
        {
            var mcc = 234;
            var mnc = _random.Next(10, 50);
            var lac = _random.Next(10000, 99999);
            var cellId = _random.Next(10000, 99999);

            return $"{mcc}-{mnc}-{lac}-{cellId}";
        }

        public bool IsChance(double chance)
        {
            return _random.NextDouble() < chance;
        }
    }
}
