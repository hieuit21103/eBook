# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["FileStorage/FileStorage.csproj", "FileStorage/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "FileStorage/FileStorage.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/FileStorage"
RUN dotnet build "FileStorage.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FileStorage.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5003
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.dll"]