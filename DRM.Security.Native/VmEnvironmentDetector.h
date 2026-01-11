#pragma once

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <string>

class VmEnvironmentDetector {
public:
    VmEnvironmentDetector();
    ~VmEnvironmentDetector();

    // Run full scan and return aggregated score
    int RunScan();

    // Get textual detailed report
    std::string GetDetailedReport() const;

    // Final decision
    bool IsVirtualMachine() const;

private:
    int m_totalScore;
    std::string m_reportLog;

    // Weights & thresholds (adjustable)
    static constexpr int WEIGHT_TPM = 30;
    static constexpr int WEIGHT_TIMING = 20;
    static constexpr int WEIGHT_CPUID = 25;
    static constexpr int WEIGHT_MAC = 10;
    static constexpr int WEIGHT_SMBIOS = 20;
    static constexpr int WEIGHT_DRIVERS = 20;
    static constexpr int WEIGHT_PNP = 15;
    static constexpr int WEIGHT_NETWORK = 10;
    static constexpr int WEIGHT_GPU = 15;
    static constexpr int WEIGHT_ACPI = 30;
    static constexpr int WEIGHT_MEMORY = 15;
    static constexpr int WEIGHT_RDTSC_JITTER = 25;

    static constexpr int SCORE_THRESHOLD_STRONG = 60;
    static constexpr unsigned long LONG_LATENCY_THRESHOLD = 4000; // ms
    static constexpr int TIMING_SAMPLES = 512; // for jitter

    // Helpers - detectors
    std::string CheckCpuidHypervisor();
    bool CheckMacAddressArtifacts();
    unsigned long CheckTimingAttackMedian();
    bool CheckSmbiosViaWmi(std::string& outInfo);
    int CheckTpmStatusWmi(std::string& tpmInfo);
    bool CheckVmServicesOrDrivers();
    bool CheckPnPDevices();
    unsigned long CheckNetworkLatencyToPublic(const std::string& host = "8.8.8.8", int port = 53);
    bool WmiQuery(const std::wstring& namespacePath, const std::wstring& query, const std::wstring& propertyName, std::string& resultResult);
    bool CheckFirmwareSmbiosUsingGetSystemFirmwareTable(std::string& outInfo);

    // Advanced checks
    bool CheckAcpiTablesForVmSignatures(std::string& outInfo);
    bool CheckGpuVendor(std::string& outInfo);
    bool CheckVmToolsFilesOrPaths();
    bool CheckPciVendors(std::string& outInfo);
    bool CheckHypervRootPartition();
    bool CheckMemoryLatencyBenchmark(unsigned long& ms);
    bool CheckRDTSCJitter(double& stddev, unsigned long& median);

    // Anti-analysis
    bool CheckDebuggerPresent();
    bool CheckSandboxArtifacts();

    // Utility
    void AddLog(const std::string& message);
    void SafeSleep(DWORD ms);
};

