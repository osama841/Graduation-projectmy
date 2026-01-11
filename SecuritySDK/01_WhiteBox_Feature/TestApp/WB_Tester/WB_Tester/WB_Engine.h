#ifndef WB_ENGINE_H
#define WB_ENGINE_H

#include <vector>
#include <cstdint> // من أجل uint8_t

// فئة المحرك: وظيفتها الوحيدة فك التشفير
class WB_Engine
{
public:
    // الدالة الرئيسية: تأخذ بيانات مشفرة وتخرج بيانات نظيفة
    // Input: Blob (16 bytes or more)
    // Output: Decrypted Data
    static void Decrypt(const std::vector<uint8_t> &input, std::vector<uint8_t> &output);
};

#endif // WB_ENGINE_H