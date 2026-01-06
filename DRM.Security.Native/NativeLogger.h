
#pragma once
#include <string>
#include <vector>
#include <mutex>

namespace DRM {
    namespace Security {
        namespace Native {
            namespace Unit {
                class NativeLogger {
                private:
                    static std::vector<std::string> currentLogs;
                    static std::mutex logMutex; // لضمان الأمان في تعدد المهام

                public:
                    // إضافة سجل جديد
                    static void Log(const std::string& message);

                    // مسح السجلات (بداية عملية جديدة)
                    static void Clear();

                    // جلب السجلات الحالية لإرسالها
                    static std::vector<std::string> GetLogs();
             };

            }
        }
    }
}


