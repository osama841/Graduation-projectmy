# دليل تشغيل المشروع (العربي)

## 1️⃣ بنية المشروع
```
├─ .git/                # بيانات Git
├─ .gitignore           # ملفات ومجلدات مستبعدة
├─ .vs/                 # ملفات Visual Studio المؤقتة
├─ Backup/              # نسخ احتياطية
├─ DRM.Security.Native/ # مكتبة أمان أصلية (C/C++)
├─ README.md            # وصف عام للمشروع
├─ SecureGate_DRM_Solution.sln # ملف حل Visual Studio
├─ packages/            # حزم NuGet (مستبعدة)
├─ src/                 # الكود المصدري
│   ├─ Apps.AdminPanel/   # تطبيق لوحة الإدارة (WPF)
│   ├─ Apps.ClientTool/   # أداة عميل (Console)
│   ├─ Apps.SecurePlayer/ # مشغل وسائط آمن
│   ├─ DRM.Shared/        # مكتبة مشتركة (C#)
│   └─ SecurePlayer/      # مشروع مشغل آخر
├─ tests/               # اختبارات (إن وجدت)
└─ x64/                # مخرجات البناء (مستبعدة)
```

## 2️⃣ المتطلبات المسبقة
| الأداة / المكتبة | السبب | الحالة |
|-------------------|--------|--------|
| **.NET SDK 9.0** | تجميع مشاريع C# | ✅ مثبت
| **Visual Studio 2022** (أو أحدث) مع حزمة **.NET desktop development** | بيئة تطوير WPF | ✅ مثبت
| **NuGet** | استعادة الحزم (`BCrypt.Net-Next`, `MaterialDesignThemes`, `Microsoft.EntityFrameworkCore.Sqlite`) | ✅ مدمج في VS
| **SQLite** | قاعدة بيانات محلية | ✅ مدمج في .NET
| **C++ Build Tools** (Desktop development with C++) | بناء مكتبة `DRM.Security.Native` إذا لم تكن مُجمعة مسبقاً | ❓ تحقق/ثبت إذا لزم الأمر
| **Git** | عمليات التحكم بالإصدار | ✅ مثبت
| **PowerShell** | تشغيل الأوامر على Windows | ✅ مثبت |

## 3️⃣ إعداد أول مرة
1. **فتح الحل**
   ```powershell
   cd "C:\Users\Osama\Desktop\Graduation projectmy\Graduation project"
   start SecureGate_DRM_Solution.sln
   ```
2. **استعادة حزم NuGet** (Visual Studio يفعل ذلك تلقائياً، أو يدويًا):
   ```powershell
   dotnet restore
   ```
3. **بناء المكتبة الأصلية** (إذا كان مصدر `DRM.Security.Native` موجوداً):
   ```powershell
   cd DRM.Security.Native
   msbuild DRM.Security.Native.vcxproj /p:Configuration=Release
   ```
   إذا وجدت ملفات DLL جاهزة في المجلد، يمكنك تخطي هذه الخطوة.
4. **تطبيق ترحيلات قاعدة البيانات** (EF Core + SQLite):
   ```powershell
   dotnet ef database update --project src/DRM.Shared/DRM.Shared.csproj
   ```
   سيُنشئ ملف قاعدة البيانات (مثل `securegate.db`).

## 4️⃣ تشغيل التطبيقات
| التطبيق | طريقة التشغيل |
|---------|----------------|
| **لوحة الإدارة** (`Apps.AdminPanel`) | في Visual Studio اضبطه كـ Startup Project → **F5**، أو:
```powershell
dotnet run --project src/Apps.AdminPanel/Apps.AdminPanel.csproj
```
| **أداة العميل** (`Apps.ClientTool`) | ```powershell
dotnet run --project src/Apps.ClientTool/Apps.ClientTool.csproj
```
| **المشغل الآمن** (`Apps.SecurePlayer`) | ```powershell
dotnet run --project src/Apps.SecurePlayer/Apps.SecurePlayer.csproj
```
| **SecurePlayer** (المشروع الثاني) | استبدل مسار `.csproj` بالمسار المناسب.

## 5️⃣ أوامر شائعة
- **تنظيف المستودع من الملفات الكبيرة** (تم إضافة القواعد إلى `.gitignore`):
  ```powershell
  git rm -r --cached x64 .vs packages Backup
  git add .gitignore
  git commit -m "Clean repo, ignore build artefacts"
  git push origin main
  ```
- **تشغيل الاختبارات** (إن وجدت في `tests/`):
  ```powershell
  dotnet test tests
  ```
- **بناء كل المشاريع**:
  ```powershell
  dotnet build
  ```

## 6️⃣ ما قد يكون ناقصاً / اختياريًا
- **C++ Build Tools** – ضرورية فقط إذا أردت إعادة بناء `DRM.Security.Native`. يمكن تثبيتها عبر **Visual Studio Installer → Desktop development with C++**.
- **SQLite Browser** – لتفحص قاعدة البيانات يدويًا (اختياري).

## 7️⃣ قائمة التحقق السريعة قبل أول تشغيل
- [ ] تثبيت .NET 9.0 SDK (`dotnet --list-sdks`).
- [ ] تثبيت Visual Studio 2022 مع **.NET desktop development** و **Desktop development with C++** (إذا لزم الأمر).
- [ ] تشغيل `dotnet restore`.
- [ ] بناء المكتبة الأصلية إذا كان المصدر موجودًا.
- [ ] تطبيق ترحيلات EF Core.
- [ ] تشغيل التطبيق المطلوب.

---
**ملحوظة:** إذا ظهرت لك أي رسائل خطأ أثناء أي خطوة، أرسل نص الخطأ بالكامل وسأساعدك في حلّها.
