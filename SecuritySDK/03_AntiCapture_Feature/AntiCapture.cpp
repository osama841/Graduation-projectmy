#include "AntiCapture.h"

bool AntiCapture::ProtectWindow(HWND hWnd)
{
    if (hWnd == NULL)
        return false;

    // WDA_MONITOR = 0x01 (تمنع التقاط الشاشة وتجعل النافذة سوداء للمصور)
    // WDA_EXCLUDEFROMCAPTURE = 0x11 (في ويندوز 10 و 11 تجعل النافذة تختفي تماماً من التصوير)

    BOOL result = SetWindowDisplayAffinity(hWnd, 0x00000011);

    return (result != 0);
}