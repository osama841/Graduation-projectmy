#include "pch.h"
#include "AES_Core.h"

// ═══════════════════════════════════════════════════════
// القسم 1: الدوال القديمة (للترخيص والعمليات العامة)
// ═══════════════════════════════════════════════════════

extern "C" __declspec(dllexport) void GenerateRandomData(uint8_t* buffer, int size) {
    GenerateRandomBytes_Core(buffer, size);
}

extern "C" __declspec(dllexport) void* CreateAESContext(uint8_t* key) {
    return (void*)InitAES_Core(key);
}

extern "C" __declspec(dllexport) bool ProcessFrame(void* ctxPtr, uint8_t* data, int length, uint8_t* iv, long long offset) {
    AES_Context* ctx = (AES_Context*)ctxPtr;
    return ProcessChunk_Core(ctx, data, length, iv, offset);
}

extern "C" __declspec(dllexport) void DestroyAESContext(void* ctxPtr) {
    AES_Context* ctx = (AES_Context*)ctxPtr;
    CleanupAES_Core(ctx);
}
