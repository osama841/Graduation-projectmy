#include "pch.h"
#include "VmEnvironmentDetector.h"
#include <winsock2.h>
#include <ws2tcpip.h>
#include <iphlpapi.h>
#include <psapi.h>
#include <setupapi.h>
#include <wbemidl.h>
#include <comdef.h>
#include <intrin.h>
#include <algorithm>
#include <vector>
#include <sstream>
#include <iostream>
#include <numeric>
#include <cmath>
#include <tchar.h>
#include <strsafe.h>
#include <intrin.h>
#include <windows.h>


// DXGI for GPU vendor
#include <dxgi.h>

// Linker libs
#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "wbemuuid.lib")
#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "setupapi.lib")
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "ole32.lib")

// Some NT Query prototypes loaded dynamically to avoid static link dependency
typedef LONG NTSTATUS;
typedef NTSTATUS(WINAPI* NtQuerySystemInformation_t)(ULONG, PVOID, ULONG, PULONG);

static NtQuerySystemInformation_t GetNtQuerySystemInformation() {
    HMODULE hNtdll = GetModuleHandleW(L"ntdll.dll");
    if (!hNtdll) return nullptr;
    return (NtQuerySystemInformation_t)GetProcAddress(hNtdll, "NtQuerySystemInformation");
}

VmEnvironmentDetector::VmEnvironmentDetector() : m_totalScore(0), m_reportLog("") {
    // Initialize COM for WMI
    HRESULT hr = CoInitializeEx(0, COINIT_APARTMENTTHREADED);
    if (SUCCEEDED(hr)) {
        CoInitializeSecurity(NULL, -1, NULL, NULL, RPC_C_AUTHN_LEVEL_DEFAULT,
            RPC_C_IMP_LEVEL_IMPERSONATE, NULL, EOAC_NONE, NULL);
    }
    // WSA
    WSADATA wsa;
    WSAStartup(MAKEWORD(2, 2), &wsa);
}

VmEnvironmentDetector::~VmEnvironmentDetector() {
    WSACleanup();
    CoUninitialize();
}

void VmEnvironmentDetector::AddLog(const std::string& message) {
    m_reportLog += message + "\n";
}

