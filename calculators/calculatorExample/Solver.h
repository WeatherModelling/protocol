#pragma once
#include "json.hpp"
#include <ostream>
class Solver
{
protected:
	double time=0;
public:
	Solver() = delete;
	Solver(const Solver&) = delete;
	Solver(const Solver&&) = delete;
	Solver(const nlohmann::json & d) {};
	double getCurrentTime() {
		return time;
	}
	virtual bool makeStep() = 0;
	virtual void getResults(std::ostream &out) = 0;

};

