FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk add --no-cache clang build-base zlib-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Webhook/Webhook.csproj", "Webhook/"]
RUN dotnet restore "Webhook/Webhook.csproj"
COPY . .
WORKDIR "/src/Webhook"
RUN dotnet publish "./Webhook.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -r linux-musl-x64

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./Webhook"]