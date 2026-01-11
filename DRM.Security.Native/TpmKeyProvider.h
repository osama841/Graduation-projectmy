#pragma once
#include <windows.h>
#include <tbs.h>
#include <string>
#include <vector>

#pragma comment(lib, "tbs.lib")

enum class TpmStatus { Available, NotFound, AccessDenied, Error };

class TpmKeyProvider {
public:
    TpmKeyProvider();
    ~TpmKeyProvider();
    TpmStatus CheckAvailability();
    bool Connect();
    std::string GetEndorsementKeyPub();
    std::string GetTpmManufacturer();
    bool IsVirtualTPM();
    void Disconnect();

private:
    TBS_HCONTEXT hContext;
    bool isConnected;
    std::string BytesToHex(const std::vector<BYTE>& data);
    std::string ParseManufacturerFromResponse(const std::vector<BYTE>& response);
};