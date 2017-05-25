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
	time += 0.01;
	for (int i = 0; i < Nx; i++) {
		v[i] = sin(time) + sin(6.28*i / Nx);
		u[i] = sin(time) * sin(6.28*i / Nx*(1+0.5*sin(time*3)));
	}
	int z=0;
#pragma omp parallel for
	for (int i = 1; i < 10000000; i++) {
		// pretend working :)
		z += i;
	}

	return z!=0;
}

void ExampleSolver::getResults(std::ostream &out)
{
	out << "time\tx\tu\tv\n";
	for (int i = 0; i < Nx; i++) {
		out << time << "\t" << i*0.111111111111111111111111111 << "\t" << u[i] << "\t" << v[i] << "\n";
	}
}

