# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Forge.Api/Forge.Api.csproj", "Forge.Api/"]
COPY ["Forge.Application/Forge.Application.csproj", "Forge.Application/"]
COPY ["Forge.Domain/Forge.Domain.csproj", "Forge.Domain/"]
COPY ["Forge.Infrastructure/Forge.Infrastructure.csproj", "Forge.Infrastructure/"]
RUN dotnet restore "Forge.Api/Forge.Api.csproj"

COPY . .
WORKDIR /src/Forge.Api
RUN dotnet publish -c Release -o /app/publish

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Forge.Api.dll"]