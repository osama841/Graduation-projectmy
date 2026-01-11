#include "pch.h"
#include "TpmKeyProvider.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include <algorithm>

TpmKeyProvider::TpmKeyProvider() : hContext(0), isConnected(false) {}
TpmKeyProvider::~TpmKeyProvider() { Disconnect(); }

TpmStatus TpmKeyProvider::CheckAvailability() {
    TBS_CONTEXT_PARAMS2 params; params.version = TBS_CONTEXT_VERSION_TWO; params.asUINT32 = 0; params.includeTpm12 = 0; params.includeTpm20 = 1;
    TBS_HCONTEXT tmp;
    TBS_RESULT res = Tbsi_Context_Create((PCTBS_CONTEXT_PARAMS)&params, &tmp);
    if (res == TBS_SUCCESS) { Tbsip_Context_Close(tmp); return TpmStatus::Available; }
    if (res == TBS_E_ACCESS_DENIED) return TpmStatus::AccessDenied;
    return TpmStatus::NotFound;
}

bool TpmKeyProvider::Connect() {
    if (isConnected) return true;
    TBS_CONTEXT_PARAMS2 params; params.version = TBS_CONTEXT_VERSION_TWO; params.asUINT32 = 0; params.includeTpm12 = 0; params.includeTpm20 = 1;
    if (Tbsi_Context_Create((PCTBS_CONTEXT_PARAMS)&params, &hContext) == TBS_SUCCESS) { isConnected = true; return true; }
    return false;
}

void TpmKeyProvider::Disconnect() {
    if (isConnected && hContext != 0) { Tbsip_Context_Close(hContext); hContext = 0; isConnected = false; }
}

std::string TpmKeyProvider::GetEndorsementKeyPub() {
    if (!isConnected) return "ERROR_Not_Connected";
    BYTE cmd[] = { 0x80, 0x01, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x01, 0x73, 0x81, 0x01, 0x00, 0x01 };
    BYTE resp[1024] = { 0 }; UINT32 len = sizeof(resp);
    TBS_RESULT res = Tbsip_Submit_Command(hContext, TBS_COMMAND_LOCALITY_ZERO, TBS_COMMAND_PRIORITY_NORMAL, cmd, sizeof(cmd), resp, &len);
    if (res != TBS_SUCCESS || resp[9] != 0x00) return "ERROR_Cmd_Failed"; // هذا الخطأ يعني Unprovisioned
    std::vector<BYTE> raw(resp + 10, resp + len);
    return BytesToHex(raw);
}

std::string TpmKeyProvider::GetTpmManufacturer() {
    if (!isConnected) return "Unknown";
    BYTE cmd[] = { 0x80, 0x01, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x01, 0x7A, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x02, 0x05, 0x00, 0x00, 0x00, 0x01 };
    BYTE resp[1024] = { 0 }; UINT32 len = sizeof(resp);
    if (Tbsip_Submit_Command(hContext, TBS_COMMAND_LOCALITY_ZERO, TBS_COMMAND_PRIORITY_NORMAL, cmd, sizeof(cmd), resp, &len) != TBS_SUCCESS) return "Error";
    return ParseManufacturerFromResponse(std::vector<BYTE>(resp, resp + len));
}

bool TpmKeyProvider::IsVirtualTPM() {
    std::string manu = GetTpmManufacturer();
    if (manu == "MSFT" || manu == "VMW" || manu == "BAR") return true;
    if (manu == "GENERIC_0") return false; // إصلاح: قبول الشريحة التي ترسل أصفاراً
    if (manu == "INTC" || manu == "AMD" || manu == "IFX" || manu == "NTC" || manu == "STM") return false;
    return true; // Unknown is treated as risk
}

std::string TpmKeyProvider::BytesToHex(const std::vector<BYTE>& data) {
    std::stringstream ss; ss << std::hex << std::setfill('0');
    for (BYTE b : data) ss << std::setw(2) << (int)b;
    return ss.str();
}

std::string TpmKeyProvider::ParseManufacturerFromResponse(const std::vector<BYTE>& resp) {
    if (resp.size() < 4) return "Unknown";
    size_t end = resp.size();
    bool isZero = true;
    for (size_t i = end - 4; i < end; i++) if (resp[i] != 0) isZero = false;
    if (isZero) return "GENERIC_0"; // إصلاح: تسمية الشريحة الفارغة

    std::string id = "";
    for (size_t i = end - 4; i < end; i++) if (resp[i] >= 32 && resp[i] <= 126) id += (char)resp[i];
    return id.empty() ? "UNKNOWN_HEX" : id;
}
