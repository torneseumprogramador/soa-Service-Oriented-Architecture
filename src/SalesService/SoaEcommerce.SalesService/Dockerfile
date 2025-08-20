FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SalesService/SoaEcommerce.SalesService/SoaEcommerce.SalesService.csproj", "SalesService/SoaEcommerce.SalesService/"]
COPY ["Contracts/SoaEcommerce.Contracts/SoaEcommerce.Contracts.csproj", "Contracts/SoaEcommerce.Contracts/"]
RUN dotnet restore "SalesService/SoaEcommerce.SalesService/SoaEcommerce.SalesService.csproj"
COPY . .
RUN dotnet build "SalesService/SoaEcommerce.SalesService/SoaEcommerce.SalesService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SalesService/SoaEcommerce.SalesService/SoaEcommerce.SalesService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoaEcommerce.SalesService.dll"]
