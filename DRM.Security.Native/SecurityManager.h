#pragma once
#include <string>
#include <vector>
#include <windows.h>

// تعريف نوع البيانات (مصفوفة بايتات)
typedef std::vector<unsigned char> Bytes;
class SecurityManager {
public:
    SecurityManager();
    ~SecurityManager();

    
    static std::string GenerateHandshakeToken();

private:
    // دالة داخلية للحصول على بصمة الجهاز الخام (TPM أو HWID)
    static  std::string GetRawDeviceFingerprint();

    // دالة لفحص البيئة الوهمية
    static  bool IsVirtualEnvironment();

    // نحسب الهاش للبصمة
    static   Bytes CalculateSha256(const std::string& input);
    //نحول الهاش إلى نص
    static  std::string ToHex(const Bytes& data);


};
