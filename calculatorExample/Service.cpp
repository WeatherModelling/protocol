#include "Service.h"

#include "json.hpp"
#include <string>
#include <exception>

#include "ExampleSolver.h"


using json = nlohmann::json;

void Service::executeCommand(const char * JSON)
{
	try {
		// Parse command
		auto doc = json::parse(JSON);
		const std::string function = doc["function"];
		if (function == "version") {
			version(doc);
		}
		else if (function == "getCapabilities") {
			getCapabilities(doc);
		}
		else if (function == "initSolver") {
			initSolver(doc);
		}
		else if (function == "evolve") {
			evolve(doc);
		}
		else if (function == "getResults") {
			getResults(doc);
		}
		else {
			throw std::invalid_argument("undefined function: " + function);
		}

	}
	catch (std::exception & ex) {
		outputStream << json{
			{"error", "invalid request"},
			{"errordetails", ex.what() }
		};
	}
}

void Service::version(const nlohmann::json & d)
{
	outputStream << R"({"version":)" << std::to_string(versionNumber) << "}";
}

void Service::getCapabilities(const nlohmann::json & d)
{
	outputStream << R"(
{"features":
    [
        "restartable",
        "dynamic"
    ],
 "initial":
	[
		{"name":"g","type":{"typename":"double"},"description":"gravity field strength","unit":"ms^-2"}
		{"name":"Nx","type":{"typename":"int"},"description":"number of points along x axis"}
		{"name":"u0","type":{"typename":"array","elementType":"double","count":"Nx"},"description":"Initial U distribution"}
	],
 "results":
    [
        {"name":"t","type":"double","description":"time","unit":"s","isIndependent":"true"},
        {"name":"x","type":"double","description":"x coordinate","unit":"m","isIndependent":"true"},
        {"name":"u","type":"double","description":"u field value","unit":"J"}
        {"name":"v","type":"double","description":"velocity value","unit":"ms^-1"}
    ]
}
)";
}

void Service::initSolver(const nlohmann::json & d)
{
	solver = new ExampleSolver(d["params"]);
	outputStream << R"({"success":"true"})";
}

void Service::evolve(const nlohmann::json & d)
{
	if (solver == nullptr) {
		throw std::exception("Solver not initiated");
	}
	bool ok = true;
	if (d.find("stepsNumber") != d.end()) {
		// do the steps number
		for (size_t i = 0; i < (size_t)d["stepsNumber"]; i++)
		{
			ok = solver->makeStep();
			if (!ok) {
				break;
			}
		}
	}
	else if (d.find("finalTime") != d.end()) {
		while (solver->getCurrentTime() <  (double)d["finalTime"])
		{
			ok = solver->makeStep();
			if (!ok) {
				break;
			}
		}
	}
	else {
		throw std::invalid_argument("you should specify either stepsNumber or finalTime");
	}
	outputStream << json{ {"success",ok},{"time",solver->getCurrentTime()} };
}

void Service::getResults(const nlohmann::json & d)
{
	if (solver == nullptr) {
		throw std::exception("Solver not initiated");
	}
	solver->getResults(outputStream);
}

