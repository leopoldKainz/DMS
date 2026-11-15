# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY DMS.OCR/DMS.OCR.csproj DMS.OCR/
COPY DMS.Business.Models/DMS.Business.Models.csproj DMS.Business.Models/
COPY DMS.DAL/DMS.DAL.csproj DMS.DAL/
COPY DMS.Storage/DMS.Storage.csproj DMS.Storage/
COPY DMS.ELK/DMS.ELK.csproj DMS.ELK/

# Restore dependencies
RUN dotnet restore DMS.OCR/DMS.OCR.csproj

# Copy source code
COPY DMS.OCR/ DMS.OCR/
COPY DMS.Business.Models/ DMS.Business.Models/
COPY DMS.DAL/ DMS.DAL/
COPY DMS.Storage/ DMS.Storage/
COPY DMS.ELK/ DMS.ELK/

# Build and publish
RUN dotnet publish DMS.OCR/DMS.OCR.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends \
    ghostscript \
    tesseract-ocr \
    tesseract-ocr-eng \
    tesseract-ocr-deu \
    tesseract-ocr-osd \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/5/tessdata

ENTRYPOINT ["dotnet", "DMS.OCR.dll"]
