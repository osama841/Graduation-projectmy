#include "pch.h"
#include "NativeLogger.h"
// File: src/DRM.Security.Native/Common/NativeLogger.cpp


namespace DRM {
    namespace Security {
        namespace Native {
            namespace Unit {

                std::vector<std::string> NativeLogger::currentLogs;
                std::mutex NativeLogger::logMutex;

                void NativeLogger::Log(const std::string& message) {
                    std::lock_guard<std::mutex> lock(logMutex);
                    currentLogs.push_back(message);
                }

                void NativeLogger::Clear() {
                    std::lock_guard<std::mutex> lock(logMutex);
                    currentLogs.clear();
                }

                std::vector<std::string> NativeLogger::GetLogs() {
                    std::lock_guard<std::mutex> lock(logMutex);
                    return currentLogs;
                }

            }
        }
    }
}