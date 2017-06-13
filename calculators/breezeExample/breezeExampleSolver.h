#pragma once
#include "Solver.h"
#include <vector>

class breezeExampleSolver :
	public Solver
{


	typedef double real;
	const real epsilon = std::numeric_limits<real>::epsilon();
public:
	// horizontal resolution
	const int Nx;
	// vertical resolution
	const int Nz;
	// 'vertical' array of machine epsilons
	const std::vector<real> epsilons;
	// davlenie, gorizontal'naja skorost', vertikal'naja skorost', temperatura
	std::vector<std::vector<real> > p1, u1, u2, w1, w2, tmp1, tmp2;
public:


	//constanty vozduha
	const real beta = 3.665e-3;   // 1/K
	const real ro = 1.23e-3;      // g/cm3
	const real ge = 9.8e+2;       // cm/s2
	const real gamma = -0.65e-4;  // K/cm
	const real kv = 1.5e-1;       // cm2/s
	const real p0shtrih = 1.01e+3; // g/cms2  =1mbar
	const real tet0shtrih = 10;     // K

									// raznye parametry
	int n;
	real tt, dx, dz, dt;
	real t0, x0, z0, T00, DT00, v0, dt0;

	// parametry v uravnenijah
	real a, b, c3, c4, d1;

	breezeExampleSolver(const nlohmann::json & d);

	// Inherited via Solver
	virtual bool makeStep() override;
	void Calculation1();
	void Calculation2();
	void Calculation3();
	void Calculation4();
	virtual void getResults(std::ostream &out) override;
};

