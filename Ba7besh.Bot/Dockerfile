FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Ba7besh.Bot/*.csproj ./Ba7besh.Bot/
COPY Ba7besh.Application/Ba7besh.Application.csproj ./Ba7besh.Application/
COPY Ba7besh.sln .
RUN dotnet restore Ba7besh.Bot/Ba7besh.Bot.csproj

# Copy everything else and build
COPY Ba7besh.Bot/. ./Ba7besh.Bot/
COPY Ba7besh.Application/. ./Ba7besh.Application/
WORKDIR /app/Ba7besh.Bot
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/Ba7besh.Bot/out ./
ENTRYPOINT ["dotnet", "Ba7besh.Bot.dll"]