void VmEnvironmentDetector::SafeSleep(DWORD ms) {
    Sleep(ms);
}
int VmEnvironmentDetector::RunScan() {
    m_totalScore = 0;
    m_reportLog.clear();

    // مؤشرات قوية على العتاد الحقيقي
    bool isTpmReal = false;
    bool isGpuRealHardware = false;

    AddLog("=== VmEnvironmentDetector Full Scan Started ===");

    // 1. Anti-analysis (Debuggers/Sandbox)
    // هذه نبقيها لأنها تدل على تلاعب حتى لو الجهاز حقيقي
    if (CheckDebuggerPresent()) {
        AddLog("[ALERT] Debugger detected");
        m_totalScore += 40;
    }
    if (CheckSandboxArtifacts()) {
        AddLog("[ALERT] Sandbox artifacts detected");
        m_totalScore += 30;
    }

    // 2. TPM Check (الدليل الأقوى)
    std::string tpmInfo;
    int tpmStatus = CheckTpmStatusWmi(tpmInfo);
    if (tpmStatus == 1) {
        AddLog("[CRITICAL] TPM: Virtual TPM Detected: " + tpmInfo);
        m_totalScore += WEIGHT_TPM;
    }
    else if (tpmStatus == 0) {
        isTpmReal = true; // تم تأكيد الجهاز الحقيقي
        AddLog("[+] TPM: Real TPM: " + tpmInfo);
    }
    else {
        AddLog("[-] TPM: Not Available or Access Denied");
    }

    // 3. GPU Vendor (الدليل الثاني القوي)
    std::string gpuInfo;
    if (CheckGpuVendor(gpuInfo)) {
        AddLog("[!] GPU vendor suspicious: " + gpuInfo);
        m_totalScore += WEIGHT_GPU;
    }
    else {
        isGpuRealHardware = true; // تم تأكيد الجهاز الحقيقي
        AddLog("[+] GPU vendor OK: " + gpuInfo);
    }

    // متغير مساعد: هل نمتلك دليلاً قوياً على أن الجهاز حقيقي؟
    bool hasStrongHardwareEvidence = (isTpmReal || isGpuRealHardware);

    // 4. CPUID Hypervisor
    std::string cpuid = CheckCpuidHypervisor();
    if (cpuid != "Clean") {
        if (hasStrongHardwareEvidence && cpuid.find("Microsoft Hv") != std::string::npos) {
            AddLog("[OK] CPUID: 'Microsoft Hv' ignored (Core Isolation/Real Hardware Confirmed)");
        }
        else if (hasStrongHardwareEvidence) {
            AddLog("[~] CPUID: Hypervisor detected but ignored due to Real Hardware: " + cpuid);
        }
        else {
            AddLog("[!] CPUID hypervisor vendor: " + cpuid);
            m_totalScore += WEIGHT_CPUID;
        }
    }
    else {
        AddLog("[+] CPUID: Clean");
    }

    // 5. RDTSC Jitter
    double rdstddev = 0.0; unsigned long rdmedian = 0;
    if (CheckRDTSCJitter(rdstddev, rdmedian)) {
        std::ostringstream ss; ss << rdstddev;
        AddLog("[*] RDTSC jitter stddev: " + ss.str());

        // تجاهل التذبذب البسيط إذا كان العتاد حقيقياً
        if (rdstddev > 100.0 && !hasStrongHardwareEvidence) {
            AddLog("[!] RDTSC jitter indicates VM");
            m_totalScore += WEIGHT_RDTSC_JITTER;
        }
    }

    // 6. Memory Latency
    unsigned long memMs = 0;
    if (CheckMemoryLatencyBenchmark(memMs)) {
        AddLog("[*] Memory memcpy 100MB took: " + std::to_string(memMs) + " ms");
        if (memMs > 600 && !hasStrongHardwareEvidence) {
            AddLog("[!] Memory latency higher than expected -> VM likely");
            m_totalScore += WEIGHT_MEMORY;
        }
    }

    // 7. MAC Address
    if (CheckMacAddressArtifacts()) {
        if (hasStrongHardwareEvidence) {
            AddLog("[~] MAC OUI suspicious, but ignored due to Real Hardware Evidence.");
        }
        else {
            AddLog("[!] MAC OUI suspicious");
            m_totalScore += WEIGHT_MAC;
        }
    }
    else {
        AddLog("[+] MAC OUI clean");
    }

    // 8. Drivers and Services
    if (CheckVmServicesOrDrivers()) {
        if (hasStrongHardwareEvidence) {
            AddLog("[~] VM-specific drivers detected but ignored (Likely leftovers or VBS).");
        }
        else {
            AddLog("[!] VM-specific drivers/services detected");
            m_totalScore += WEIGHT_DRIVERS;
        }
    }

    // 9. PnP Devices
    if (CheckPnPDevices()) {
        if (hasStrongHardwareEvidence) {
            AddLog("[~] Virtual PnP devices found but ignored (Likely Hyper-V/WSL components).");
        }
        else {
            AddLog("[!] Virtual PnP devices found");
            m_totalScore += WEIGHT_PNP;
        }
    }

    // 10. Hyper-V Root Partition (المشكلة الرئيسية كانت هنا)
    if (CheckHypervRootPartition()) {
        if (hasStrongHardwareEvidence) {
            AddLog("[OK] Hyper-V root partition detected but ignored (Core Isolation/WSL2 Active).");
            // لا نضيف أي نقاط هنا لأننا تأكدنا من العتاد
        }
        else {
            AddLog("[!] Hyper-V root partition detected (No HW Evidence)");
            m_totalScore += 30;
        }
    }

    // 11. Network Latency
    unsigned long netlat = CheckNetworkLatencyToPublic();
    if (netlat == ULONG_MAX) {
        AddLog("[-] Network connection failed");
    }
    else {
        AddLog("[*] Network latency to public: " + std::to_string(netlat) + " ms");
        if (netlat > 2000 && !hasStrongHardwareEvidence) {
            // نتجاهل بطء الشبكة إذا كان العتاد حقيقياً
            AddLog("[!] High network latency");
            m_totalScore += WEIGHT_NETWORK;
        }
    }

    // بقية الفحوصات الثانوية (SMBIOS, ACPI)
    // يمكن تركها كما هي أو تطبيق نفس المنطق (hasStrongHardwareEvidence) عليها

    std::string acpiInfo;
    if (CheckAcpiTablesForVmSignatures(acpiInfo)) {
        if (!hasStrongHardwareEvidence) {
            AddLog("[!] ACPI: VM signature found: " + acpiInfo);
            m_totalScore += WEIGHT_ACPI;
        }
        else {
            AddLog("[~] ACPI signature ignored due to HW evidence: " + acpiInfo);
        }
    }

    std::string smbiosInfo;
    if (CheckSmbiosViaWmi(smbiosInfo)) {
        if (!hasStrongHardwareEvidence) {
            AddLog("[!] SMBIOS WMI suspicious: " + smbiosInfo);
            m_totalScore += WEIGHT_SMBIOS;
        }
    }

    AddLog("--- Final Score: " + std::to_string(m_totalScore) + " ---");
    AddLog("=== VmEnvironmentDetector Full Scan Finished ===");

    return m_totalScore;
}


