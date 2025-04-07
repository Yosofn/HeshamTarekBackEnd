# 1. استخدم صورة .NET 8 الرسمية لتشغيل التطبيق
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. استخدم صورة SDK الخاصة ببناء التطبيق
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore

# 3. نسخ باقي ملفات المشروع وبناؤه
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 4. إعداد الـ Runtime Image وتشغيل التطبيق
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]