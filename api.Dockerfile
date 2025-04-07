# استخدم صورة الأساس لـ .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# نسخ ملفات اcd لمشروع واستعادة التبعيات
COPY . ./
RUN dotnet restore

# بناء المشروع
RUN dotnet publish -c Release -o out

# استخدم صورة الأساس لـ .NET Runtime لتشغيل التطبيق
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# تشغيل التطبيق
ENTRYPOINT ["dotnet", "API.dll"]
