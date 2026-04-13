FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["XatiCraft/XatiCraft.csproj", "XatiCraft/"]
RUN dotnet restore "XatiCraft/XatiCraft.csproj"
COPY . .
WORKDIR "/src/XatiCraft"
RUN dotnet build "./XatiCraft.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./XatiCraft.csproj" -c $BUILD_CONFIGURATION -o /app/publish -p:UseAppHost=false -p:DefineConstants="USE_CERT"
#RUN dotnet publish "./XatiCraft.csproj" -c $BUILD_CONFIGURATION -o /app/publish -p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XatiCraft.dll"]
