FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY images/images.csproj ./images/

RUN dotnet restore images/images.csproj

COPY . .

WORKDIR "/src/images"
RUN dotnet publish "images.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "images.dll"]