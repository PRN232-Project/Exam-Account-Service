# Use .NET 9 SDK as build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY ["PRN232.ExamAccount.Api/PRN232.ExamAccount.Api.csproj", "PRN232.ExamAccount.Api/"]
COPY ["PRN232.ExamAccount.Application/PRN232.ExamAccount.Application.csproj", "PRN232.ExamAccount.Application/"]
COPY ["PRN232.ExamAccount.Infrastructure/PRN232.ExamAccount.Infrastructure.csproj", "PRN232.ExamAccount.Infrastructure/"]
COPY ["PRN232.ExamAccount.Domain/PRN232.ExamAccount.Domain.csproj", "PRN232.ExamAccount.Domain/"]

# Restore dependencies
RUN dotnet restore "PRN232.ExamAccount.Api/PRN232.ExamAccount.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the API project
WORKDIR "/src/PRN232.ExamAccount.Api"
RUN dotnet publish "PRN232.ExamAccount.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use .NET 9 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose HTTP API port
EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.ExamAccount.Api.dll"]


