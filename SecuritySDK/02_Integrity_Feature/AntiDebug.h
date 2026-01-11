#ifndef ANTI_DEBUG_H
#define ANTI_DEBUG_H

#include <windows.h>
#include <thread>
#include <atomic>

class IntegrityGuard {
private:
    std::atomic<bool> keepRunning; // للتحكم في تشغيل وإيقاف الحارس
    std::thread guardThread;        // الخيط الذي سيعمل فيه الحارس بالخلفية

    void WorkerThread();           // دالة الفحص المستمر

public:
    IntegrityGuard();
    ~IntegrityGuard();

    void Start(); // لتشغيل الحماية
    void Stop();  // لإيقاف الحماية

    // متغير ثابت: إذا صار true يعني البرنامج تحت هجوم
    static bool IsCompromised; 
};

#endif