std::string VmEnvironmentDetector::GetDetailedReport() const {
    return m_reportLog;
}

bool VmEnvironmentDetector::IsVirtualMachine() const {
    return m_totalScore >= SCORE_THRESHOLD_STRONG;
}

// ------------------ Implementation of detectors ------------------

std::string VmEnvironmentDetector::CheckCpuidHypervisor() {
    int cpuInfo[4] = { 0 };
    __cpuid(cpuInfo, 1);
    bool hypervisor = ((cpuInfo[2] >> 31) & 1) != 0;
    if (!hypervisor) return "Clean";
    __cpuid(cpuInfo, 0x40000000);
    char vendor[13] = { 0 };
    memcpy(vendor, &cpuInfo[1], 4);
    memcpy(vendor + 4, &cpuInfo[2], 4);
    memcpy(vendor + 8, &cpuInfo[3], 4);
    std::string vstr(vendor);
    if (vstr.empty()) return "Hypervisor";
    return vstr;
}

bool VmEnvironmentDetector::CheckMacAddressArtifacts() {
    ULONG outBufLen = 15000;
    std::vector<BYTE> buf(outBufLen);
    PIP_ADAPTER_ADDRESSES adapters = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buf.data());

    DWORD rv = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_SKIP_ANYCAST, NULL, adapters, &outBufLen);
    if (rv == ERROR_BUFFER_OVERFLOW) {
        buf.resize(outBufLen);
        adapters = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buf.data());
        rv = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_SKIP_ANYCAST, NULL, adapters, &outBufLen);
    }
    if (rv != NO_ERROR) return false;

    const std::vector<std::vector<BYTE>> suspiciousOUI = {
        {0x08,0x00,0x27}, {0x00,0x05,0x69}, {0x00,0x0C,0x29}, {0x00,0x50,0x56},
        {0x00,0x15,0x5D}, {0x52,0x54,0x00}, {0x00,0x1C,0x42}
    };

    for (PIP_ADAPTER_ADDRESSES cur = adapters; cur != NULL; cur = cur->Next) {
        if (cur->PhysicalAddressLength >= 3) {
            for (const auto& oui : suspiciousOUI) {
                if (cur->PhysicalAddress[0] == oui[0] &&
                    cur->PhysicalAddress[1] == oui[1] &&
                    cur->PhysicalAddress[2] == oui[2]) {
                    return true;
                }
            }
        }
    }
    return false;
}

unsigned long VmEnvironmentDetector::CheckTimingAttackMedian() {
    // deprecated in favor of CheckRDTSCJitter and Memory Benchmark
    return 0;
}

bool VmEnvironmentDetector::CheckSmbiosViaWmi(std::string& outInfo) {
    bool found = WmiQuery(L"ROOT\\CIMV2", L"SELECT Manufacturer, Model FROM Win32_ComputerSystem", L"Manufacturer", outInfo);
    if (!found) { outInfo = "Unknown"; return false; }
    std::string low = outInfo;
    std::transform(low.begin(), low.end(), low.begin(), ::tolower);
    if (low.find("virtual") != std::string::npos || low.find("vmware") != std::string::npos ||
        low.find("innotek") != std::string::npos || low.find("xen") != std::string::npos ||
        low.find("qemu") != std::string::npos) {
        return true;
    }
    return false;
}

