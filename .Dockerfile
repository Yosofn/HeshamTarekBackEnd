# استخدم صورة الأساس لـ Node.js
FROM node:14 AS build

# تعيين مجلد العمل في الحاوية
WORKDIR /app

# نسخ ملفات المشروع إلى الحاوية
COPY . .

# تثبيت التبعيات وبناء المشروع
RUN npm install
RUN npm run build --prod

# استخدم صورة الأساس لـ Nginx
FROM nginx:alpine

# نسخ ملفات البناء من خطوة البناء السابقة إلى مجلد Nginx
COPY --from=build /app/dist/your-angular-app /usr/share/nginx/html

# نسخة من ملف التهيئة Nginx (اختياري)
#COPY nginx.conf /etc/nginx/nginx.conf

# الكشف عن منفذ 80 لتطبيق الويب
EXPOSE 80

# تشغيل خادم Nginx
CMD ["nginx", "-g", "daemon off;"]
