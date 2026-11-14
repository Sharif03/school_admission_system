# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files
COPY SchoolAdmissionSystem.sln ./

# Copy project files first for better Docker layer caching
COPY backend/SchoolAdmission.Domain/SchoolAdmission.Domain.csproj backend/SchoolAdmission.Domain/
COPY backend/SchoolAdmission.Application/SchoolAdmission.Application.csproj backend/SchoolAdmission.Application/
COPY backend/SchoolAdmission.Infrastructure/SchoolAdmission.Infrastructure.csproj backend/SchoolAdmission.Infrastructure/
COPY backend/SchoolAdmission.API/SchoolAdmission.API.csproj backend/SchoolAdmission.API/

# Restore NuGet dependencies
RUN dotnet restore SchoolAdmissionSystem.sln

# Copy remaining backend source code
COPY backend/ backend/

# Publish API
RUN dotnet publish backend/SchoolAdmission.API/SchoolAdmission.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SchoolAdmission.API.dll"]