int VmEnvironmentDetector::CheckTpmStatusWmi(std::string& tpmInfo) {
    std::string manuIdStr;
    bool found = WmiQuery(L"ROOT\\CIMV2\\Security\\MicrosoftTpm",
        L"SELECT ManufacturerId FROM Win32_Tpm", L"ManufacturerId", manuIdStr);

    if (!found) { tpmInfo = "Not Found / Access Denied"; return 2; }

    unsigned long manuId = 0;
    try { manuId = std::stoul(manuIdStr); }
    catch (...) { return 2; }
    tpmInfo = "ID:" + manuIdStr;

    switch (manuId) {
    case 1297303124: tpmInfo += " (Microsoft vTPM)"; return 1;
    case 1447775575: tpmInfo += " (VMware vTPM)"; return 1;
    case 1196379975: tpmInfo += " (Google Cloud vTPM)"; return 1;
    case 1464156928: tpmInfo += " (Xen vTPM)"; return 1;
    case 1111572803: tpmInfo += " (QEMU/SWTPM)"; return 1;

        // Real Hardware List
    case 1229870147: tpmInfo += " (Intel PTT)"; return 0;
    case 1229346816: tpmInfo += " (Intel PTT)"; return 0;
    case 1094930500: tpmInfo += " (AMD fTPM)"; return 0;
    case 1229081856: tpmInfo += " (Infineon)"; return 0;
    case 1398033696: tpmInfo += " (STMicro)"; return 0;
    case 1314145024: tpmInfo += " (Nuvoton)"; return 0;
    case 1096043852: tpmInfo += " (Atmel)"; return 0;
    case 1314073888: tpmInfo += " (Nationz)"; return 0;
    case 1112687437: tpmInfo += " (Broadcom)"; return 0;
    case 1397576527: tpmInfo += " (Sinosun)"; return 0;
    case 1464025856: tpmInfo += " (Winbond)"; return 0;
    case 1213220096: tpmInfo += " (HP Enterprise)"; return 0;
    default: tpmInfo += " (Unknown - assumed real)"; return 0;
    }
}

bool VmEnvironmentDetector::CheckVmServicesOrDrivers() {
    // قمت بإزالة "vmbus" من القائمة لأنها موجودة في Windows 11 الحقيقي
    std::vector<std::string> blacklist = {
        "vboxservice","vboxsvc","vmtools","vmtoolsd","vmhgfs","vmware","vmmouse","vboxguest",
        "vm3dmp","vmmouse","hgfs","vmci","vmsrvc","vmusrvc","vm3d"

    };



    std::vector<LPVOID> drivers(4096);
    DWORD cbNeeded = 0;
    if (EnumDeviceDrivers(drivers.data(), (DWORD)(drivers.size() * sizeof(LPVOID)), &cbNeeded)) {
        size_t count = cbNeeded / sizeof(LPVOID);
        char name[MAX_PATH];
        for (size_t i = 0; i < count; ++i) {
            if (GetDeviceDriverBaseNameA(drivers[i], name, MAX_PATH)) {
                std::string s(name);
                std::transform(s.begin(), s.end(), s.begin(), ::tolower);
                for (auto& b : blacklist) {
                    if (s.find(b) != std::string::npos) return true;
                }
            }
        }
    }
    return false;
}

bool VmEnvironmentDetector::CheckPnPDevices() {
    HDEVINFO hDevInfo = SetupDiGetClassDevs(NULL, NULL, NULL, DIGCF_ALLCLASSES | DIGCF_PRESENT);
    if (hDevInfo == INVALID_HANDLE_VALUE) return false;

    SP_DEVINFO_DATA deviceInfoData;
    deviceInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
    bool found = false;

    for (DWORD i = 0; SetupDiEnumDeviceInfo(hDevInfo, i, &deviceInfoData); i++) {
        char buf[1024];
        if (SetupDiGetDeviceInstanceIdA(hDevInfo, &deviceInfoData, buf, sizeof(buf), NULL)) {
            std::string id(buf);
            std::transform(id.begin(), id.end(), id.begin(), ::tolower);
            if (id.find("vmware") != std::string::npos || id.find("vbox") != std::string::npos ||
                id.find("qemu") != std::string::npos || id.find("ven_80ee") != std::string::npos ||
                id.find("ven_15ad") != std::string::npos || id.find("ven_1af4") != std::string::npos) {
                found = true; break;
            }
        }
    }
    SetupDiDestroyDeviceInfoList(hDevInfo);
    return found;
}

