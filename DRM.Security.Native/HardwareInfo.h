// File: src/DRM.Security.Native/Modules/HardwareID/HardwareInfo.h
#pragma once
#include <string>
#include <vector>

namespace DRM {
    namespace Security {
        namespace Native {
            namespace Modules {
                namespace HardwareID {

                    class HardwareInfo {
                    public:
                        // دالة مساعدة لجلب رقم اللوحة الأم التسلسلي (باستخدام WMI)
                        static std::string GetMotherboardSerial();

                        // دالة مساعدة لجلب رقم المعالج التسلسلي (باستخدام WMI)
                        static std::string GetProcessorID();

                        // دالة مساعدة لجلب رقم القرص الصلب التسلسلي (باستخدام WMI)
                        static std::string GetHDDSerial();

                        // يمكن إضافة المزيد من دوال جمع المعلومات هنا
                    };

                }
            }
        }
    }
}