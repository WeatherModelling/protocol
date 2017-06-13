#include "breezeExampleSolver.h"
#include <cmath>
#include <limits>
#include <cstdio>
#include <vector>
#include <algorithm>

inline std::vector<std::vector<double>> arrayOfDouble(size_t first, size_t second) {
	auto row = std::vector<double>(second, DBL_EPSILON);
	return std::vector<std::vector<double> >(first, row);
}

breezeExampleSolver::breezeExampleSolver(const nlohmann::json & d) :
	Solver(d),

	// Const initialization
	Nx(d["Nx"]), 
	Nz(d["Nz"]),
	x0(d["x0"]),
	z0(d["z0"]),
	v0(d["v0"]),
	DT00(d["DT00"]),
	dt0(d["dt0"]),

	epsilons(Nz, epsilon),
	// initial conditions
	// create and fill arrays with machine epsilons 
	p1(Nx, epsilons),
	u1(Nx, epsilons),
	u2(Nx, epsilons),
	w1(Nx, epsilons),
	w2(Nx, epsilons),
	tmp1(Nx, epsilons),
	tmp2(Nx, epsilons)

{
	// vremja t0 v sekundah
	// *** unnamed const
	t0 = (1e+5*x0) / (1e+2*v0);

	//parametry v uravnenijah
	a = beta*ro*ge*tet0shtrih*z0*(1e+5) / p0shtrih;        // z0 perevedeno v cm

	b = x0 / z0;

	// *** unnamed const
	dx = 10.0 / Nx;
	dz = 1.0 / Nz;


	dt = dt0 / t0;

	// *** unnamed const
	c3 = p0shtrih / (v0*v0*(1e+4)*ro);              // v0 perevedeno v cm/s

													// *** unnamed const
	c4 = (1e+6)*kv*b / (v0*z0*(1e+7));              // z0 perevedeno v cm,  v0 perevedeno v cm/s, uvelichen koeff vjazkosti do turbulentnogo

													// *** unnamed const
	d1 = x0*(1e+5)*gamma / tet0shtrih;

	T00 = DT00 / tet0shtrih;
	n = 0;
	tt = 0;

	/*
	for i: = 0 to Nx do                    // raspredelenie temperatury
	begin

	tmp1[i, 0] : = T00*0.5*((Exp(2 * dx*(i - (Nx div 2))) - 1) / (Exp(2 * dx*(i - (Nx div 2))) + 1) + 1);

	end;

	*/
	for (auto i = 0; i < Nx; i++) {
		tmp1[i][0] = T00*0.5*((exp(2 * dx*(i - (Nx / 2))) - 1) / (exp(2 * dx*(i - (Nx / 2))) + 1) + 1);
	}
}

bool breezeExampleSolver::makeStep()
{
	static int nn = 1;
	static int np = 0;
	static int nnp = 1;
	Calculation1();
	Calculation2();
	Calculation3();
	Calculation4();

	// vremja v minutah
	tt = n*dt*t0 / 60;


	// next step
	n++;
	/*
	for i:=0 to Nx do                                     // Perehod na sledujuschij shag po vremeni
	begin
	for j:=0 to Nz do
	begin
	tmp1[i,j]:=tmp2[i,j];
	u1[i,j]:=u2[i,j];
	w1[i,j]:=w2[i,j];
	p1[i,j]:=1e-16;
	end;
	end;
	*/

	tmp1 = tmp2;
	u1 = u2;
	w1 = w2;
	for (auto &row : p1) {
		for (auto &p1ij : row) {
			p1ij = epsilon;
		}
	}

	nn++;
	return true;
}



// reshaet uravnenie dlja davlenija
void breezeExampleSolver::Calculation1() {
	/*
	for i: = 0 to Nx do
	begin
	p1[i, Nz] : = 1e-16;
	end;
	*/
	for (auto i = 0; i < Nx; i++) {
		p1[i][Nz - 1] = epsilon;
	}
	/*
	for i: = 0 to Nx do
	begin
	for j : = Nz - 1 downto 0 do
	begin

	p1[i, j] : = p1[i, j + 1] - 0.5*dz*a*(tmp1[i, j] + tmp1[i, j + 1]);

	end;
	end;
	*/
	for (auto i = 0; i < Nx; i++) {
		for (auto j = Nz - 2; j >= 0; j--) {
			p1[i][j] = p1[i][j + 1] - 0.5*dz*a*(tmp1[i][j] + tmp1[i][j + 1]);
		}
	}
}