unsigned long VmEnvironmentDetector::CheckNetworkLatencyToPublic(const std::string& host, int port) {
    struct addrinfo hints = {}, * res = NULL;
    hints.ai_family = AF_UNSPEC; hints.ai_socktype = SOCK_STREAM;
    if (getaddrinfo(host.c_str(), std::to_string(port).c_str(), &hints, &res) != 0) return 0;
    SOCKET s = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    if (s == INVALID_SOCKET) { freeaddrinfo(res); return 0; }
    u_long mode = 1; ioctlsocket(s, FIONBIO, &mode);
    DWORD start = GetTickCount();
    connect(s, res->ai_addr, (int)res->ai_addrlen);
    fd_set fdwrite; FD_ZERO(&fdwrite); FD_SET(s, &fdwrite);
    timeval timeout; timeout.tv_sec = 1; timeout.tv_usec = 0;
    int ret = select(0, NULL, &fdwrite, NULL, &timeout);
    unsigned long latency = GetTickCount() - start;
    closesocket(s); freeaddrinfo(res);
    if (ret > 0) return latency;
    return ULONG_MAX;
}

bool VmEnvironmentDetector::WmiQuery(const std::wstring& namespacePath, const std::wstring& query, const std::wstring& propertyName, std::string& resultResult) {
    IWbemLocator* pLoc = nullptr; IWbemServices* pSvc = nullptr; IEnumWbemClassObject* pEnumerator = nullptr;
    bool success = false;
    HRESULT hr = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER, IID_IWbemLocator, (LPVOID*)&pLoc);
    if (FAILED(hr)) return false;
    hr = pLoc->ConnectServer(_bstr_t(namespacePath.c_str()), NULL, NULL, NULL, 0, NULL, NULL, &pSvc);
    if (SUCCEEDED(hr)) {
        hr = CoSetProxyBlanket(pSvc, RPC_C_AUTHN_WINNT, RPC_C_AUTHZ_NONE, 0, RPC_C_AUTHN_LEVEL_CALL, RPC_C_IMP_LEVEL_IMPERSONATE, 0, EOAC_NONE);
        if (SUCCEEDED(hr)) {
            hr = pSvc->ExecQuery(bstr_t("WQL"), _bstr_t(query.c_str()), WBEM_FLAG_FORWARD_ONLY, 0, &pEnumerator);
            if (SUCCEEDED(hr) && pEnumerator) {
                IWbemClassObject* pObj = nullptr; ULONG u = 0;
                while (pEnumerator->Next(WBEM_INFINITE, 1, &pObj, &u) == S_OK && u) {
                    VARIANT v; hr = pObj->Get(propertyName.c_str(), 0, &v, 0, 0);
                    if (SUCCEEDED(hr)) {
                        if (v.vt == VT_BSTR) { std::wstring w(v.bstrVal); resultResult = std::string(w.begin(), w.end()); success = true; }
                        else if (v.vt == VT_I4 || v.vt == VT_UI4) { resultResult = std::to_string(v.uintVal); success = true; }
                        VariantClear(&v);
                    }
                    pObj->Release();
                }
            }
            if (pEnumerator) pEnumerator->Release();
        }
        if (pSvc) pSvc->Release();
    }
    if (pLoc) pLoc->Release();
    return success;
}

bool VmEnvironmentDetector::CheckFirmwareSmbiosUsingGetSystemFirmwareTable(std::string& outInfo) {
    DWORD sig = 'RSMB';
    DWORD size = GetSystemFirmwareTable(sig, 0, NULL, 0);
    if (size == 0) return false;
    std::vector<BYTE> buf(size);
    if (GetSystemFirmwareTable(sig, 0, buf.data(), size) == 0) return false;
    std::string s((char*)buf.data(), buf.size());
    std::string printable;
    for (char c : s) { if (c >= 32 && c <= 126) printable += c; else printable += ' '; }
    if (printable.length() > 200) outInfo = printable.substr(0, 200) + "..."; else outInfo = printable;
    return true;
}

// ACPI table scanning for common VM signatures
bool VmEnvironmentDetector::CheckAcpiTablesForVmSignatures(std::string& outInfo) {
    // We already used GetSystemFirmwareTable for SMBIOS; iterate few ACPI signatures
    std::vector<DWORD> sigs = { 'DSDT', 'SSDT', 'FACP' }; // rough set
    std::string found;
    for (DWORD s : sigs) {
        DWORD size = GetSystemFirmwareTable(s, 0, NULL, 0);
        if (size == 0) continue;
        std::vector<BYTE> buf(size);
        if (GetSystemFirmwareTable(s, 0, buf.data(), size) == 0) continue;
        std::string str((char*)buf.data(), buf.size());
        std::transform(str.begin(), str.end(), str.begin(), ::tolower);
        // signatures:
        if (str.find("vbox") != std::string::npos) found += "vbox ";
        if (str.find("vmware") != std::string::npos) found += "vmware ";
        if (str.find("qemu") != std::string::npos) found += "qemu ";
        if (str.find("kvm") != std::string::npos) found += "kvm ";
        if (str.find("bochs") != std::string::npos) found += "bochs ";
    }
    if (!found.empty()) { outInfo = found; return true; }
    outInfo = "none";
    return false;
}

