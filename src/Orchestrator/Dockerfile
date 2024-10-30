#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.35 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0.427-1 AS build
WORKDIR /src
COPY ["Orchestrator/Orchestrator.csproj", "Orchestrator/"]
COPY ["OrchestratorContracts/OrchestratorContracts.csproj", "OrchestratorContracts/"]
RUN dotnet restore "Orchestrator/Orchestrator.csproj"
COPY . .
WORKDIR "/src/Orchestrator"
RUN dotnet build "Orchestrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orchestrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orchestrator.dll"]
