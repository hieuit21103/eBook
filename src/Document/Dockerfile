# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Document/Document.csproj", "Document/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Document/Document.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Document"
RUN dotnet build "Document.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Document.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5002
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Document.dll"]