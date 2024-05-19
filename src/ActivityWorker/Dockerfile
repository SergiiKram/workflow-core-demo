#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.30 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0.422 AS build
WORKDIR /src
COPY ["ActivityWorker/ActivityWorker.csproj", "ActivityWorker/"]
COPY ["OrchestratorContracts/OrchestratorContracts.csproj", "OrchestratorContracts/"]
RUN dotnet restore "ActivityWorker/ActivityWorker.csproj"
COPY . .
WORKDIR "/src/ActivityWorker"
RUN dotnet build "ActivityWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ActivityWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ActivityWorker.dll"]
