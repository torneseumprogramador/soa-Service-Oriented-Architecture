FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CatalogService/SoaEcommerce.CatalogService/SoaEcommerce.CatalogService.csproj", "CatalogService/SoaEcommerce.CatalogService/"]
COPY ["Contracts/SoaEcommerce.Contracts/SoaEcommerce.Contracts.csproj", "Contracts/SoaEcommerce.Contracts/"]
RUN dotnet restore "CatalogService/SoaEcommerce.CatalogService/SoaEcommerce.CatalogService.csproj"
COPY . .
RUN dotnet build "CatalogService/SoaEcommerce.CatalogService/SoaEcommerce.CatalogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogService/SoaEcommerce.CatalogService/SoaEcommerce.CatalogService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoaEcommerce.CatalogService.dll"]
