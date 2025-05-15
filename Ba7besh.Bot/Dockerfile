FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

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
FROM base
WORKDIR /app
COPY --from=build /app/Ba7besh.Bot/out ./

# Set environment variables with defaults, these can be overridden at runtime
ENV BotConfiguration__BotToken="YOUR_BOT_TOKEN" \
    Api__BaseUrl="http://api:8080/api/v1" \
    Api__AuthToken="" \
    ASPNETCORE_ENVIRONMENT="Production" \
    ASPNETCORE_URLS="http://+:80"

ENTRYPOINT ["dotnet", "Ba7besh.Bot.dll"]