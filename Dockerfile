# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY DMS.API/DMS.API.csproj DMS.API/
COPY DMS.Business/DMS.Business.csproj DMS.Business/
COPY DMS.Business.Models/DMS.Business.Models.csproj DMS.Business.Models/
COPY DMS.DAL/DMS.DAL.csproj DMS.DAL/

# Restore dependencies
RUN dotnet restore DMS.API/DMS.API.csproj

# Copy source code
COPY DMS.API/ DMS.API/
COPY DMS.Business/ DMS.Business/
COPY DMS.Business.Models/ DMS.Business.Models/
COPY DMS.DAL/ DMS.DAL/

# Build and publish
RUN dotnet publish DMS.API/DMS.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DMS.API.dll"]
