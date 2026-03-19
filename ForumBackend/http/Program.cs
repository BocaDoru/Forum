using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using usersServiceClient;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using http.Auth;
using FastEndpoints;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

Console.Clear();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = ForumAuthentication.SchemeName;
    options.DefaultChallengeScheme = ForumAuthentication.SchemeName;
}).AddScheme<AuthenticationSchemeOptions, ForumAuthentication>(
    ForumAuthentication.SchemeName,
    option => { }
);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(ForumPolicy.AuthenticatedUser, ForumPolicy.AuthenticatedUserPolicy)
    .AddPolicy(ForumPolicy.UnverifiedUser, ForumPolicy.UnverifiedUserPolicy);

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Serializer.Options.PropertyNamingPolicy = null;
});

app.Run();
