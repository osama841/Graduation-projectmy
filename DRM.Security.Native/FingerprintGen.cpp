#include "pch.h"
#include "NativeLogger.h"
#include <cstring> // for _strdup
#include <string>
#include "CoreResponse.h"

using namespace DRM::Security::Native::Unit;

// دالة مساعدة لنسخ النص إلى ذاكرة جديدة (لتجنب مشاكل الذاكرة)
char* AllocateString(const std::string& str) {
    char* result = new char[str.length() + 1];
    strcpy_s(result, str.length() + 1, str.c_str());
    return result;
}

extern "C" __declspec(dllexport) const char* GetDeviceFingerprintJson()
{
    // 1. تنظيف السجلات لبدء عملية جديدة
    NativeLogger::Clear();
    NativeLogger::Log("Init: Starting device fingerprint generation process.");

    CoreResponse response;

    try {
        // --- محاكاة لعملية استخراج البصمة (للتجربة) ---

        // خطوة 1
        NativeLogger::Log("Step 1: Initializing WMI connection...");
        // (تخيل هنا كود WMI)
        NativeLogger::Log("Success: WMI Connected.");

        // خطوة 2
        NativeLogger::Log("Step 2: Reading Motherboard Serial...");
        // (هنا سنضع قيمة وهمية للتجربة)
        std::string mbSerial = "BASE-BOARD-SERIAL-1234";
        NativeLogger::Log("Info: Found Motherboard ID: " + mbSerial);

        // خطوة 3
        NativeLogger::Log("Step 3: Hashing combined identifiers...");

        // النتيجة النهائية
        response.success = true;
        response.data = "A1B2-C3D4-E5F6-TEST-HASH"; // البصمة النهائية
        response.message = "Fingerprint extracted successfully.";
    }
    catch (const std::exception& e) {
        response.success = false;
        response.message = "Critical Error in Native Core.";
        response.errorCode = 500;
        NativeLogger::Log(std::string("EXCEPTION: ") + e.what());
    }
    catch (...) {
        response.success = false;
        response.message = "Unknown Error occurred.";
        response.errorCode = 501;
        NativeLogger::Log("EXCEPTION: Unknown system error...");
    }

    // 2. إرفاق السجلات الكاملة بالرد
    response.logs = NativeLogger::GetLogs();

    // 3. تحويل الرد إلى JSON وإرجاعه
    return AllocateString(response.ToJson());
}