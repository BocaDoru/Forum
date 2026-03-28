using votes;
using votes.Implementations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using database;
using System.Security.AccessControl;
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
string databaseName = builder.Configuration["MongoDB:DatabaseName"];

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddGrpc();

builder.Services.AddSingleton<MessageDispatcher>();
builder.Services.AddMessageHandlers();

builder.Services.AddSingleton<IVotesService, VotesServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<VotesGrpcService>();

app.Run();