// GPU vendor via DXGI
bool VmEnvironmentDetector::CheckGpuVendor(std::string& outInfo) {
    IDXGIFactory* pFactory = nullptr;
    HRESULT hr = CreateDXGIFactory(__uuidof(IDXGIFactory), (void**)&pFactory);
    if (FAILED(hr) || !pFactory) { outInfo = "dxgi_fail"; return false; }
    IDXGIAdapter* pAdapter = nullptr;
    if (pFactory->EnumAdapters(0, &pAdapter) != S_OK) { pFactory->Release(); outInfo = "none"; return false; }
    DXGI_ADAPTER_DESC desc; ZeroMemory(&desc, sizeof(desc));
    pAdapter->GetDesc(&desc);
    wchar_t buff[128]; StringCchPrintfW(buff, 128, L"%s", desc.Description);
    std::wstring wdesc(buff);
    std::string sdesc(wdesc.begin(), wdesc.end());
    pAdapter->Release();
    pFactory->Release();
    std::string low = sdesc; std::transform(low.begin(), low.end(), low.begin(), ::tolower);
    outInfo = sdesc;
    if (low.find("microsoft basic") != std::string::npos || low.find("vmware") != std::string::npos ||
        low.find("virtualbox") != std::string::npos || low.find("parallels") != std::string::npos) {
        return true;
    }
    return false;
}

// Check existence of common VM tool files/paths
bool VmEnvironmentDetector::CheckVmToolsFilesOrPaths() {
    std::vector<std::wstring> paths = {
        L"C:\\Program Files\\VMware\\VMware Tools",
        L"C:\\Program Files\\Oracle\\VirtualBox Guest Additions",
        L"C:\\Windows\\System32\\drivers\\vmhgfs.sys",
        L"C:\\Windows\\System32\\drivers\\vmmouse.sys",
        L"C:\\Windows\\System32\\drivers\\vboxguest.sys",
        L"C:\\Program Files\\Microsoft\\virtmgmt" // example
    };
    for (auto& p : paths) {
        DWORD attrib = GetFileAttributesW(p.c_str());
        if (attrib != INVALID_FILE_ATTRIBUTES) return true;
    }
    return false;
}

// PCI vendor check
bool VmEnvironmentDetector::CheckPciVendors(std::string& outInfo) {
    // We will scan device instance ids (similar to PnP) and look for known vendor strings
    HDEVINFO hDevInfo = SetupDiGetClassDevs(NULL, NULL, NULL, DIGCF_ALLCLASSES | DIGCF_PRESENT);
    if (hDevInfo == INVALID_HANDLE_VALUE) { outInfo = "fail"; return false; }
    SP_DEVINFO_DATA deviceInfoData; deviceInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
    std::string found;
    for (DWORD i = 0; SetupDiEnumDeviceInfo(hDevInfo, i, &deviceInfoData); ++i) {
        char buf[1024];
        if (SetupDiGetDeviceInstanceIdA(hDevInfo, &deviceInfoData, buf, sizeof(buf), NULL)) {
            std::string id(buf);
            std::transform(id.begin(), id.end(), id.begin(), ::tolower);
            if (id.find("ven_15ad") != std::string::npos || id.find("ven_80ee") != std::string::npos ||
                id.find("ven_1af4") != std::string::npos || id.find("ven_1d0f") != std::string::npos) {
                found += id + ";";
            }
        }
    }
    SetupDiDestroyDeviceInfoList(hDevInfo);
    if (!found.empty()) { outInfo = found; return true; }
    outInfo = "none";
    return false;
}

