#pragma once
#include <istream>
#include <ostream>
#include "json.hpp"
#include "Solver.h"

class Service {
	const int versionNumber = 1;
	std::ostream& outputStream;
	Solver* solver = nullptr;
public:
	Service(std::ostream& outputStream_) :
		outputStream(outputStream_)
	{

	}
	void executeCommand(const char* JSON);
private:
	void version(const nlohmann::json& d);
	void getCapabilities(const nlohmann::json& d);
	void initSolver(const nlohmann::json& d);
	void evolve(const nlohmann::json& d);
	void getResults(const nlohmann::json& d);
};
