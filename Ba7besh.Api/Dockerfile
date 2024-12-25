FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ba7besh.Api/Ba7besh.Api.csproj", "Ba7besh.Api/"]
COPY ["Ba7besh.Application/Ba7besh.Application.csproj", "Ba7besh.Application/"]
COPY ["Ba7besh.Infrastructure/Ba7besh.Infrastructure.csproj", "Ba7besh.Infrastructure/"]
RUN dotnet restore "Ba7besh.Api/Ba7besh.Api.csproj"
COPY . .
WORKDIR "/src/Ba7besh.Api"
RUN dotnet build "Ba7besh.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ba7besh.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ba7besh.Api.dll"]