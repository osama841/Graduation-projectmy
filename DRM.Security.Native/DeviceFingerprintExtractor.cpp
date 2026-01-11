#include "pch.h"
#include "DeviceFingerprintExtractor.h"
#include <windows.h>
#include <winioctl.h>
#include <intrin.h>
#include <algorithm>
#include <vector>
#include <sstream>
#include <iomanip>
#include <iostream>

// =============================================================
// ATA Definitions (Fix)
// =============================================================
#ifndef IOCTL_SCSI_BASE
#define IOCTL_SCSI_BASE 0x00000004
#endif

#ifndef IOCTL_ATA_PASS_THROUGH
#define IOCTL_ATA_PASS_THROUGH CTL_CODE(IOCTL_SCSI_BASE, 0x040b, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#endif

#ifndef ATA_FLAGS_DATA_IN
#define ATA_FLAGS_DATA_IN 0x02
#endif

// =============================================================
// Constructor (لا يستخدم شيء حالياً)
// =============================================================
DeviceFingerprintExtractor::DeviceFingerprintExtractor() {}

// =============================================================
// Utils
// =============================================================
std::string DeviceFingerprintExtractor::TrimString(const std::string& str) {
    std::string s = str;
    s.erase(std::remove_if(s.begin(), s.end(), ::isspace), s.end());
    s.erase(std::remove_if(s.begin(), s.end(),
        [](unsigned char c) { return c < 32 || c > 126; }),
        s.end());
    return s;
}

std::string DeviceFingerprintExtractor::SwapChars(const char* data, int length) {
    std::string result = "";
    for (int i = 0; i < length; i += 2) {
        if (i + 1 < length) {
            result += data[i + 1];
            result += data[i];
        }
    }
    return TrimString(result);
}

// =============================================================
// CPU Silicon DNA
// =============================================================
std::string DeviceFingerprintExtractor::GetCpuSiliconDNA() {
    int cpuInfo[4] = { 0 };
    std::stringstream ss;

    for (int i = 0; i <= 3; i++) {
        __cpuid(cpuInfo, i);
        ss << std::hex << std::setw(8) << std::setfill('0') << cpuInfo[0];
        ss << std::hex << std::setw(8) << std::setfill('0') << cpuInfo[3];
    }

    for (unsigned int i = 0x80000000; i <= 0x80000004; i++) {
        __cpuid(cpuInfo, i);
        ss << std::hex << std::setw(8) << std::setfill('0') << cpuInfo[0];
        ss << std::hex << std::setw(8) << std::setfill('0') << cpuInfo[1];
        ss << std::hex << std::setw(8) << std::setfill('0') << cpuInfo[2];
        ss << std::hex << std::setw(8) << std::setfill('0') << cpuInfo[3];
    }

    return TrimString(ss.str());
}

// =============================================================
// HDD Serial (ATA Pass Through)
// =============================================================
typedef struct _ATA_PASS_THROUGH_EX_INTERNAL {
    USHORT Length;
    USHORT AtaFlags;
    UCHAR PathId;
    UCHAR TargetId;
    UCHAR Lun;
    UCHAR ReservedAsUchar;
    ULONG DataTransferLength;
    ULONG TimeOutValue;
    ULONG ReservedAsUlong;
    ULONG_PTR DataBufferOffset;
    UCHAR PreviousTaskFile[8];
    UCHAR CurrentTaskFile[8];
} ATA_PASS_THROUGH_EX_INTERNAL;

