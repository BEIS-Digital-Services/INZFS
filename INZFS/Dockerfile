# Set the base image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

# Add the SDK so you can run the dotnet restore and build commands
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ./ ./
RUN dotnet restore "INZFS/INZFS.Web.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "INZFS/INZFS.Web.csproj" -c Release -o /app/build
FROM build as test
LABEL unittestlayer=true
LABEL test=true
# install the report generator tool
#RUN dotnet add package coverlet.collector
#RUN dotnet add package coverlet.msbuild
#RUN dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.8.10
RUN dotnet tool install -g dotnet-reportgenerator-globaltool
RUN dotnet tool install dotnet-reportgenerator-globaltool --tool-path tools
RUN dotnet new tool-manifest
RUN dotnet tool install dotnet-reportgenerator-globaltool
# run the test and collect code coverage (requires coverlet.msbuild to be added to test project)
# for exclude, use %2c for ,
RUN dotnet test "INZFS.UnitTests/INZFS.UnitTests.csproj" --logger "trx;LogFileName=/tests/INZFS.trx" --logger "xunit;LogFileName=/tests/coverage.cobertura.xml" --results-directory /tests /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=/tests /p:Exclude="[xunit.*]*%2c[StackExchange.*]*"
# generate html reports using report generator tool
#RUN dotnet tool list -g
#RUN ls -la tools
RUN tools/reportgenerator -reports:/tests/coverage.cobertura.xml -targetdir:/tests -reporttypes:"HTMLInline;HTMLChart"
RUN tools/reportgenerator -reports:/tests/coverage.cobertura.xml -targetdir:/tests -reporttypes:"Cobertura"
RUN ls -la /tests/*

# Create the publish files
FROM build AS publish
RUN dotnet publish "INZFS/INZFS.Web.csproj" -c Release -o /app/publish

# Copy the publish files into the container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "INZFS.Web.dll"]
