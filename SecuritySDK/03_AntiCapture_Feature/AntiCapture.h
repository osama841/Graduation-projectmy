#ifndef ANTI_CAPTURE_H
#define ANTI_CAPTURE_H

#include <windows.h>

class AntiCapture {
public:
    // هذه الدالة تجعل النافذة سوداء في برامج التسوير
    static bool ProtectWindow(HWND hWnd);
};

#endif