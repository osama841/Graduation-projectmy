#pragma once

#ifdef DRMSECURITYNATIVE_EXPORTS
    #define DRM_API __declspec(dllexport)
#else
    #define DRM_API __declspec(dllimport)
#endif

extern "C" {
    // استخراج بصمة الجهاز - ترجع نص JSON أو رسالة خطأ
    DRM_API const char* GetDeviceFingerprint();
    
    // تحرير الذاكرة المخصصة
    DRM_API void FreeMem(void* ptr);
}
