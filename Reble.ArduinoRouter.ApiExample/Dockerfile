# Build stage
FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY Reble.ArduinoRouter/*.csproj Reble.ArduinoRouter/
COPY Reble.ArduinoRouter.ApiExample/*.csproj Reble.ArduinoRouter.ApiExample/

# Restore dependencies
RUN dotnet restore Reble.ArduinoRouter.ApiExample/Reble.ArduinoRouter.ApiExample.csproj

# Copy source code
COPY Reble.ArduinoRouter/ Reble.ArduinoRouter/
COPY Reble.ArduinoRouter.ApiExample/ Reble.ArduinoRouter.ApiExample/

# Build and publish
RUN dotnet publish Reble.ArduinoRouter.ApiExample/Reble.ArduinoRouter.ApiExample.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

# The Arduino Router socket must be mounted at runtime:
# docker run -v /var/run/arduino-router.sock:/var/run/arduino-router.sock ...

EXPOSE 8080

ENTRYPOINT ["dotnet", "Reble.ArduinoRouter.ApiExample.dll"]
