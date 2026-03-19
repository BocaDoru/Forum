using Microsoft.AspNetCore.Builder;
using users;
using users.Implementations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using database;
using Microsoft.Extensions.DependencyInjection;
using commons.MessageBase;

var builder = WebApplication.CreateBuilder(args);

Console.Clear();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddGrpc();

builder.Services.AddSingleton<MessageDispatcher>();
builder.Services.AddMessageHandlers();

builder.Services.AddSingleton<IUsersService, UsersServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<UsersService>();

app.Run();