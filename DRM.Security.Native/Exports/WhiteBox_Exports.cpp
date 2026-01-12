#include "../Modules/WhiteBox/WB_Engine.h"
#include <cstdint>
#include <cstring>
#include <vector>

extern "C" __declspec(dllexport)
void WB_Encrypt_Video(uint8_t* data, int dataLength, uint8_t* output)
{
    std::vector<uint8_t> input(data, data + dataLength);
    std::vector<uint8_t> outputVec;
    
    WB_Engine::Encrypt(input, outputVec);
    
    std::memcpy(output, outputVec.data(), outputVec.size());
}

extern "C" __declspec(dllexport)
void WB_Decrypt_Video(uint8_t* data, int dataLength, uint8_t* output)
{
    std::vector<uint8_t> input(data, data + dataLength);
    std::vector<uint8_t> outputVec;
    
    WB_Engine::Decrypt(input, outputVec);
    
    std::memcpy(output, outputVec.data(), outputVec.size());
}
