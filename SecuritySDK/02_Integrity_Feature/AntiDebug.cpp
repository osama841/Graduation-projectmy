#include "AntiDebug.h"
#include <iostream>
#include <chrono>

// تهيئة متغير الحالة (في البداية الوضع آمن)
bool IntegrityGuard::IsCompromised = false;

IntegrityGuard::IntegrityGuard() : keepRunning(false) {}

IntegrityGuard::~IntegrityGuard() { Stop(); }

void IntegrityGuard::Start()
{
    if (keepRunning)
        return;
    keepRunning = true;
    IsCompromised = false;
    // تشغيل الحارس في خيط منفصل لكي لا يتوقف البرنامج الأصلي
    guardThread = std::thread(&IntegrityGuard::WorkerThread, this);
}

void IntegrityGuard::Stop()
{
    keepRunning = false;
    if (guardThread.joinable())
        guardThread.join();
}

void IntegrityGuard::WorkerThread()
{
    while (keepRunning)
    {
        bool detected = false;

        // 🔍 الفحص الأول: هل هناك Debugger ملتصق بالبرنامج؟
        if (IsDebuggerPresent())
        {
            detected = true;
        }

        // 🔍 الفحص الثاني: فحص الـ Debugger عن بعد (أكثر قوة)
        BOOL isRemoteDebugger = FALSE;
        CheckRemoteDebuggerPresent(GetCurrentProcess(), &isRemoteDebugger);
        if (isRemoteDebugger)
        {
            detected = true;
        }

        if (detected)
        {
            IntegrityGuard::IsCompromised = true;
            // يمكنك هنا إضافة أمر لغلق البرنامج فوراً إذا أردت:
            // exit(0);
        }

        // انتظر ثانية واحدة قبل الفحص القادم لتوفير المعالج
        std::this_thread::sleep_for(std::chrono::milliseconds(1000));
    }
}