// Hyper-V root partition detection via NtQuerySystemInformation (if available)
bool VmEnvironmentDetector::CheckHypervRootPartition() {
    auto ntqi = GetNtQuerySystemInformation();
    if (!ntqi) return false;
    // SystemHypervisorDetailInformation is undocumented in public headers - try simple approach:
    // Query SystemCodeIntegrityInformation (as probe) or query for hypervisor presence via IsProcessorFeaturePresent
    if (IsProcessorFeaturePresent(PF_VIRT_FIRMWARE_ENABLED)) {
        // This flag indicates virtualization firmware support; not conclusive.
    }
    // Secondary check: presence of vmbus device
    HDEVINFO hDevInfo = SetupDiGetClassDevs(NULL, L"ROOT", NULL, DIGCF_ALLCLASSES | DIGCF_PRESENT);
    if (hDevInfo == INVALID_HANDLE_VALUE) return false;
    SP_DEVINFO_DATA deviceInfoData; deviceInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
    bool found = false;
    for (DWORD i = 0; SetupDiEnumDeviceInfo(hDevInfo, i, &deviceInfoData); ++i) {
        char buf[1024];
        if (SetupDiGetDeviceInstanceIdA(hDevInfo, &deviceInfoData, buf, sizeof(buf), NULL)) {
            std::string id(buf);
            std::transform(id.begin(), id.end(), id.begin(), ::tolower);
            if (id.find("vmbus") != std::string::npos || id.find("hyper-v") != std::string::npos || id.find("hyperv") != std::string::npos) {
                found = true; break;
            }
        }
    }
    SetupDiDestroyDeviceInfoList(hDevInfo);
    return found;
}

// Memory latency benchmark (memcpy large)
bool VmEnvironmentDetector::CheckMemoryLatencyBenchmark(unsigned long& ms) {
    const size_t sizeBytes = 100 * 1024 * 1024; // 100 MB
    try {
        std::vector<char> a(sizeBytes), b(sizeBytes);
        // warmup
        memset(a.data(), 0xAA, sizeBytes);
        memset(b.data(), 0x55, sizeBytes);
        DWORD start = GetTickCount();
        memcpy(b.data(), a.data(), sizeBytes);
        DWORD end = GetTickCount();
        ms = end - start;
        return true;
    }
    catch (...) {
        ms = 0; return false;
    }
}


// Helper: compute median of vector<unsigned long long>
static unsigned long long median_ull(std::vector<unsigned long long>& v) {
    if (v.empty()) return 0;
    size_t n = v.size();
    std::sort(v.begin(), v.end());
    if (n % 2) return v[n / 2];
    return (v[n / 2 - 1] + v[n / 2]) / 2;
}

// Helper: median absolute deviation
static double compute_mad(const std::vector<unsigned long long>& samples, unsigned long long med) {
    std::vector<double> absdev;
    absdev.reserve(samples.size());
    for (auto s : samples) absdev.push_back(std::fabs((double)s - (double)med));
    std::sort(absdev.begin(), absdev.end());
    size_t n = absdev.size();
    if (n == 0) return 0.0;
    if (n % 2) return absdev[n / 2];
    return (absdev[n / 2 - 1] + absdev[n / 2]) / 2.0;
}

