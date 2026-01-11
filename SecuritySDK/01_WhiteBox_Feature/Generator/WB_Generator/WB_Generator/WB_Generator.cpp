#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <iomanip>
#include <random>

using namespace std;

const string MASTER_KEY = "MY_FINAL_SECRET_KEY_2026";

// دالة دقيقة جداً لكتابة الجداول بالحجم الصحيح
void WriteTable(ofstream& file, string tableName, int size, int seedModifier) {
    seed_seq seed{ (int)MASTER_KEY[0], (int)tableName[0], size, seedModifier };
    mt19937 generator(seed);
    uniform_int_distribution<int> distribution(0, 255);

    // كتابة تعريف المصفوفة
    file << "const unsigned char " << tableName << "[" << size << "] = {\n    ";

    for (int i = 0; i < size; ++i) {
        int val = distribution(generator);
        file << "0x" << hex << uppercase << setfill('0') << setw(2) << val;

        // وضع فاصلة إذا لم نكن في آخر عنصر
        if (i < size - 1) {
            file << ", ";
            // سطر جديد كل 16 عنصر للترتيب
            if ((i + 1) % 16 == 0) file << "\n    ";
        }
    }
    file << "\n};\n\n";
}

int main() {
    cout << ">>> Generating FINAL Tables..." << endl;

    // سنحفظ الملف باسم جديد لتمييزه
    ofstream headerFile("Tables_Final.h");

    if (!headerFile.is_open()) {
        cerr << "Error creating file!" << endl;
        return 1;
    }

    headerFile << "#ifndef WB_TABLES_H\n";
    headerFile << "#define WB_TABLES_H\n\n";

    // 1. توليد الجولات (1024 عنصر)
    for (int i = 0; i < 10; ++i) {
        WriteTable(headerFile, "TBox_Round_" + to_string(i), 1024, i);
    }

    // 2. توليد جداول الخلط (256 عنصر)
    WriteTable(headerFile, "Input_Mixing", 256, 100);
    WriteTable(headerFile, "Output_Unmixing", 256, 200);

    headerFile << "#endif\n";
    headerFile.close();

    cout << "[SUCCESS] Created 'Tables_Final.h' successfully." << endl;
    cout << "Now COPY the content of this file to your Test Project." << endl;

    system("pause");
    return 0;
}