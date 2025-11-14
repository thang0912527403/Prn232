# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# COPY csproj và restore
COPY ["EbayClone.Web.csproj", "./"]
RUN dotnet restore "EbayClone.Web.csproj"

# COPY toàn bộ source và build
COPY . .
RUN dotnet build "EbayClone.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "EbayClone.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "EbayClone.Web.dll"]
