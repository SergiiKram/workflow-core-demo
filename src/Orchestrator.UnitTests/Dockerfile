#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0.428-1 AS build
WORKDIR /src
COPY ["Orchestrator.UnitTests/Orchestrator.UnitTests.csproj", "Orchestrator.UnitTests/"]
COPY ["Orchestrator/Orchestrator.csproj", "Orchestrator/"]
COPY ["OrchestratorContracts/OrchestratorContracts.csproj", "OrchestratorContracts/"]
RUN dotnet restore "Orchestrator.UnitTests/Orchestrator.UnitTests.csproj"
COPY . .
WORKDIR "/src/Orchestrator.UnitTests"
RUN dotnet build "Orchestrator.UnitTests.csproj" -c Release --no-restore
ENTRYPOINT ["dotnet", "test", "--configuration", "Release", "--no-build", "--logger:trx"]