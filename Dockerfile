# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AslanEtsy.Domain/AslanEtsy.Domain.csproj", "AslanEtsy.Domain/"]
COPY ["AslanEtsy.Application/AslanEtsy.Application.csproj", "AslanEtsy.Application/"]
COPY ["AslanEtsy.Infrastructure/AslanEtsy.Infrastructure.csproj", "AslanEtsy.Infrastructure/"]
COPY ["AslanEtsy.WebApi/AslanEtsy.WebApi.csproj", "AslanEtsy.WebApi/"]

# Restore dependencies
RUN dotnet restore "AslanEtsy.WebApi/AslanEtsy.WebApi.csproj"

# Copy full source
COPY . .

# Publish release
WORKDIR "/src/AslanEtsy.WebApi"
RUN dotnet publish "AslanEtsy.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Configure environment & port
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AslanEtsy.WebApi.dll"]