bool VmEnvironmentDetector::CheckRDTSCJitter(double& stddev_out, unsigned long& median_out) {
    const int SAMPLES = TIMING_SAMPLES; // 512 as defined
    std::vector<unsigned long long> raw;
    raw.reserve(SAMPLES);

    // 1) Calibrate CPU frequency (cycles/sec) using QPC over 200ms
    LARGE_INTEGER f, t1, t2;
    QueryPerformanceFrequency(&f);
    QueryPerformanceCounter(&t1);
    unsigned int aux;
    unsigned long long r1 = __rdtscp(&aux);
    Sleep(200);
    unsigned long long r2 = __rdtscp(&aux);
    QueryPerformanceCounter(&t2);
    double elapsedSec = double(t2.QuadPart - t1.QuadPart) / double(f.QuadPart);
    double cpuHz = (elapsedSec > 0.0) ? (double)(r2 - r1) / elapsedSec : 0.0;
    if (cpuHz < 1e6) cpuHz = 1e9; // fallback if weird

    // 2) Pin thread to current processor & raise priority
    HANDLE hThread = GetCurrentThread();
    DWORD proc = GetCurrentProcessorNumber();
    ULONG_PTR mask = ((ULONG_PTR)1 << (proc % (8 * sizeof(ULONG_PTR))));
    DWORD_PTR prevAffinity = SetThreadAffinityMask(hThread, mask);
    int prevPrio = GetThreadPriority(hThread);
    SetThreadPriority(hThread, THREAD_PRIORITY_HIGHEST);

    // warm-up loop
    for (int i = 0; i < 50; ++i) { __rdtscp(&aux); }

    // 3) Collect samples
    for (int i = 0; i < SAMPLES; ++i) {
        unsigned long long t1c = __rdtscp(&aux);
        // small busy op (use portable nop)
        for (volatile int k = 0; k < 10; ++k) { __nop(); }
        unsigned long long t2c = __rdtscp(&aux);
        raw.push_back(t2c - t1c);
    }

    // restore thread affinity & priority
    if (prevAffinity != 0) SetThreadAffinityMask(hThread, prevAffinity);
    SetThreadPriority(hThread, prevPrio);

    if (raw.empty()) return false;

    // 4) compute median and MAD, exclude outliers beyond median + K * MAD
    unsigned long long med = median_ull(raw);
    double mad = compute_mad(raw, med);
    if (mad == 0.0) mad = 1.0; // avoid div by zero
    const double K = 10.0; // tunable: excludes extreme outliers
    std::vector<unsigned long long> filtered;
    filtered.reserve(raw.size());
    for (auto v : raw) {
        if (std::fabs((double)v - (double)med) <= K * mad) filtered.push_back(v);
    }
    if (filtered.empty()) filtered = raw; // fallback if we filtered everything

    // 5) compute stddev on filtered samples
    double mean = 0.0;
    for (auto v : filtered) mean += (double)v;
    mean /= filtered.size();
    double var = 0.0;
    for (auto v : filtered) {
        double d = (double)v - mean;
        var += d * d;
    }
    var /= filtered.size();
    double stddev_cycles = sqrt(var);

    // convert stddev to milliseconds (approx) using cpuHz
    double stddev_ms = (stddev_cycles / cpuHz) * 1000.0;

    // output normalized values
    stddev_out = stddev_ms;                // in ms
    median_out = (unsigned long)med;       // raw cycles median

    return true;
}

// Anti-debug & sandbox
bool VmEnvironmentDetector::CheckDebuggerPresent() {
    if (IsDebuggerPresent()) return true;
    // Check remote debug
    BOOL remote = FALSE;
    CheckRemoteDebuggerPresent(GetCurrentProcess(), &remote);
    if (remote) return true;
    // Check NtQueryInformationProcess for debug port - load dynamically to be safe
    HMODULE hNtdll = GetModuleHandleW(L"ntdll.dll");
    if (hNtdll) {
        typedef NTSTATUS(WINAPI* NtQIP)(HANDLE, ULONG, PVOID, ULONG, PULONG);
        NtQIP pNtQIP = (NtQIP)GetProcAddress(hNtdll, "NtQueryInformationProcess");
        if (pNtQIP) {
            ULONG retLen = 0;
            ULONG debugPort = 0;
            if (pNtQIP(GetCurrentProcess(), 7 /*ProcessDebugPort*/, &debugPort, sizeof(debugPort), &retLen) == 0) {
                if (debugPort != 0 && debugPort != (ULONG)-1) return true;
            }
        }
    }
    return false;
}

bool VmEnvironmentDetector::CheckSandboxArtifacts() {
    // Heuristics: presence of known sandbox paths, strange user name, low RAM, single CPU core, short uptime.
    std::vector<std::wstring> suspiciousPaths = {
        L"C:\\Windows\\Temp\\scservice.exe",
        L"C:\\Program Files\\Cuckoo",
        L"C:\\Program Files\\HybridAnalysis",
        L"C:\\Program Files\\Any.Run"
    };
    for (auto& p : suspiciousPaths) {
        if (GetFileAttributesW(p.c_str()) != INVALID_FILE_ATTRIBUTES) return true;
    }
    // check small physical memory or 1 core
    MEMORYSTATUSEX mem; mem.dwLength = sizeof(mem);
    GlobalMemoryStatusEx(&mem);
    if (mem.ullTotalPhys < (512ull * 1024ull * 1024ull)) return true; // <512MB suspicious
    SYSTEM_INFO si; GetSystemInfo(&si);
    if (si.dwNumberOfProcessors <= 1) return true;
    // uptime very small
    ULONGLONG up = GetTickCount64() / 1000ull;
    if (up < 60ull) return true; // started less than 1 minute ago
    return false;
}