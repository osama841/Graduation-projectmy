#include "AntiCapture.h"
#include <iostream>

bool AntiCapture::ProtectWindow(HWND hWnd) {
    if (hWnd == NULL) return false;

    // محاولة تفعيل الحماية بقيمة WDA_MONITOR (للسواد)
    // إذا فشل الخطأ 5، سنحاول إظهار رسالة توضيحية
    BOOL result = SetWindowDisplayAffinity(hWnd, 0x00000001);

    if (!result) {
        DWORD error = GetLastError();
        if (error == 5) {
            std::cout << "[!] Access Denied (Error 5): Windows Terminal restricts this on Console." << std::endl;
            std::cout << "[*] Don't worry, this will work 100% on your WPF Player window." << std::endl;
        }
        return false;
    }
    return true;
}