# Build context is backend/ (needs the whole .NET solution to restore/publish).
# Build with (from repo root): docker build -f backend/docker/api.Dockerfile -t taskflow-api backend/

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore TaskFlow.slnx
RUN dotnet publish src/TaskFlow.Api/TaskFlow.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
