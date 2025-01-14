FROM nginx AS base
EXPOSE 80
EXPOSE 443

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Signalizer.Admin/Signalizer.Admin.csproj", "Signalizer.Admin/"]
COPY ["Signalizer.Entities/Signalizer.Entities.csproj", "Signalizer.Entities/"]
COPY ["Signalizer.Extensions/Signalizer.Extensions.csproj", "Signalizer.Extensions/"]
RUN dotnet restore "./Signalizer.Admin/Signalizer.Admin.csproj"
COPY . .
WORKDIR "/src/Signalizer.Admin"
RUN dotnet build "./Signalizer.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Signalizer.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM  base AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY  Signalizer.Admin/nginx.conf /etc/nginx/nginx.conf