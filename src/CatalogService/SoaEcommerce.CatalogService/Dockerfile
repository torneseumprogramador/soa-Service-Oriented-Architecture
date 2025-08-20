FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SoaEcommerce.CatalogService.csproj", "./"]
COPY ["../../Contracts/SoaEcommerce.Contracts/SoaEcommerce.Contracts.csproj", "../Contracts/SoaEcommerce.Contracts/"]
RUN dotnet restore "SoaEcommerce.CatalogService.csproj"
COPY . .
RUN dotnet build "SoaEcommerce.CatalogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoaEcommerce.CatalogService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoaEcommerce.CatalogService.dll"]
