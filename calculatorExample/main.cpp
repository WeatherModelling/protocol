#include <iostream>
#include <vector>
#include <string>
#include "Service.h"



int main() {
	std::vector<std::string> commands{
		R"({"function":"version"})",
		R"({"function":"getCapabilities"})",
		R"({"function":"initSolver",	"params":{		"g":9.81,		"Nx":10,		"u0":[0,0.1,0.2,0.1,0,0,0,0,0,0]}})",
		R"({"function":"evolve","stepsNumber":10})",
		R"({"function":"getResults","names" : "all"})"
	};
	Service service(std::cout);
	for (auto &cmd : commands) {
		std::cout << "\n\n=========COMMAND===========\n" << cmd << "\n==========RESPONSE===========\n";
		service.executeCommand(cmd.c_str());
		std::cout << "\n==========END==========\n";
	}

	return 0;

}

