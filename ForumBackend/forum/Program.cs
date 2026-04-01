using commons.MessageBase;
using database;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using forum;
using forum.Implementations;
using usersServiceClient;

var builder = WebApplication.CreateBuilder(args);

Console.Clear();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
});

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddGrpc();

builder.Services.AddSingleton<MessageDispatcher>();
builder.Services.AddMessageHandlers();

builder.Services.AddSingleton<IQuestionsService, QuestionsServiceImplementation>();

builder.Services.AddSingleton<ICommentsService, CommentsServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<QuestionsService>();
app.MapGrpcService<CommentsService>();

app.Run();