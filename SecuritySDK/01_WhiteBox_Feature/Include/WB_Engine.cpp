#include "WB_Engine.h"
#include "Tables.h" // <--- هنا السر! نستدعي الملف الذي ولدته أنت
#include <iostream>

// دالة مساعدة لمحاكاة عملية "Look-up" من الجداول
// في الحقيقة، خوارزمية Chow's Algorithm أعقد من هذا،
// لكننا هنا نستخدم الجداول التي ولدناها لإثبات المفهوم.
uint8_t Lookup(const unsigned char *table, int index)
{
    // نأخذ قيمة من الجدول بناءً على المدخل
    // ونستخدم عملية XOR بسيطة لمحاكاة فك التشفير
    return table[index % 1024] ^ (index % 255);
}

void WB_Engine::Decrypt(const std::vector<uint8_t> &input, std::vector<uint8_t> &output)
{
    output.clear();

    // يجب أن تكون البيانات من مضاعفات 16 بايت (AES Block Size)
    // للتبسيط، سنفك أول 16 بايت فقط في هذا المثال
    int blockSize = 16;
    if (input.size() < blockSize)
        return;

    std::vector<uint8_t> state(blockSize);

    // 1. مرحلة خلط المدخلات (Input Mixing)
    for (int i = 0; i < blockSize; i++)
    {
        // نستخدم جدول Input_Mixing لإخفاء البيانات قبل دخولها المحرك
        state[i] = Input_Mixing[input[i] ^ 0xAA]; // 0xAA هو تمويه بسيط
    }

    // 2. مراحل الجولات (Rounds) - تشبه AES
    // نمرر البيانات عبر الـ 10 جولات التي ولدنا جداولها
    for (int r = 0; r < 10; r++)
    {
        // اختيار الجدول المناسب للجولة الحالية
        const unsigned char *currentTable;

        switch (r)
        {
        case 0:
            currentTable = TBox_Round_0;
            break;
        case 1:
            currentTable = TBox_Round_1;
            break;
        case 2:
            currentTable = TBox_Round_2;
            break;
        case 3:
            currentTable = TBox_Round_3;
            break;
        case 4:
            currentTable = TBox_Round_4;
            break;
        case 5:
            currentTable = TBox_Round_5;
            break;
        case 6:
            currentTable = TBox_Round_6;
            break;
        case 7:
            currentTable = TBox_Round_7;
            break;
        case 8:
            currentTable = TBox_Round_8;
            break;
        case 9:
            currentTable = TBox_Round_9;
            break;
        default:
            currentTable = TBox_Round_0;
        }

        // تحويل البيانات باستخدام الجدول
        for (int i = 0; i < blockSize; i++)
        {
            state[i] = Lookup(currentTable, state[i]);
        }
    }

    // 3. مرحلة فك الخلط النهائي (Output Unmixing)
    for (int i = 0; i < blockSize; i++)
    {
        uint8_t val = Output_Unmixing[state[i]];
        output.push_back(val);
    }
}