#include "pch.h"
#include "AES_Core.h"

// ÊÕÏíÑ ÇáÏæÇá ááÜ DLL

extern "C" __declspec(dllexport) void GenerateRandomData(uint8_t * buffer, int size) {
    GenerateRandomBytes_Core(buffer, size);
}

// 1. ÅäÔÇÁ ÇáÓíÇŞ (íäÇÏì ãÑÉ æÇÍÏÉ ÚäÏ İÊÍ ÇáİíÏíæ)
// äÑÌÚ void* áÃäå İí C# ÓíÊã ÇÓÊŞÈÇáå ßÜ IntPtr
extern "C" __declspec(dllexport) void* CreateAESContext(uint8_t * key) {
    return (void*)InitAES_Core(key);
}

// 2. ãÚÇáÌÉ ÇáİÑíãÇÊ (ÓÑíÚÉ ÌÏÇğ áÃäåÇ ÊÓÊŞÈá ÇáÓíÇŞ ÇáÌÇåÒ ctx)
extern "C" __declspec(dllexport) bool ProcessFrame(void* ctxPtr, uint8_t * data, int length, uint8_t * iv, long long offset) {
    AES_Context* ctx = (AES_Context*)ctxPtr;
    return ProcessChunk_Core(ctx, data, length, iv, offset);
}

// 3. ÊÏãíÑ ÇáÓíÇŞ (ÚäÏ ÅÛáÇŞ ÇáİíÏíæ)
extern "C" __declspec(dllexport) void DestroyAESContext(void* ctxPtr) {
    AES_Context* ctx = (AES_Context*)ctxPtr;
    CleanupAES_Core(ctx);
}
