FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SoaEcommerce.CompositionService.csproj", "./"]
COPY ["../../Contracts/SoaEcommerce.Contracts/SoaEcommerce.Contracts.csproj", "../Contracts/SoaEcommerce.Contracts/"]
COPY ["../../Clients/SoaEcommerce.Clients/SoaEcommerce.Clients.csproj", "../Clients/SoaEcommerce.Clients/"]
RUN dotnet restore "SoaEcommerce.CompositionService.csproj"
COPY . .
RUN dotnet build "SoaEcommerce.CompositionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoaEcommerce.CompositionService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoaEcommerce.CompositionService.dll"]
