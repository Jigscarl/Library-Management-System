# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY ["Library Management System.sln", "."]
COPY ["Library.Core/Library.Core.csproj", "Library.Core/"]
COPY ["Library.Data/Library.Data.csproj", "Library.Data/"]
COPY ["Library.Api/Library.Api.csproj", "Library.Api/"]

RUN dotnet restore "Library Management System.sln"

# Copy everything else and publish the API
COPY . .
WORKDIR "/src/Library.Api"
RUN dotnet publish "Library.Api.csproj" -c Release -o /app/publish

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render expects your app to listen on port 10000
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Library.Api.dll"]