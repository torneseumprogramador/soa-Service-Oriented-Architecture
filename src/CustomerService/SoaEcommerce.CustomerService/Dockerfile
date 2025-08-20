FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SoaEcommerce.CustomerService.csproj", "./"]
COPY ["../../Contracts/SoaEcommerce.Contracts/SoaEcommerce.Contracts.csproj", "../Contracts/SoaEcommerce.Contracts/"]
RUN dotnet restore "SoaEcommerce.CustomerService.csproj"
COPY . .
RUN dotnet build "SoaEcommerce.CustomerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoaEcommerce.CustomerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoaEcommerce.CustomerService.dll"]
