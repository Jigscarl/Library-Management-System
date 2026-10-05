FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["LibraryManagement.sln", "."]
COPY ["Library.Api/Library.Api.csproj", "Library.Api/"]
COPY ["Library.Data/Library.Data.csproj", "Library.Data/"]
COPY ["Library.Core/Library.Core.csproj", "Library.Core/"]
RUN dotnet restore "LibraryManagement.sln"
COPY . .
WORKDIR "/src/Library.Api"
RUN dotnet publish "Library.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=10000
ENTRYPOINT ["dotnet", "Library.Api.dll"]