// reshaet uravnenie dlja gorizontal'noj skorosti
void breezeExampleSolver::Calculation2() {

	/*

	for i: = 1 to Nx - 1 do
	begin
	for j : = 1 to Nz - 1 do
	begin


	if u1[i, j]>0 then
	u2[i, j]: = u1[i, j] - dt*u1[i, j] * (u1[i, j] - u1[i - 1, j]) / dx - dt*b*w1[i, j] * (u1[i, j] - u1[i, j - 1]) / dz -
	dt*c3*(p1[i + 1, j] - p1[i, j]) / dx + dt*c4*(u1[i, j + 1] - 2 * u1[i, j] + u1[i, j - 1]) / (dz*dz)
	+ dt*c4*(u1[i + 1, j] - 2 * u1[i, j] + u1[i - 1, j]) / (1 * dx*dx)
	else
	u2[i, j]: = u1[i, j] - dt*u1[i, j] * (u1[i + 1, j] - u1[i, j]) / dx - dt*b*w1[i, j] * (u1[i, j + 1] - u1[i, j]) / dz -
	dt*c3*(p1[i + 1, j] - p1[i, j]) / dx + dt*c4*(u1[i, j + 1] - 2 * u1[i, j] + u1[i, j - 1]) / (dz*dz)
	+ dt*c4*(u1[i + 1, j] - 2 * u1[i, j] + u1[i - 1, j]) / (1 * dx*dx);


	end;
	end;

	*/
	for (auto i = 1; i < Nx - 1; i++) {
		for (auto j = 1; j < Nz - 1; j++) {
			if (u1[i][j] > 0) {

				u2[i][j] = u1[i][j] - dt*u1[i][j] * (u1[i][j] - u1[i - 1][j]) / dx - dt*b*w1[i][j] * (u1[i][j] - u1[i][j - 1]) / dz -
					dt*c3*(p1[i + 1][j] - p1[i][j]) / dx + dt*c4*(u1[i][j + 1] - 2 * u1[i][j] + u1[i][j - 1]) / (dz*dz)
					+ dt*c4*(u1[i + 1][j] - 2 * u1[i][j] + u1[i - 1][j]) / (1 * dx*dx);
			}
			else
			{
				u2[i][j] = u1[i][j] - dt*u1[i][j] * (u1[i + 1][j] - u1[i][j]) / dx - dt*b*w1[i][j] * (u1[i][j + 1] - u1[i][j]) / dz -
					dt*c3*(p1[i + 1][j] - p1[i][j]) / dx + dt*c4*(u1[i][j + 1] - 2 * u1[i][j] + u1[i][j - 1]) / (dz*dz)
					+ dt*c4*(u1[i + 1][j] - 2 * u1[i][j] + u1[i - 1][j]) / (1 * dx*dx);
			}
		}
	}

	/*

	for j: = 0 to Nz do
	begin
	u2[0, j] : = 1e-16;
	end;

	for j: = 0 to Nz do
	begin
	u2[Nx, j] : = 1e-16;
	end;
	*/

	for (auto j = 0; j < Nz; j++) {
		u2[0][j] = epsilon;
		u2[Nx - 1][j] = epsilon;
	}

	/*
	for i: = 0 to Nx do
	begin
	u2[i, 0] : = 1e-16;
	end;

	for i: = 0 to Nx do
	begin

	u2[i, Nz] : = 1e-16;

	end;
	*/
	for (auto i = 0; i < Nx; i++) {
		u2[i][0] = epsilon;
		u2[i][Nz - 1] = epsilon;
	}
}

