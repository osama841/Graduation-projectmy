#pragma once
#include <windows.h>
#include <bcrypt.h>
#include <vector>
#include <cstdint>

// —»ÿ «·„ﬂ »…
#pragma comment(lib, "bcrypt.lib")

// «·ÂÌﬂ· «·–Ì ÌÕ„· "√”—«—" «· ‘›Ì— ›Ì «·–«ﬂ—…
struct AES_Context {
    BCRYPT_ALG_HANDLE hAlg;
    BCRYPT_KEY_HANDLE hKey;
    std::vector<uint8_t> keyObject; // «·–«ﬂ—… «·›⁄·Ì… ··„› «Õ
};

// œÊ«· «·„Õ—ﬂ «·œ«Œ·Ì (€Ì— „—∆Ì… ··‹ C#)
AES_Context* InitAES_Core(uint8_t* keyRaw);
void CleanupAES_Core(AES_Context* ctx);
bool ProcessChunk_Core(AES_Context* ctx, uint8_t* data, int length, uint8_t* iv, long long fileOffset);
bool GenerateRandomBytes_Core(uint8_t* buffer, int size);