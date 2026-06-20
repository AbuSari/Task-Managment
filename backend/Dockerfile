# ===== مرحلة البناء =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/TaskManagement.Api/TaskManagement.Api.csproj src/TaskManagement.Api/
RUN dotnet restore src/TaskManagement.Api/TaskManagement.Api.csproj

COPY . .
RUN dotnet publish src/TaskManagement.Api/TaskManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== مرحلة التشغيل =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# الصورة الأساسية تستمع على المنفذ 8080
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
