# دليل النشر (Deployment) — للوصول من الايباد عبر رابط

النظام مُحزّم بالكامل في حاويات **Docker**: قاعدة بيانات SQL Server + واجهة .NET API + واجهة Angular.
بعد النشر تحصل على رابط عام تفتحه من الايباد أو أي جهاز.

> الواجهة (Angular) تُمرّر طلبات `/api` داخلياً إلى الباك اند عبر nginx، فتعمل كلها على **رابط واحد** بدون مشاكل CORS.

---

## أسرع طريقة: خادم سحابي (VM) + Docker (موصى به)

تعمل على أي مزود: **Azure VM، DigitalOcean، AWS EC2، Hetzner، Oracle Cloud (طبقة مجانية)...**

### 1) أنشئ خادماً (VM)
- نظام: **Ubuntu 22.04**.
- حجم: 2 vCPU و**4 GB RAM على الأقل** (SQL Server يحتاج ذاكرة).
- في إعدادات الجدار الناري/Networking افتح المنافذ: **22** (SSH) و**80** (HTTP).

### 2) ادخل على الخادم وثبّت Docker
```bash
ssh root@SERVER_IP
curl -fsSL https://get.docker.com | sh
```

### 3) أنزل المشروع
```bash
git clone https://github.com/AbuSari/Task-Managment.git
cd Task-Managment
git checkout claude/task-management-system-vg16oj
```

### 4) جهّز كلمات السر
```bash
cp .env.example .env
nano .env     # غيّر SA_PASSWORD و JWT_KEY إلى قيم قوية
```

### 5) شغّل كل شيء بأمر واحد
```bash
docker compose up -d --build
```
أول تشغيل يبني الصور ويُنشئ القاعدة والبيانات المبدئية (قد يأخذ 1–3 دقائق).

### 6) افتح الرابط من الايباد
```
http://SERVER_IP
```
سجّل الدخول:
- مدير: `manager@task.com` / `Manager@123`
- موظف: `employee@task.com` / `Employee@123`

### أوامر مفيدة
```bash
docker compose ps          # حالة الخدمات
docker compose logs -f api # سجلّات الـ API
docker compose down        # إيقاف
docker compose up -d --build  # إعادة تشغيل بعد تحديث الكود
```

---

## ربط دومين + HTTPS (اختياري لكنه مهم)
الايباد أحياناً يتطلب HTTPS. لإضافة شهادة مجانية بسهولة، استخدم **Caddy** كبروكسي أمام الواجهة:

1. وجّه دومينك (DNS A record) إلى `SERVER_IP`.
2. ثبّت Caddy على الخادم، وأنشئ `/etc/caddy/Caddyfile`:
   ```
   your-domain.com {
       reverse_proxy localhost:80
   }
   ```
3. `systemctl reload caddy` — يحصل على شهادة HTTPS تلقائياً.

بعدها افتح: `https://your-domain.com`

---

## بديل: منصّة Railway (نشر مُدار بدون خادم تديره بنفسك)
Railway يدعم تشغيل حاويات Docker (بما فيها SQL Server):
1. أنشئ حساباً على [railway.app](https://railway.app) واربط مستودع GitHub.
2. أضِف ثلاث خدمات: `db` (صورة `mcr.microsoft.com/mssql/server:2022-latest`)، `api` (من مجلد `backend`)، `frontend` (من مجلد `frontend`).
3. اضبط متغيّرات البيئة كما في `docker-compose.yml` (سلسلة الاتصال، `Jwt__Key`...).
4. Railway يعطي رابطاً عاماً للواجهة تلقائياً.

---

## بديل: Azure (خدمات مُدارة)
- **Azure SQL Database** بدل حاوية SQL.
- **Azure App Service (Linux, Container)** لنشر صورة الـ API.
- **Azure Static Web Apps** لنشر واجهة Angular.
- اضبط `ConnectionStrings__DefaultConnection` و`Jwt__Key` في إعدادات App Service.

---

## ملاحظات إنتاجية مهمة
- **غيّر كلمات السر الافتراضية** للحسابات التجريبية بعد أول دخول (أو احذفها من `DbSeeder`).
- اجعل `SA_PASSWORD` و`JWT_KEY` قويين وسريين (لا تحفظهما في git — ملف `.env` مُستثنى أصلاً).
- البيانات محفوظة في حجم Docker باسم `mssql-data` وتبقى بعد إعادة التشغيل.
- لأخذ نسخة احتياطية: انسخ حجم `mssql-data` أو استخدم `BACKUP DATABASE` داخل SQL Server.
