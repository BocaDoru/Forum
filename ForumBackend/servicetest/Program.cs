using servicetest;
using System.Reflection;
using System.Text.RegularExpressions;

Console.WriteLine("STARTING AUTOMATED E2E TESTS");
try
{
    string gatewayUrl = Environment.GetEnvironmentVariable("GatewayUrl") ?? "http://localhost:8080";
    var testRunner = new TestRunner(gatewayUrl);

    var testSuites = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && typeof(ITestFlow).IsAssignableFrom(t))
        .ToList();

    if (testSuites.Count == 0)
    {
        Console.WriteLine("\nNo test suites found. Skipping.");
    }
    else
    {
        Console.WriteLine("\nWaiting for API Gateway to become healthy...");
        using var pingClient = new HttpClient();
        bool isHealthy = false;

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var res = await pingClient.GetAsync(gatewayUrl);
                isHealthy = true;
                break;
            }
            catch
            {
                await Task.Delay(1500);
            }
        }

        if (!isHealthy)
        {
            throw new Exception("API Gateway never woke up. Aborting tests.");
        }

        Console.WriteLine("API Gateway is online. Starting test suites...");

        foreach (var suiteType in testSuites)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n>> Loading Test Suite: {suiteType.Name}");
            Console.ResetColor();

            var testInstance = (ITestFlow)Activator.CreateInstance(suiteType)!;
            await testInstance.RunAsync(testRunner);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nALL {testSuites.Count} TEST SUITES PASSED");
        Console.ResetColor();
    }
}
catch (Exception)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FATAL: End-to-End tests failed. Stopping execution.");
    Console.ResetColor();
    Environment.Exit(1);
}

bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (isDocker)
{
    Console.WriteLine("Running in automated Docker environment. Tests passed cleanly. Exiting container.");
    return;
}

Console.WriteLine("\nGRPC SERVICE MESSAGE CONSOLE REPL");

try
{
    GrpcRouter.InitializeFromJson("clients.json");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Startup Error: {ex.Message}");
    Console.ResetColor();
    return;
}

Console.WriteLine("Format: service.message({\"key1\": value1, \"key2\": value2})");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input)) 
        continue;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) 
        break;

    try
    {
        var commandRegex = new Regex(@"^([a-zA-Z0-9_]+)\.([a-zA-Z0-9_]+)\((.*)\)$");
        var match = commandRegex.Match(input);

        if (!match.Success)
        {
            Console.WriteLine("Invalid syntax. Expected: service.message({\"key1\": value1, \"key2\": value2})");
            continue;
        }

        string service = match.Groups[1].Value.ToLower();
        string message = match.Groups[2].Value;
        string jsonPayload = match.Groups[3].Value;

        await GrpcRouter.RouteCommandAsync(service, message, jsonPayload);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}