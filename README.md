# نظام إدارة ومتابعة المهام (Task Management System)

نظام متكامل لإدارة المهام ومتابعتها، يتيح للموظفين إدخال مهامهم عبر **جدول قابل للتحرير يشبه الإكسل**، مع **استيراد وتصدير ملفات Excel (.xlsx)**، وحفظ كل البيانات في قاعدة بيانات **SQL Server**.

## التقنيات المستخدمة

| الطبقة | التقنية |
|--------|---------|
| الواجهة الأمامية (Frontend) | Angular 19 (Standalone) + TypeScript + SCSS |
| الواجهة الخلفية (Backend) | ASP.NET Core 8 Web API (C#) |
| قاعدة البيانات | SQL Server + Entity Framework Core 8 |
| المصادقة | JWT + الأدوار (موظف / مدير) |
| Excel | ClosedXML |

## المزايا

- 🔐 تسجيل دخول وصلاحيات بدور **موظف** و**مدير**.
- 📊 **جدول تحرير شبيه بالإكسل**: إضافة/تعديل/حذف صفوف وحفظها دفعة واحدة.
- 📥 **استيراد** مهام من ملف Excel، و📤 **تصدير** المهام إلى Excel، وتحميل **قالب** فارغ.
- 👥 المدير يرى ويُسند مهام جميع الموظفين، والموظف يرى مهامه فقط.
- 📈 متابعة الحالة، الأولوية، نسبة الإنجاز، التواريخ، والساعات.
- 📚 توثيق API تلقائي عبر Swagger.

---

## التشغيل

### المتطلبات
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (أو SQL Server LocalDB / Express)
- [Node.js](https://nodejs.org/) 18.19+ و npm

### 1) الواجهة الخلفية (Backend)

```bash
cd backend/src/TaskManagement.Api

# عدّل سلسلة الاتصال في appsettings.json أو appsettings.Development.json حسب خادم SQL لديك
# ثم شغّل المشروع (سيُنشئ قاعدة البيانات والبيانات المبدئية تلقائياً):
dotnet restore
dotnet run
```

- الـ API يعمل على: `https://localhost:7xxx` و `http://localhost:5xxx` (انظر الإخراج).
- توثيق Swagger: `/swagger`.
- قاعدة البيانات تُنشأ تلقائياً عند أول تشغيل عبر EF Core Migrations + بيانات مبدئية.

> لإنشاء/تحديث المخطط يدوياً: `dotnet ef database update`

### 2) الواجهة الأمامية (Frontend)

```bash
cd frontend
npm install
npm start
```

افتح المتصفح على: `http://localhost:4200`

> اضبط عنوان الـ API في `src/environments/environment.ts` إذا اختلف المنفذ.

---

## حسابات الدخول الافتراضية

| الدور | البريد | كلمة المرور |
|-------|--------|-------------|
| مدير | `manager@task.com` | `Manager@123` |
| موظف | `employee@task.com` | `Employee@123` |

---

## بنية المشروع

```
Task-Managment/
├── backend/
│   ├── TaskManagement.sln
│   └── src/TaskManagement.Api/
│       ├── Controllers/   (Auth, Tasks, Users, Excel)
│       ├── Data/          (AppDbContext, DbSeeder)
│       ├── DTOs/
│       ├── Migrations/    (InitialCreate)
│       ├── Models/        (User, TaskItem, Enums)
│       ├── Services/      (JwtService, ExcelService)
│       └── Program.cs
└── frontend/
    └── src/app/
        ├── core/      (services, guards, interceptors, models)
        ├── features/  (login, tasks)
        └── ...
```

## أعمدة ملف Excel (الاستيراد/التصدير)

`العنوان | الوصف | الحالة | الأولوية | نسبة الإنجاز % | تاريخ البدء | تاريخ الاستحقاق | الساعات المقدّرة | الساعات الفعلية | ملاحظات | بريد المسؤول`

- **الحالة**: `New, InProgress, OnHold, Completed, Cancelled`
- **الأولوية**: `Low, Medium, High, Critical`
