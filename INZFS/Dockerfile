# Set the base image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Add the SDK so you can run the dotnet restore and build commands
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ./ ./
RUN dotnet restore "INZFS/INZFS.Web.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "INZFS/INZFS.Web.csproj" -c Release -o /app/build

# Create the publish files
FROM build AS publish
RUN dotnet publish "INZFS/INZFS.Web.csproj" -c Release -o /app/publish

# Copy the publish files into the container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "INZFS.Web.dll"]