// reshaet uravnenie nepreryvnosti
void breezeExampleSolver::Calculation3() {

	/*

	for i: = 0 to Nx do
	begin
	w2[i, Nz] : = 1e-16;
	end;
	*/
	for (auto i = 0; i < Nx; i++) {
		w2[i][Nz - 1] = epsilon;
	}

	/*
	for i: = 1 to Nx - 1 do
	begin
	for j : = Nz - 1 downto 1 do
	begin
	if u2[i, j] >= 0 then
	w2[i, j] : = w2[i, j + 1] - dz*(u2[i, j] - u2[i - 1, j]) / (b*dx)

	else
	w2[i, j]: = w2[i, j + 1] - dz*(u2[i + 1, j] - u2[i, j]) / (b*dx);

	end;
	end;

	*/


	for (auto i = 1; i < Nx - 1; i++) {
		for (auto j = Nz - 2; j >= 1; j--) {
			if (u2[i][j] >= 0) {
				w2[i][j] = w2[i][j + 1] - dz*(u2[i][j] - u2[i - 1][j]) / (b*dx);
			}
			else {
				w2[i][j] = w2[i][j + 1] - dz*(u2[i + 1][j] - u2[i][j]) / (b*dx);
			}
		}
	}

	/*
	for j: = 0 to Nz do
	begin
	w2[0, j] : = 1e-16;
	end;


	for j: = 0 to Nz do
	begin
	w2[Nx, j] : = 1e-16;
	end;
	*/

	for (auto j = 0; j < Nz; j++) {
		w2[0][j] = epsilon;
		w2[Nx - 1][j] = epsilon;
	}

	/*

	for i: = 0 to Nx do
	begin
	w2[i, 0] : = 1e-16;
	end;

	*/

	for (auto i = 1; i < Nx; i++) {
		w2[i][0] = epsilon;
	}
}

// reshaet uravnenie teploprovodnosti
void breezeExampleSolver::Calculation4() {

	/*

	for i: = 0 to Nx do                       // pogranichnoe raspredelenie temperatury
	begin

	tmp2[i, 0] : = T00*0.5*((Exp(2 * dx*(i - (Nx div 2))) - 1) / (Exp(2 * dx*(i - (Nx div 2))) + 1) + 1);

	end;
	*/
	// pogranichnoe raspredelenie temperatury 
	for (auto i = 1; i < Nx; i++) {
		tmp2[i][0] = T00*0.5*((exp(2 * dx*(i - (Nx / 2))) - 1) / (exp(2 * dx*(i - (Nx / 2))) + 1) + 1);
	}

	/*
	for i: = 1 to Nx - 1 do
	begin
	for j : = 1 to Nz - 1 do
	begin

	tmp2[i, j] : = tmp1[i, j] - dt*u1[i, j] * (tmp1[i, j] - tmp1[i - 1, j]) / dx -
	dt*b*w1[i, j] * (tmp1[i, j] - tmp1[i, j - 1]) / dz - dt*d1*w1[i, j]
	+ dt*c4*(tmp1[i, j + 1] - 2 * tmp1[i, j] + tmp1[i, j - 1]) / (dz*dz)
	+ dt*c4*(tmp1[i + 1, j] - 2 * tmp1[i, j] + tmp1[i - 1, j]) / (1 * dx*dx);

	end;
	end;
	*/
	for (auto i = 1; i < Nx - 1; i++) {
		for (auto j = 1; j < Nz - 1; j++) {
			tmp2[i][j] = tmp1[i][j] - dt*u1[i][j] * (tmp1[i][j] - tmp1[i - 1][j]) / dx -
				dt*b*w1[i][j] * (tmp1[i][j] - tmp1[i][j - 1]) / dz - dt*d1*w1[i][j]
				+ dt*c4*(tmp1[i][j + 1] - 2 * tmp1[i][j] + tmp1[i][j - 1]) / (dz*dz)
				+ dt*c4*(tmp1[i + 1][j] - 2 * tmp1[i][j] + tmp1[i - 1][j]) / (1 * dx*dx);
		}
	}
	/*
	for i: = 0 to Nx do
	begin
	tmp2[i, Nz] : = 1e-16;
	end;
	*/

	for (auto i = 0; i < Nx; i++) {
		tmp2[i][Nz - 1] = epsilon;
	}

	/*
	for j: = 1 to Nz do
	begin
	tmp2[0, j] : = tmp2[1, j];
	end;

	for j: = 1 to Nz do
	begin
	tmp2[Nx, j] : = tmp2[Nx - 1, j];
	end;
	*/
	for (auto j = 1; j < Nz - 1; j++) {
		tmp2[0, j] = tmp2[1, j];
		tmp2[Nx - 1, j] = tmp2[Nx - 2, j];
	}

}



void breezeExampleSolver::getResults(std::ostream &out)
{
	out << "t\tx\tz\tp1\tu1\tu2\n";
	for (int i = 0; i < Nx; i++) {
		for (int j = 0; j < Nz; j++) {
			out << time << "\t" << i*0.111111111111111111111111111 << "\t" << j*0.111111111111111111111111111 << "\t" << p1[i][j] << "\t" << u1[i][j] << "\t" << u2[i][j] << "\n";
		}
	}
}



