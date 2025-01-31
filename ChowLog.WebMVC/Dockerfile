# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Accept build arguments
ARG BUILD_DATE
ARG BUILD_SOURCE
ARG BUILD_VERSION

# Pass them as environment variables to the container
ENV BUILD_DATE=$BUILD_DATE
ENV BUILD_SOURCE=$BUILD_SOURCE
ENV APP_VERSION=$BUILD_VERSION

#ENV DB_PATH="/app/app.db"
#ENV IMAGES_PATH="/app/wwwroot/plates"


## Create the necessary directories
#RUN mkdir -p "$(dirname "$DB_PATH")" && mkdir -p "$IMAGES_PATH"
#
## Ensure correct permissions (optional, adjust as needed)
#RUN chmod -R 755 "$(dirname "$DB_PATH")" "$IMAGES_PATH"

RUN mkdir -p /app/Data/Plates && chown -R app:app /app/Data


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ChowLog.Services/ChowLog.Services.csproj", "ChowLog.Common/"] 
COPY ["ChowLog.DataAccess/ChowLog.DataAccess.csproj", "ChowLog.DataAccess/"]
COPY ["ChowLog.Hubs/ChowLog.Hubs.csproj", "ChowLog.Hubs/"]
COPY ["ChowLog.Models/ChowLog.Models.csproj", "ChowLog.Models/"]
COPY ["ChowLog.Services/ChowLog.Services.csproj", "ChowLog.Services/"]
COPY ["ChowLog.Utilities/ChowLog.Utilities.csproj", "ChowLog.Utilities/"]
COPY ["ChowLog.WebMVC/ChowLog.WebMVC.csproj", "ChowLog.WebMVC/"]

RUN dotnet restore "./ChowLog.WebMVC/ChowLog.WebMVC.csproj"
RUN dotnet tool install --global dotnet-ef 

COPY . .
WORKDIR "/src/ChowLog.WebMVC"
RUN dotnet build "./ChowLog.WebMVC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChowLog.WebMVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ChowLog.WebMVC.dll"]