// HardwareID_Exports.cpp - نقطة الدخول الرئيسية للتحقق الأمني
#include <windows.h>
#include <string>
#include <cstring>
#include "HardwareID_Exports.h"
#include "../SecurityManager.h"

// استخدام buffer ثابت بدلاً من malloc/free لتجنب مشاكل الذاكرة
static char g_resultBuffer[4096] = {0};

// دالة مساعدة لنسخ النتيجة إلى الـ buffer الثابت
static const char* CopyToBuffer(const std::string& result) {
    memset(g_resultBuffer, 0, sizeof(g_resultBuffer));
    size_t len = result.length();
    if (len >= sizeof(g_resultBuffer)) {
        len = sizeof(g_resultBuffer) - 1;
    }
    memcpy(g_resultBuffer, result.c_str(), len);
    return g_resultBuffer;
}

extern "C" {

    DRM_API const char* GetDeviceFingerprint() {
        try {
            // ✅ استخدام SecurityManager الذي يتضمن:
            // 1. فحص VM (VmEnvironmentDetector) - 15+ فحص
            // 2. فحص TPM الحقيقي/الوهمي
            // 3. استخراج البصمة (DeviceFingerprintExtractor)
            // 4. تشفير SHA256
            std::string fingerprint = SecurityManager::GenerateHandshakeToken();
            
            // التحقق من النتيجة
            if (fingerprint.empty()) {
                return CopyToBuffer("ERROR_EMPTY_FINGERPRINT");
            }
            
            // التحقق من حالات الخطأ أو الحظر
            if (fingerprint.find("ERROR") != std::string::npos || 
                fingerprint.find("BLOCK") != std::string::npos) {
                return CopyToBuffer(fingerprint);
            }
            
            return CopyToBuffer(fingerprint);
        }
        catch (const std::exception& ex) {
            std::string error = "ERROR_CPP_EXCEPTION: ";
            error += ex.what();
            return CopyToBuffer(error);
        }
        catch (...) {
            return CopyToBuffer("ERROR_UNKNOWN_EXCEPTION");
        }
    }

    DRM_API void FreeMem(void* ptr) {
        // لا نحتاج لتحرير أي شيء لأننا نستخدم buffer ثابت
        // هذه الدالة موجودة للتوافق مع الكود القديم
        (void)ptr; // تجاهل المعامل
    }

}




