#include "pch.h" //  √ﬂœ √‰ Â–« „ÊÃÊœ ≈–« ﬂ‰   ” Œœ„ Visual Studio
#include "AES_Core.h"
#include <stdio.h>

// --- 1. œ«·…  Ê·Ìœ «·⁄‘Ê«∆Ì… (Secure Random) ---
bool GenerateRandomBytes_Core(uint8_t* buffer, int size) {
    // ‰” Œœ„ „Ê·œ «·‰Ÿ«„ «·¬„‰ (System Preferred RNG)
    // Â–« ¬„‰ ÷œ «· Œ„Ì‰ (Cryptographically Secure)
    NTSTATUS status = BCryptGenRandom(NULL, buffer, size, BCRYPT_USE_SYSTEM_PREFERRED_RNG);
    return BCRYPT_SUCCESS(status);
}

// --- 2.  ÂÌ∆… «· ‘›Ì— (  „ „—… Ê«Õœ… ›ﬁÿ) ---
AES_Context* InitAES_Core(uint8_t* keyRaw) {
    AES_Context* ctx = new AES_Context();
    ctx->hAlg = NULL;
    ctx->hKey = NULL;
    NTSTATUS status = 0;

    // › Õ «·ŒÊ«—“„Ì…
    if (!BCRYPT_SUCCESS(BCryptOpenAlgorithmProvider(&ctx->hAlg, BCRYPT_AES_ALGORITHM, MS_PRIMITIVE_PROVIDER, 0))) {
        delete ctx; return nullptr;
    }

    // ÷»ÿ Ê÷⁄ ECB (·√‰‰« ‰»‰Ì CTR ÌœÊÌ«)
    if (!BCRYPT_SUCCESS(BCryptSetProperty(ctx->hAlg, BCRYPT_CHAINING_MODE, (PUCHAR)BCRYPT_CHAIN_MODE_ECB, sizeof(BCRYPT_CHAIN_MODE_ECB), 0))) {
        BCryptCloseAlgorithmProvider(ctx->hAlg, 0); delete ctx; return nullptr;
    }

    // ÕÃ“ «·–«ﬂ—… ··ﬂ«∆‰
    DWORD cbKeyObject = 0, cbData = 0;
    BCryptGetProperty(ctx->hAlg, BCRYPT_OBJECT_LENGTH, (PUCHAR)&cbKeyObject, sizeof(DWORD), &cbData, 0);
    ctx->keyObject.resize(cbKeyObject);

    // ’‰«⁄… «·„› «Õ
    if (!BCRYPT_SUCCESS(BCryptGenerateSymmetricKey(ctx->hAlg, &ctx->hKey, ctx->keyObject.data(), cbKeyObject, keyRaw, 16, 0))) {
        BCryptCloseAlgorithmProvider(ctx->hAlg, 0); delete ctx; return nullptr;
    }

    return ctx;
}

// --- 3. «· ‰ŸÌ› ---
void CleanupAES_Core(AES_Context* ctx) {
    if (ctx) {
        if (ctx->hKey) BCryptDestroyKey(ctx->hKey);
        if (ctx->hAlg) BCryptCloseAlgorithmProvider(ctx->hAlg, 0);
        delete ctx;
    }
}

// --- 4. «·„⁄«·Ã… (”—Ì⁄… Ãœ« Ê„Õ”‰…) ---
bool ProcessChunk_Core(AES_Context* ctx, uint8_t* data, int length, uint8_t* iv, long long fileOffset) {
    if (!ctx || !ctx->hKey) return false;

    uint8_t counter_block[16];
    uint8_t keystream[16];
    ULONG resultLen = 0;

    // 1. Õ”«» «·⁄œ«œ «·√Ê·Ì
    long long blockIndex = fileOffset / 16;

    // ‰”Œ «·‹ IV ··»œ¡ „‰Â
    memcpy(counter_block, iv, 16);

    // «· Õ”Ì‰ «·—Ì«÷Ì (≈÷«›… blockIndex ··⁄œ«œ)
    long long tempIndex = blockIndex;

    // «·Õ·ﬁ… «·„Õ”‰… („⁄ break Ê long long sum ·„‰⁄ «·ÿ›Õ)
    for (int i = 15; i >= 0; i--) {
        if (tempIndex == 0) break; // <-- «· Õ”Ì‰: «·Œ—ÊÃ «·„»ﬂ—

        // «” Œœ„‰« long long ··„Ã„Ê⁄ ·÷„«‰ ⁄œ„ ÕœÊÀ Overflow
        long long sum = (long long)counter_block[i] + (tempIndex & 0xFF);

        counter_block[i] = (uint8_t)sum;
        tempIndex = (tempIndex >> 8) + (sum >> 8);
    }

    int byteOffset = fileOffset % 16;
    int processed = 0;

    while (processed < length) {
        // ‰”Œ «·⁄œ«œ «·Õ«·Ì · ‘›Ì—Â
        memcpy(keystream, counter_block, 16);

        // «· ‘›Ì— ( Ê·Ìœ «·„› «Õ «·„ œ›ﬁ)
        if (!BCRYPT_SUCCESS(BCryptEncrypt(ctx->hKey, keystream, 16, NULL, NULL, 0, keystream, 16, &resultLen, 0))) {
            return false;
        }

        // XOR
        int streamPos = (processed == 0) ? byteOffset : 0;
        int chunkSize = (length - processed > (16 - streamPos)) ? (16 - streamPos) : (length - processed);

        for (int i = 0; i < chunkSize; i++) {
            data[processed + i] ^= keystream[streamPos + i];
        }

        // “Ì«œ… «·⁄œ«œ
        if (chunkSize + streamPos == 16) {
            for (int i = 15; i >= 0; i--) {
                if (++counter_block[i] != 0) break;
            }
            byteOffset = 0;
        }
        processed += chunkSize;
    }
    return true;
}