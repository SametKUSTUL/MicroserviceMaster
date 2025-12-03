FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["IdentityService/Identity.API/Identity.API.csproj", "IdentityService/Identity.API/"]
COPY ["IdentityService/Identity.Application/Identity.Application.csproj", "IdentityService/Identity.Application/"]
COPY ["IdentityService/Identity.Domain/Identity.Domain.csproj", "IdentityService/Identity.Domain/"]
COPY ["IdentityService/Identity.Infrastructure/Identity.Infrastructure.csproj", "IdentityService/Identity.Infrastructure/"]
COPY ["Shared/Security/Security.csproj", "Shared/Security/"]
COPY ["Shared/Observability/Observability.csproj", "Shared/Observability/"]
COPY ["Shared/Middleware/Middleware.csproj", "Shared/Middleware/"]

RUN dotnet restore "IdentityService/Identity.API/Identity.API.csproj"

COPY . .
WORKDIR "/src/IdentityService/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
