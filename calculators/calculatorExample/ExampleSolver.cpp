#include "ExampleSolver.h"
#include <cmath>

ExampleSolver::ExampleSolver(const nlohmann::json & d) :
	Solver(d),
	g(d["g"]),Nx(d["Nx"]),
	u(d["u0"].size()),
	v(Nx)
{
	for (size_t i = 0; i < Nx; i++)
	{
		u[i] = d["u0"][i];
	}
}

bool ExampleSolver::makeStep()
{
	time += 0.03;
	for (int i = 0; i < Nx; i++) {
		v[i] = sin(time) + sin(6.28*i / Nx);
		u[i] = sin(time) * sin(6.28*i / Nx);
	}
	return true;
}

void ExampleSolver::getResults(std::ostream &out)
{
	out << "time\tx\tu\tv\n";
	for (int i = 0; i < Nx; i++) {
		out << time << "\t" << i*0.111111111111111111111111111 << "\t" << u[i] << "\t" << v[i] << "\n";
	}
}

