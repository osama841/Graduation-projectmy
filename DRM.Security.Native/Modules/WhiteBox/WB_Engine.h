#pragma once
#include <vector>
#include <cstdint>
#include <cstring>
#include "Tables.h"

namespace WB_Engine
{
    constexpr size_t BLOCK_SIZE = 16;

    // ═══════════════════════════════════════════════════════
    // WhiteBox AES - Simplified Implementation
    // يدمج المفتاح مع عمليات AES
    // ═══════════════════════════════════════════════════════

    // XOR مع المفتاح
    static void AddRoundKey(uint8_t* state, const uint8_t* key)
    {
        for (int i = 0; i < BLOCK_SIZE; ++i)
        {
            state[i] ^= key[i];
        }
    }

    // SubBytes باستخدام S-Box
    static void SubBytes(uint8_t* state)
    {
        for (int i = 0; i < BLOCK_SIZE; ++i)
        {
            state[i] = AES_SBOX[state[i]];
        }
    }

    // Inverse SubBytes
    static void InvSubBytes(uint8_t* state)
    {
        for (int i = 0; i < BLOCK_SIZE; ++i)
        {
            state[i] = AES_INV_SBOX[state[i]];
        }
    }

    // ShiftRows (simplified)
    static void ShiftRows(uint8_t* state)
    {
        uint8_t temp;
        
        // Row 1: shift left by 1
        temp = state[1];
        state[1] = state[5];
        state[5] = state[9];
        state[9] = state[13];
        state[13] = temp;
        
        // Row 2: shift left by 2
        temp = state[2];
        state[2] = state[10];
        state[10] = temp;
        temp = state[6];
        state[6] = state[14];
        state[14] = temp;
        
        // Row 3: shift left by 3
        temp = state[15];
        state[15] = state[11];
        state[11] = state[7];
        state[7] = state[3];
        state[3] = temp;
    }

    // Inverse ShiftRows
    static void InvShiftRows(uint8_t* state)
    {
        uint8_t temp;
        
        // Row 1: shift right by 1
        temp = state[13];
        state[13] = state[9];
        state[9] = state[5];
        state[5] = state[1];
        state[1] = temp;
        
        // Row 2: shift right by 2
        temp = state[2];
        state[2] = state[10];
        state[10] = temp;
        temp = state[6];
        state[6] = state[14];
        state[14] = temp;
        
        // Row 3: shift right by 3
        temp = state[3];
        state[3] = state[7];
        state[7] = state[11];
        state[11] = state[15];
        state[15] = temp;
    }

    // ═══════════════════════════════════════════════════════
    // تشفير block واحد (16 bytes) - Simplified WhiteBox AES
    // ═══════════════════════════════════════════════════════
    static void EncryptBlock(const uint8_t* input, uint8_t* output)
    {
        uint8_t state[16];
        std::memcpy(state, input, 16);
        
        // Round 0: AddRoundKey
        AddRoundKey(state, HIDDEN_KEY);
        
        // 3 Rounds (مبسّط بدلاً من 10)
        for (int round = 0; round < 3; ++round)
        {
            SubBytes(state);
            ShiftRows(state);
            AddRoundKey(state, HIDDEN_KEY);
        }
        
        std::memcpy(output, state, 16);
    }

    // ═══════════════════════════════════════════════════════
    // فك تشفير block واحد (16 bytes)
    // ═══════════════════════════════════════════════════════
    static void DecryptBlock(const uint8_t* input, uint8_t* output)
    {
        uint8_t state[16];
        std::memcpy(state, input, 16);
        
        // عكس العمليات
        for (int round = 0; round < 3; ++round)
        {
            AddRoundKey(state, HIDDEN_KEY);
            InvShiftRows(state);
            InvSubBytes(state);
        }
        
        AddRoundKey(state, HIDDEN_KEY);
        
        std::memcpy(output, state, 16);
    }

    // ═══════════════════════════════════════════════════════
    // دالة التشفير الكاملة
    // ═══════════════════════════════════════════════════════
    static void Encrypt(const std::vector<uint8_t>& input, std::vector<uint8_t>& output)
    {
        output.resize(input.size());
        
        for (size_t i = 0; i < input.size(); i += BLOCK_SIZE)
        {
            size_t remaining = input.size() - i;
            
            if (remaining >= BLOCK_SIZE)
            {
                EncryptBlock(&input[i], &output[i]);
            }
            else
            {
                // Padding للبيانات الأخيرة
                uint8_t temp_in[BLOCK_SIZE] = {0};
                uint8_t temp_out[BLOCK_SIZE] = {0};
                
                std::memcpy(temp_in, &input[i], remaining);
                EncryptBlock(temp_in, temp_out);
                std::memcpy(&output[i], temp_out, remaining);
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    // دالة فك التشفير الكاملة
    // ═══════════════════════════════════════════════════════
    static void Decrypt(const std::vector<uint8_t>& input, std::vector<uint8_t>& output)
    {
        output.resize(input.size());
        
        for (size_t i = 0; i < input.size(); i += BLOCK_SIZE)
        {
            size_t remaining = input.size() - i;
            
            if (remaining >= BLOCK_SIZE)
            {
                DecryptBlock(&input[i], &output[i]);
            }
            else
            {
                uint8_t temp_in[BLOCK_SIZE] = {0};
                uint8_t temp_out[BLOCK_SIZE] = {0};
                
                std::memcpy(temp_in, &input[i], remaining);
                DecryptBlock(temp_in, temp_out);
                std::memcpy(&output[i], temp_out, remaining);
            }
        }
    }
}
