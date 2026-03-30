using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using database;
using commons.MessageBase;
using images.Implementation;
using images;

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

builder.Services.AddSingleton<ImageServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<ImagesService>();

app.Run();