#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["E2eTests/E2eTests.csproj", "E2eTests/"]
RUN dotnet restore "E2eTests/E2eTests.csproj"
COPY . .
WORKDIR "/src/E2eTests"
RUN dotnet build "E2eTests.csproj" --configuration Release --no-restore
ENTRYPOINT ["dotnet", "test", "--configuration", "Release", "--no-build", "--logger:trx"]