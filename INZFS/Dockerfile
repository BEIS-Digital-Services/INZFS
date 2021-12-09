# Set the base image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 50505

# Add the SDK so you can run the dotnet restore and build commands
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ./ ./
RUN dotnet restore "INZFS/INZFS.Web.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "INZFS/INZFS.Web.csproj" -c Release -o /app/build

ENV EXPORTER_ROOT=/aspnet-exporter
WORKDIR ${EXPORTER_ROOT}
COPY --from=dockersamples/aspnet-monitoring-exporter:4.7.2 \aspnet-exporter .
ENV COLLECTOR_CONFIG_PATH="w3svc-collectors.json"

ENTRYPOINT ["powershell"]
CMD Start-Service W3SVC; `
    Invoke-WebRequest http://localhost -UseBasicParsing | Out-Null; `
    Start-Process -NoNewWindow -WorkingDirectory $env:EXPORTER_ROOT "$env:EXPORTER_ROOT\aspnet-exporter.exe"; `
    C:\ServiceMonitor.exe w3svc

# Create the publish files
FROM build AS publish
RUN dotnet publish "INZFS/INZFS.Web.csproj" -c Release -o /app/publish

# Copy the publish files into the container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "INZFS.Web.dll"]