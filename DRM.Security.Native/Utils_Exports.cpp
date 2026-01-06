#include "pch.h"
#include <cstring>

// دالة لتحرير الذاكرة التي حجزها C++ للنصوص الراجعة
extern "C" __declspec(dllexport) void FreeMem(char* ptr) {
    if (ptr != nullptr) {
        delete[] ptr;
    }
}