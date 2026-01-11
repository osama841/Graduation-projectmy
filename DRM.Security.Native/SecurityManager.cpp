#include "pch.h"
#include "SecurityManager.h"
#include "DeviceFingerprintExtractor.h"
#include "TpmKeyProvider.h"
#include "VmEnvironmentDetector.h"
#include <iostream>
#include <sstream>
#include <iomanip>

#pragma comment(lib, "bcrypt.lib")

#ifndef NT_SUCCESS
#define NT_SUCCESS(Status) (((NTSTATUS)(Status)) >= 0)
#endif

SecurityManager::SecurityManager() {}
SecurityManager::~SecurityManager() {}

// =========================================================
// دوال مساعدة داخلية
// =========================================================

bool SecurityManager::IsVirtualEnvironment() {
    try {
        VmEnvironmentDetector detector;
        detector.RunScan();
        return detector.IsVirtualMachine();
    }
    catch (...) {
        return false; // عند الفشل، افترض أنه جهاز حقيقي
    }
}

std::string SecurityManager::GetRawDeviceFingerprint() {
    try {
        // 1. الأولوية لـ TPM (لأنها مفتاح عتادي قوي لا يتغير)
        TpmKeyProvider tpm;
        if (tpm.CheckAvailability() == TpmStatus::Available && tpm.Connect() && !tpm.IsVirtualTPM()) {
            std::string tpmKey = tpm.GetEndorsementKeyPub();
            if (!tpmKey.empty() && tpmKey.find("ERROR") == std::string::npos) {
                return tpmKey;
            }
        }

        // 2. البديل HWID (في حال عدم وجود TPM)
        DeviceFingerprintExtractor hw;
        std::string hwid = hw.GenerateMachineID();
        if (hwid.find("ERR") == std::string::npos) {
            return hwid;
        }

        return ""; // فشل تام
    }
    catch (...) {
        return "ERROR_EXCEPTION_FINGERPRINT";
    }
}

// الداله العامة
std::string SecurityManager::GenerateHandshakeToken() {
    try {
        // 1. فحص الأمان الأساسي
        if (IsVirtualEnvironment()) {
            return "BLOCK:VM_DETECTED";
        }

        // 2. الحصول على البصمة الخام
        std::string rawFingerprint = GetRawDeviceFingerprint();
        if (rawFingerprint.empty()) {
            return "ERROR:DEVICE_ID_FAILED";
        }

        // التحقق من أخطاء البصمة
        if (rawFingerprint.find("ERROR") != std::string::npos) {
            return rawFingerprint;
        }

        // =================================================================
        // التعديل: استخدام الهاش (SHA256) لتقليل الحجم
        // =================================================================

        // أ) نحسب الهاش للبصمة (الناتج 32 بايت دائماً)
        std::vector<unsigned char> hashBytes = SecurityManager::CalculateSha256(rawFingerprint);

        // ب) نحول الهاش إلى نص Hex (الناتج 64 حرفاً)
        std::string fingerprintHashHex = SecurityManager::ToHex(hashBytes);

        // 4. إرجاع النتيجة
        return fingerprintHashHex;
    }
    catch (...) {
        return "ERROR:SECURITY_EXCEPTION";
    }
}

// نحسب الهاش للبصمة
Bytes SecurityManager::CalculateSha256(const std::string& input) {
    BCRYPT_ALG_HANDLE hAlg = NULL; BCRYPT_HASH_HANDLE hHash = NULL; Bytes hash(32);
    if (NT_SUCCESS(BCryptOpenAlgorithmProvider(&hAlg, BCRYPT_SHA256_ALGORITHM, NULL, 0))) {
        if (NT_SUCCESS(BCryptCreateHash(hAlg, &hHash, NULL, 0, NULL, 0, 0))) {
            BCryptHashData(hHash, (PUCHAR)input.data(), (ULONG)input.size(), 0);
            BCryptFinishHash(hHash, hash.data(), (ULONG)hash.size(), 0);
            BCryptDestroyHash(hHash);
        }
        BCryptCloseAlgorithmProvider(hAlg, 0);
    }
    return hash;
}

std::string SecurityManager::ToHex(const Bytes& data) {
    std::ostringstream ss; ss << std::hex << std::setfill('0');
    for (unsigned char c : data) ss << std::setw(2) << (int)c;
    return ss.str();
}