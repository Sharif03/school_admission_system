# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SchoolAdmissionSystem.sln ./

COPY backend/SchoolAdmission.Domain/SchoolAdmission.Domain.csproj backend/SchoolAdmission.Domain/
COPY backend/SchoolAdmission.Application/SchoolAdmission.Application.csproj backend/SchoolAdmission.Application/
COPY backend/SchoolAdmission.Infrastructure/SchoolAdmission.Infrastructure.csproj backend/SchoolAdmission.Infrastructure/
COPY backend/SchoolAdmission.API/SchoolAdmission.API.csproj backend/SchoolAdmission.API/

# Restore dependencies
RUN dotnet restore SchoolAdmissionSystem.sln

# Copy the remaining source files
COPY backend/ backend/

# Publish API
RUN dotnet publish backend/SchoolAdmission.API/SchoolAdmission.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SchoolAdmission.API.dll"]