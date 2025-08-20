FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CustomerService/SoaEcommerce.CustomerService/SoaEcommerce.CustomerService.csproj", "CustomerService/SoaEcommerce.CustomerService/"]
COPY ["Contracts/SoaEcommerce.Contracts/SoaEcommerce.Contracts.csproj", "Contracts/SoaEcommerce.Contracts/"]
RUN dotnet restore "CustomerService/SoaEcommerce.CustomerService/SoaEcommerce.CustomerService.csproj"
COPY . .
RUN dotnet build "CustomerService/SoaEcommerce.CustomerService/SoaEcommerce.CustomerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerService/SoaEcommerce.CustomerService/SoaEcommerce.CustomerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoaEcommerce.CustomerService.dll"]
