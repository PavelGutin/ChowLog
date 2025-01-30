# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Accept build arguments
ARG BUILD_DATE
ARG BUILD_SOURCE

# Pass them as environment variables to the container
ENV BUILD_DATE=$BUILD_DATE
ENV BUILD_SOURCE=$BUILD_SOURCE

ENV DB_PATH="/app/data/app.db"
ENV IMAGES_PATH="/app/wwwroot/plates"


# Create the necessary directories
RUN mkdir -p "$(dirname "$DB_PATH")" && mkdir -p "$IMAGES_PATH"

# Ensure correct permissions (optional, adjust as needed)
RUN chmod -R 755 "$(dirname "$DB_PATH")" "$IMAGES_PATH"

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AllYourPlates.Services/AllYourPlates.Services.csproj", "AllYourPlates.Common/"]
COPY ["AllYourPlates.DataAccess/AllYourPlates.DataAccess.csproj", "AllYourPlates.DataAccess/"]
COPY ["AllYourPlates.Hubs/AllYourPlates.Hubs.csproj", "AllYourPlates.Hubs/"]
COPY ["AllYourPlates.Models/AllYourPlates.Models.csproj", "AllYourPlates.Models/"]
COPY ["AllYourPlates.Services/AllYourPlates.Services.csproj", "AllYourPlates.Services/"]
COPY ["AllYourPlates.Utilities/AllYourPlates.Utilities.csproj", "AllYourPlates.Utilities/"]
COPY ["AllYourPlates.WebMVC/AllYourPlates.WebMVC.csproj", "AllYourPlates.WebMVC/"]

RUN dotnet restore "./AllYourPlates.WebMVC/AllYourPlates.WebMVC.csproj"
RUN dotnet tool install --global dotnet-ef 
RUN /root/.dotnet/tools/dotnet-ef migrations add InitialCreate --project "./AllYourPlates.WebMVC/AllYourPlates.WebMVC.csproj" -v
RUN /root/.dotnet/tools/dotnet-ef database update


COPY . .
WORKDIR "/src/AllYourPlates.WebMVC"
RUN dotnet build "./AllYourPlates.WebMVC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AllYourPlates.WebMVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#COPY --from=build /src/AllYourPlates.WebMVC/Migrations ./Migrations 
ENTRYPOINT ["dotnet", "AllYourPlates.WebMVC.dll"]