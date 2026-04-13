FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
RUN apk add --no-cache icu-libs icu-data-full docker-cli
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Webhook/Webhook.csproj", "Webhook/"]
RUN dotnet restore "Webhook/Webhook.csproj"
COPY . .
WORKDIR "/src/Webhook"
RUN dotnet build "./Webhook.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Webhook.csproj" -r linux-musl-x64 -c $BUILD_CONFIGURATION -o /app/publish -p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Webhook.dll"]