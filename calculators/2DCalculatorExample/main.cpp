#include <iostream>
#include <vector>
#include <string>
#include <sstream>
#include <istream>
#include <algorithm>
#include "Service.h"
int main() {
	std::istream& input = std::cin;
	
	Service service(std::cout);
	while (!input.eof()) {
		std::string cmd;
		std::getline(input, cmd, '\0');
		service.executeCommand(cmd.c_str());
	}

	return 0;

}

