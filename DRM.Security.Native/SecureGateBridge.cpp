#include "pch.h"
#include "SecurityManager.h"
#include "VmEnvironmentDetector.h"
#include <windows.h>
#include <string>

// ملاحظة: GetDeviceFingerprint و FreeMem موجودين في Exports/HardwareID_Exports.cpp
// لا حاجة لتكرارهم هنا

// 🔐 بوابة فك التشفير (سنقوم بدمجها لاحقاً مع محرك White-Box)
extern "C" __declspec(dllexport) bool ProcessSecureFrame(unsigned char* data, int len) {
    // فحص أمني سريع قبل المعالجة باستخدام كودك الحقيقي
    VmEnvironmentDetector detector;
    if (detector.RunScan() >= 60) return false; // إذا اكتشف VM احظر العملية

    // هنا سيتم وضع منطق فك التشفير لاحقاً
    return true;
}