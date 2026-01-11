#pragma once
#include <string>

class DeviceFingerprintExtractor {
public:
    DeviceFingerprintExtractor();

    // الدالة الرئيسية
    std::string GenerateMachineID();

private:
    // دوال داخلية
    std::string GetCpuSiliconDNA();
    std::string GetHddSerial_ATA();
    std::string GetBiosSerialNumber();

    // دوال مساعدة
    std::string TrimString(const std::string& str);
    std::string SwapChars(const char* data, int length);

    // SMBIOS utils
    struct RawSMBIOSData;
    struct SMBIOSHeader;
    const char* GetSmbiosString(SMBIOSHeader* header, int stringIndex);
};


