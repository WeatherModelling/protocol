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
	outputStream << '\0';
	outputStream.flush();
	fflush(stdout);
}

void Service::version(const nlohmann::json & d)
{
	outputStream << json{ {"version",protocolVersion } };
}

void Service::getCapabilities(const nlohmann::json & d)
{
	outputStream << R"(
{"features":
    [
        "restartable",
        "dynamic"
    ],
 "initials":
	[
		{"name":"g","type":{"typename":"double"},"description":"gravity field strength","unit":"ms^-2"},
		{"name":"Nx","type":{"typename":"int"},"description":"number of points along x axis"},
		{"name":"u0","type":{"typename":"array","type":{"typename":"double"},"count":"Nx"},"description":"Initial U distribution"}
	],
 "results":
    [
        {"name":"t","type":"double","description":"time","unit":"s","isIndependent":"true"},
        {"name":"x","type":"double","description":"x","unit":"m","isIndependent":"true"},
        {"name":"z","type":"double","description":"z","unit":"m","isIndependent":"true"},
        {"name":"T","type":"double","description":"temperature","unit":"K"},
        {"name":"vx","type":"double","description":"horizontal velocity","unit":"ms^-1"},
        {"name":"vz","type":"double","description":"vertical velocity","unit":"ms^-1"}
    ]
}
)";
}

void Service::initSolver(const nlohmann::json & d)
{
	if (solver != nullptr) {
		delete solver;
	}
	solver = new ExampleSolver(d["params"]);
	outputStream << R"({"success":"true"})";
}

void Service::evolve(const nlohmann::json & d)
{
	if (solver == nullptr) {
		throw std::exception("Solver not initiated");
	}
	bool ok = true;
	if (d.find("timeStepsNumber") != d.end()) {
		const size_t timeStepsNumber = (size_t)d["timeStepsNumber"];
		// do the steps number
		for (size_t i = 0; i < timeStepsNumber; i++)
		{
			ok = solver->makeStep();
			if (!ok) {
				break;
			}
		}
	}
	else if (d.find("finalTime") != d.end()) {
		const double finalTime = d["finalTime"];
		while (solver->getCurrentTime() < finalTime)
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

