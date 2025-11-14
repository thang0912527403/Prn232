FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# COPY csproj và restore
COPY ["EbayClone.API.csproj", "./"]
RUN dotnet restore "EbayClone.API.csproj"

# COPY toàn bộ source và build
COPY . .
RUN dotnet build "EbayClone.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "EbayClone.API.csproj" -c Release -o /app/publish

# Runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "EbayClone.API.dll"]
