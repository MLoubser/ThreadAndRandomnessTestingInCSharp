using System.Diagnostics;

const int totalIterations = 10_000_000;     //Iterations per thread
const int numberOfThreads = 5;              //Number of threads
const int minValue = 1;                     //RNG Min number (incl)
const int maxValue = 6;                     //RNG Max number (incl)

Mutex mutex = new(false);


long[] intsInitOnce = new long[maxValue];
long[] intsFreshInit = new long[maxValue];

Thread[] threads = new Thread[numberOfThreads];

Stopwatch stopwatch = new();
stopwatch.Start();

for (int k = 0; k < numberOfThreads; k++)
{
    threads[k] = new Thread(new ParameterizedThreadStart((threadId) => {

        int? idPrivate = threadId as int?;

        int[] intsInitOncePrivate = new int[maxValue];
        int[] intsFreshInitPrivate = new int[maxValue];

        var rgen = new RandomGenerator()
        {
            MinValue = minValue,
            MaxValue = maxValue
        };

        WriteLine($"Thread {idPrivate} starting");

        for (int i = 0; i < totalIterations; i++)
        {
            var rInitOnce = rgen.GenerateRandomFromInitializedGenerator();
            var rFreshInit = rgen.GenerateRandomFromFreshInitGenerator();

            intsInitOncePrivate[rInitOnce - 1] += 1;
            intsFreshInitPrivate[rFreshInit - 1] += 1;
        }

        WriteLine($"Thread {idPrivate} complete, Waiting for mutex");

        mutex.WaitOne();
        WriteLine($"Thread {idPrivate} Has Mutex");
        for (int i = 0; i < intsInitOncePrivate.Length; i++)
        {
            intsInitOnce[i] += intsInitOncePrivate[i];
            intsFreshInit[i] += intsFreshInitPrivate[i];
        }
        mutex.ReleaseMutex();
        WriteLine($"Thread {idPrivate} Released Mutex");
    }));

    threads[k].Start(k + 1);
}



for (int k = 0; k < numberOfThreads; k++)
{
    threads[k].Join();
}
Console.WriteLine();
WriteLine("All threads complete");
Console.WriteLine();

stopwatch.Stop();

WriteLine("FINAL RESULTS:");
Console.WriteLine();
WriteLine("Init Once:");
Console.WriteLine();

decimal? maxVariancePercentage = null;
decimal? minVariancePercentage = null;

long totalIterationsOverThreads = (long)totalIterations * numberOfThreads;

for (int i = 0; i < intsInitOnce.Length; i++)
{
    decimal percentage = ((decimal)intsInitOnce[i] / totalIterationsOverThreads) * 100m;

    if (minVariancePercentage is null || percentage < minVariancePercentage)
    {
        minVariancePercentage = percentage;
    }
    if (maxVariancePercentage is null || percentage > maxVariancePercentage)
    {
        maxVariancePercentage = percentage;
    }

    WriteLine($"Value : {i + 1}, Count : {intsInitOnce[i]}, Percentage : {Math.Round(percentage, 2)} %");
}

Console.WriteLine();
WriteLine($"Max Percentage Delta: {maxVariancePercentage - minVariancePercentage} %");

Console.WriteLine();
WriteLine("Init Every Time:");
Console.WriteLine();

maxVariancePercentage = null;
minVariancePercentage = null;

for (int i = 0; i < intsFreshInit.Length; i++)
{
    decimal percentage = ((decimal)intsFreshInit[i] / totalIterationsOverThreads) * 100m;

    if (minVariancePercentage is null || percentage < minVariancePercentage)
    {
        minVariancePercentage = percentage;
    }
    if (maxVariancePercentage is null || percentage > maxVariancePercentage)
    {
        maxVariancePercentage = percentage;
    }

    WriteLine($"Value : {i + 1}, Count : {intsFreshInit[i]}, Percentage : {Math.Round(percentage, 2)} %");
}

Console.WriteLine();
WriteLine($"Max Percentage Delta: {maxVariancePercentage - minVariancePercentage} %");
Console.WriteLine();
WriteLine($"Total execution time: {GetElapsedTimeInString(stopwatch.ElapsedMilliseconds)}");

Console.ReadLine();


string GetElapsedTimeInString(long miliseconds)
{
    int MILI_IN_SECOND = 1000;
    int MILI_IN_MINUTE = 60 * MILI_IN_SECOND;

    if (miliseconds < MILI_IN_SECOND)
    {
        return $"{miliseconds}ms";
    }
    else if (miliseconds < MILI_IN_MINUTE)
    {
        return $"{Math.Round((decimal)miliseconds / MILI_IN_SECOND, 2)} seconds";
    }
    else
    {
        return $"{Math.Round((decimal)miliseconds / MILI_IN_MINUTE, 2)} minutes";
    }
}

void WriteLine(string line)
{
    Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} - {line}");
}


class RandomGenerator
{
    private readonly Random random = new();
    public int MinValue { get; init; } = 1;
    public int MaxValue { get; init; } = 10;

    public int GenerateRandomFromInitializedGenerator() => random.Next(MinValue, MaxValue + 1);

    public int GenerateRandomFromFreshInitGenerator() => (new Random()).Next(MinValue, MaxValue + 1);
}