std::string DeviceFingerprintExtractor::GetHddSerial_ATA() {
    try {
        HANDLE hDrive = CreateFileA("\\\\.\\PhysicalDrive0",
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            NULL, OPEN_EXISTING, 0, NULL);

        if (hDrive == INVALID_HANDLE_VALUE) {
            // حاول بدون GENERIC_WRITE
            hDrive = CreateFileA("\\\\.\\PhysicalDrive0",
                GENERIC_READ,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                NULL, OPEN_EXISTING, 0, NULL);
            
            if (hDrive == INVALID_HANDLE_VALUE) {
                return "HDD_NO_ACCESS";
            }
        }

        const int dataSize = 512;
        const int bufferSize = sizeof(ATA_PASS_THROUGH_EX_INTERNAL) + dataSize;

        std::vector<char> buffer(bufferSize, 0);

        auto* pATA = (ATA_PASS_THROUGH_EX_INTERNAL*)buffer.data();
        pATA->Length = sizeof(ATA_PASS_THROUGH_EX_INTERNAL);
        pATA->AtaFlags = ATA_FLAGS_DATA_IN;
        pATA->DataTransferLength = dataSize;
        pATA->TimeOutValue = 3;
        pATA->DataBufferOffset = sizeof(ATA_PASS_THROUGH_EX_INTERNAL);

        pATA->CurrentTaskFile[6] = 0xEC; // Identify Device

        DWORD bytesReturned = 0;
        BOOL status = DeviceIoControl(hDrive,
            IOCTL_ATA_PASS_THROUGH,
            buffer.data(), bufferSize,
            buffer.data(), bufferSize,
            &bytesReturned, NULL);

        CloseHandle(hDrive);

        if (!status)
            return "HDD_ATA_FAILED";

        char* dataBuffer = buffer.data() + sizeof(ATA_PASS_THROUGH_EX_INTERNAL);

        std::string serial = SwapChars(dataBuffer + 20, 20);

        if (serial.empty()) return "HDD_EMPTY";
        return serial;
    }
    catch (...) {
        return "HDD_ERROR";
    }
}

// =============================================================
// SMBIOS structures
// =============================================================
struct DeviceFingerprintExtractor::RawSMBIOSData {
    BYTE Used20CallingMethod;
    BYTE SMBIOSMajorVersion;
    BYTE SMBIOSMinorVersion;
    BYTE DmiRevision;
    DWORD Length;
    BYTE SMBIOSTableData[];
};

struct DeviceFingerprintExtractor::SMBIOSHeader {
    BYTE Type;
    BYTE Length;
    WORD Handle;
};

const char* DeviceFingerprintExtractor::GetSmbiosString(SMBIOSHeader* header, int stringIndex) {
    if (stringIndex == 0) return nullptr;

    const char* start = (const char*)header + header->Length;

    for (int i = 1; i < stringIndex; i++) {
        while (*start != 0) start++;
        start++;
    }

    return start;
}

// =============================================================
// BIOS Serial
// =============================================================
std::string DeviceFingerprintExtractor::GetBiosSerialNumber() {
    try {
        DWORD signature = 'RSMB';
        DWORD size = GetSystemFirmwareTable(signature, 0, NULL, 0);

        if (size == 0) return "BIOS_NO_TABLE";

        std::vector<BYTE> buffer(size);
        GetSystemFirmwareTable(signature, 0, buffer.data(), size);

        RawSMBIOSData* smbios = (RawSMBIOSData*)buffer.data();
        BYTE* dataStart = smbios->SMBIOSTableData;
        BYTE* dataEnd = buffer.data() + size;

        BYTE* current = dataStart;

        while (current < dataEnd) {
            SMBIOSHeader* header = (SMBIOSHeader*)current;

            if (header->Type == 1) {
                if (header->Length >= 8) {
                    int serialIndex = current[7];
                    const char* serialStr = GetSmbiosString(header, serialIndex);
                    if (serialStr && strlen(serialStr) > 0)
                        return TrimString(serialStr);
                }
            }

            BYTE* next = current + header->Length;
            while (next < dataEnd - 1 && (next[0] != 0 || next[1] != 0))
                next++;

            current = next + 2;
        }

        return "BIOS_DEFAULT";
    }
    catch (...) {
        return "BIOS_ERROR";
    }
}

// =============================================================
// Main Entry — Combine All Data
// =============================================================
std::string DeviceFingerprintExtractor::GenerateMachineID() {

    std::string cpu = GetCpuSiliconDNA();
    std::string hdd = GetHddSerial_ATA();
    std::string bios = GetBiosSerialNumber();

    return cpu + hdd + bios;
}




