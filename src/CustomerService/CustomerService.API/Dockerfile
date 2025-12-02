FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Shared/Observability/Observability.csproj", "Shared/Observability/"]
COPY ["Shared/Middleware/Middleware.csproj", "Shared/Middleware/"]
COPY ["Shared/Security/Security.csproj", "Shared/Security/"]
COPY ["CustomerService/CustomerService.API/CustomerService.API.csproj", "CustomerService/CustomerService.API/"]
COPY ["CustomerService/CustomerService.Application/CustomerService.Application.csproj", "CustomerService/CustomerService.Application/"]
COPY ["CustomerService/CustomerService.Domain/CustomerService.Domain.csproj", "CustomerService/CustomerService.Domain/"]
COPY ["CustomerService/CustomerService.Infrastructure/CustomerService.Infrastructure.csproj", "CustomerService/CustomerService.Infrastructure/"]
RUN dotnet restore "CustomerService/CustomerService.API/CustomerService.API.csproj"
COPY . .
WORKDIR "/src/CustomerService/CustomerService.API"
RUN dotnet build "CustomerService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerService.API.dll"]
