# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project definitions and restore
COPY ["AslanEtsy.Domain/AslanEtsy.Domain.csproj", "AslanEtsy.Domain/"]
COPY ["AslanEtsy.Application/AslanEtsy.Application.csproj", "AslanEtsy.Application/"]
COPY ["AslanEtsy.Infrastructure/AslanEtsy.Infrastructure.csproj", "AslanEtsy.Infrastructure/"]
COPY ["AslanEtsy.WebApi/AslanEtsy.WebApi.csproj", "AslanEtsy.WebApi/"]

RUN dotnet restore "AslanEtsy.WebApi/AslanEtsy.WebApi.csproj"

# Copy all source files
COPY . .

# Publish
WORKDIR "/src/AslanEtsy.WebApi"
RUN dotnet publish "AslanEtsy.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AslanEtsy.WebApi.dll"]
