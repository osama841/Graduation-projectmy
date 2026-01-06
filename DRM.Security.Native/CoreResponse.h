// src/DRM.Security.Native/Common/CoreResponse.h
#pragma once
#include <string>
#include <vector>
#include <nlohmann/json.hpp>
using json = nlohmann::json;
namespace DRM {
    namespace Security {
        namespace Native {
            namespace Unit {
                struct CoreResponse {
                    bool success = false;
                    int errorCode = 0;
                    std::string message;
                    std::string data;
                    std::vector<std::string> logs;

                    // تحويل إلى JSON
                    std::string ToJson() const {
                        json j;
                        j["success"] = success;
                        j["errorCode"] = errorCode;
                        j["message"] = message;
                        j["data"] = data;
                        j["logs"] = logs;
                        return j.dump();
                    }
                };

            }
        }
    }
}