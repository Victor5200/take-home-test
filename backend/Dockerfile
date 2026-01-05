# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj", "Fundo.Applications.WebApi/"]
RUN dotnet restore "Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj"

# Copy the rest of the code and build
COPY src/ .
WORKDIR "/src/Fundo.Applications.WebApi"
RUN dotnet build "Fundo.Applications.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Fundo.Applications.WebApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
EXPOSE 5000

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]
