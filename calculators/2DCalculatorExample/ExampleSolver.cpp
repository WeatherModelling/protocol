#include "ExampleSolver.h"
#include <cmath>

inline std::vector<std::vector<double>> arrayOfDouble(size_t first, size_t second) {
	auto row = std::vector<double>(second, DBL_EPSILON);
	return std::vector<std::vector<double> >(first, row);
}

ExampleSolver::ExampleSolver(const nlohmann::json & d) :
	Solver(d),
	g(d["g"]),Nx(d["Nx"]),Nz(2*Nx),
	T(arrayOfDouble(Nx,Nz)),
	vx(arrayOfDouble(Nx,Nz)),
	vz(arrayOfDouble(Nx,Nz))
{
}

bool ExampleSolver::makeStep()
{
	time += 0.03;
	for (int i = 0; i < Nx; i++) {
		for (int j = 0; j < Nz; j++) {
			T[i][j] = sin(time) * sin(6.28*i / Nx)*cos(6.28*i / Nz);
			vx[i][j] = exp(-time) * (i*i+j*j);
			vz[i][j] = sin(time) * (i*i-j*j);
		}
	}
	return true;
}

void ExampleSolver::getResults(std::ostream &out)
{
	out << "time\tx\tz\tT\tvx\tvz\n";
	for (int i = 0; i < Nx; i++) {
		for (int j = 0; j < Nz; j++) {
			out << time << "\t" << i*0.111111111111111111111111111 << "\t" << j*0.111111111111111111111111111 << "\t" << T[i][j] << "\t" << vx[i][j] << "\t" << vz[i][j] << "\n";
		}
	}
}

