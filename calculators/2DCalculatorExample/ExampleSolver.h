#pragma once
#include "Solver.h"
#include <vector>
class ExampleSolver :
	public Solver
{
public:
	const double g;
	const int Nx,Nz;
	std::vector<std::vector<double> > T, vx,vz;
	
	ExampleSolver(const nlohmann::json & d);

	// Inherited via Solver
	virtual bool makeStep() override;
	virtual void getResults(std::ostream &out) override;
};

