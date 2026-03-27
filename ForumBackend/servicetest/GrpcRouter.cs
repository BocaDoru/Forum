using commons.Protos;
using Grpc.Net.Client;
using System.Reflection;
using System.Text.Json;

namespace servicetest;

public class GrpcClientConfig
{
    public string Url { get; set; } = string.Empty;
    public string ClientClass { get; set; } = string.Empty;
}

public static class GrpcRouter
{
    private static readonly Dictionary<string, object> Clients = new(StringComparer.OrdinalIgnoreCase);

    public static void InitializeFromJson(string filePath)
    {
        Console.WriteLine($"\n[Init] Loading clients from {filePath}...");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file {filePath} not found.");

        string json = File.ReadAllText(filePath);
        var configMap = JsonSerializer.Deserialize<Dictionary<string, GrpcClientConfig>>(json);

        if (configMap == null) return;

        var executingAssembly = Assembly.GetExecutingAssembly();
        var allTypes = executingAssembly.GetTypes();

        foreach (var kvp in configMap)
        {
            string consoleCommandPrefix = kvp.Key;
            GrpcClientConfig config = kvp.Value;

            Type? clientType = allTypes.FirstOrDefault(t =>
                t.Name.Equals(config.ClientClass, StringComparison.OrdinalIgnoreCase) ||
                (
                    t.FullName is not null &&
                    t.FullName.Replace("+", ".").EndsWith(config.ClientClass, StringComparison.OrdinalIgnoreCase)
                )
            );

            if (clientType is null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Warning] Could not find a C# class named '{config.ClientClass}'. Skipping '{consoleCommandPrefix}'.");
                Console.ResetColor();
                continue;
            }

            var channel = GrpcChannel.ForAddress(config.Url);

            object? clientInstance = Activator.CreateInstance(clientType, channel);

            if (clientInstance is not null)
            {
                Clients[consoleCommandPrefix] = clientInstance;
                Console.WriteLine($"[Init] Loaded '{consoleCommandPrefix}' -> {config.Url} ({clientType.Name})");
            }
        }

        Console.WriteLine();
    }

    public static async Task RouteCommandAsync(string serviceName, string messageName, string jsonPayload)
    {
        try
        {
            if (!Clients.TryGetValue(serviceName, out var client))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Error] Service '{serviceName}' not found.");
                Console.ResetColor();
                return;
            }

            Type clientType = client.GetType();

            MethodInfo? method = clientType.GetMethods(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => 
                m.Name.Equals(messageName + "Async", StringComparison.OrdinalIgnoreCase) && 
                m.GetParameters().Length == 2)
                ?? 
                clientType.GetMethods(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => 
                m.Name.Equals(messageName, StringComparison.OrdinalIgnoreCase) && 
                m.GetParameters().Length == 2);

            if (method is null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Error] Message '{messageName}' not found.");
                Console.ResetColor();
                return;
            }

            ParameterInfo[] methodParams = method.GetParameters();
            Type requestType = methodParams[0].ParameterType;
            Type callOptionsType = methodParams[1].ParameterType;

            object? parser = requestType.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            object? grpcRequestObj = parser?.GetType()
                .GetMethod("ParseJson")?
                .Invoke(parser,
                [
                    jsonPayload
                ]);

            if (grpcRequestObj is null) 
                throw new Exception("Failed to build gRPC request from the provided JSON.");

            object?[] invokeArgs =
            [
                grpcRequestObj,
                Activator.CreateInstance(callOptionsType),
            ];

            object? callObject = method.Invoke(client, invokeArgs);
            if (callObject is null) 
                throw new Exception("gRPC call returned null.");

            PropertyInfo? responseAsyncProp = callObject.GetType().GetProperty("ResponseAsync");
            Task responseTask = (Task)responseAsyncProp!.GetValue(callObject)!;
            await responseTask;

            PropertyInfo resultProp = responseTask.GetType().GetProperty("Result")!;
            object rawResult = resultProp.GetValue(responseTask)!;

            if (rawResult is MessageResponse myResponse)
            {
                Console.ForegroundColor = myResponse.Success ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"Status: {(myResponse.Success ? "SUCCESS" : "FAIL")} | Code: {myResponse.Code}");
                Console.ResetColor();

                if (!string.IsNullOrEmpty(myResponse.Errors))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Errors: {myResponse.Errors}");
                    Console.ResetColor();
                }

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string prettyJson = JsonSerializer.Serialize(myResponse.Payload.Json, jsonOptions);

                Console.WriteLine($"Payload:\n{prettyJson}\n");
            }
        }
        catch (TargetInvocationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[gRPC Error] {ex.InnerException?.Message ?? ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[System Error] {ex.Message}");
            Console.ResetColor();
        }
    }
}