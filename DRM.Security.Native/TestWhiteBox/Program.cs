using System;
using System.Runtime.InteropServices;

class Program
{
    // استدعاء دوال WhiteBox من DLL
    [DllImport("DRM.Security.Native.dll")]
    private static extern void WB_Encrypt_Video(byte[] data, int dataLength, byte[] output);

    [DllImport("DRM.Security.Native.dll")]
    private static extern void WB_Decrypt_Video(byte[] data, int dataLength, byte[] output);

    static void Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   WhiteBox Cryptography Test");
        Console.WriteLine("========================================\n");

        // بيانات تجريبية (16 bytes)
        byte[] testData = new byte[16]
        {
            0x00, 0x11, 0x22, 0x33,
            0x44, 0x55, 0x66, 0x77,
            0x88, 0x99, 0xAA, 0xBB,
            0xCC, 0xDD, 0xEE, 0xFF
        };

        byte[] encrypted = new byte[16];
        byte[] decrypted = new byte[16];

        // طباعة البيانات الأصلية
        Console.WriteLine("Original Data:");
        PrintBytes(testData);

        // تشفير
        Console.WriteLine("\nEncrypting with WhiteBox...");
        WB_Encrypt_Video(testData, 16, encrypted);

        Console.WriteLine("Encrypted Data:");
        PrintBytes(encrypted);

        // فك التشفير
        Console.WriteLine("\nDecrypting with WhiteBox...");
        WB_Decrypt_Video(encrypted, 16, decrypted);

        Console.WriteLine("Decrypted Data:");
        PrintBytes(decrypted);

        // التحقق
        bool success = true;
        for (int i = 0; i < 16; i++)
        {
            if (testData[i] != decrypted[i])
            {
                success = false;
                break;
            }
        }

        Console.WriteLine("\n========================================");
        if (success)
        {
            Console.WriteLine("   ✅ TEST PASSED!");
            Console.WriteLine("   WhiteBox is working correctly!");
        }
        else
        {
            Console.WriteLine("   ❌ TEST FAILED!");
        }
        Console.WriteLine("========================================");

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void PrintBytes(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            Console.Write($"{data[i]:X2} ");
            if ((i + 1) % 4 == 0) Console.Write(" ");
        }
        Console.WriteLine();
    }
}
