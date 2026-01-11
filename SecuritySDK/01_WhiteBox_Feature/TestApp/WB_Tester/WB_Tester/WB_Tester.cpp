#include <iostream>
#include <vector>
#include <iomanip>
#include "WB_Engine.h"      // الطبقة الثانية
#include "AntiDebug.h"     // الطبقة الثالثة
#include "AntiCapture.h"   // الطبقة الرابعة

using namespace std;

// دالة مساعدة لطباعة النتيجة
void PrintHex(const string& label, const vector<uint8_t>& data) {
    cout << label << ": ";
    for (auto b : data) {
        cout << "0x" << hex << uppercase << setw(2) << setfill('0') << (int)b << " ";
    }
    cout << endl;
}

int main() {
    // --- أولاً: تفعيل حماية منع التصوير (Anti-Capture) ---
    HWND hWnd = GetConsoleWindow();
    if (AntiCapture::ProtectWindow(hWnd)) {
        cout << "[+] Anti-Capture Protection Enabled (Black Screen Mode)" << endl;
    }
    else {
        cout << "[-] Failed to enable Anti-Capture." << endl;
    }

    cout << "=== SecureGate System: Full Protection Test ===" << endl;

    // --- ثانياً: تشغيل حارس النزاهة (Integrity Guard) ---
    IntegrityGuard guard;
    guard.Start();
    cout << "[+] Integrity Guard is active. (Running in background...)" << endl;

    // بيانات وهمية للاختبار (Frame Data)
    vector<uint8_t> encryptedData = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80 };
    vector<uint8_t> decryptedOutput;

    cout << "\n--- Monitoring System Activity ---\n";

    for (int i = 0; i < 10; i++) {
        // فحص التهديدات (هل هناك Debugger؟)
        if (IntegrityGuard::IsCompromised) {
            cout << "\a"; // صوت تنبيه
            cout << "!!! SECURITY ALERT: Debugger Detected! Corrupting output..." << endl;
            // في حالة الخطر، نقوم بإفراغ البيانات بدلاً من فك تشفيرها
            decryptedOutput.clear();
            break;
        }
        else {
            cout << "Frame [" << i + 1 << "]: Secure. Decrypting content..." << endl;
            WB_Engine::Decrypt(encryptedData, decryptedOutput);
            // PrintHex("Result", decryptedOutput); // اختيارياً لرؤية الأرقام
        }

        std::this_thread::sleep_for(std::chrono::seconds(1));
    }

    guard.Stop();
    cout << "\nTest Finished. SecureGate core remains protected." << endl;
    system("pause");
    return 0;
}