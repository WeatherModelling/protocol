#include <iostream>
#include <vector>
#include <string>
#include <sstream>
#include <istream>
#include <algorithm>
#include "Service.h"

static std::stringstream make_fake_stream() {
	std::vector<std::string> commands{
		R"({"function":"version"})",
		R"({"function":"getCapabilities"})",
		R"({"function":"initSolver",	"params":{		"g":9.81,		"Nx":10,		"u0":[0,0.1,0.2,0.1,0,0,0,0,0,0]}})",
		R"({"function":"evolve","stepsNumber":10})",
		R"({"function":"getResults","names" : "all"})"
	};

	std::stringstream ss;
	std::for_each(commands.begin(), commands.end(), [&ss](auto x) {ss << x << '\0'; });
	return ss;
}

int main() {
	std::istream& input = std::cin;
	//std::istream& input = make_fake_stream();
	
	Service service(std::cout);
	while (!input.eof()) {
		std::string cmd;
		std::getline(input, cmd, '\0');
		service.executeCommand(cmd.c_str());
	}

	return 0;

}

