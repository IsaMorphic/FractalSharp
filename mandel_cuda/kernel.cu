
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "cuComplex.h"
#include "math.h"

__device__ double map(double old_value, double old_min, double old_max, double new_min, double new_max)
{
	double old_range = old_max - old_min;
	double new_range = new_max - new_min;

	return (((old_value - old_min) * new_range) / old_range) + new_min;
}

__device__ int lerp_colors(int color1, int color2, double v)
{
	int red1 = (color1 >> 16) & 0xFF;
	int green1 = (color1 >> 8) & 0xFF;
	int blue1 = color1 & 0xFF;

	int red2 = (color2 >> 16) & 0xFF;
	int green2 = (color2 >> 8) & 0xFF;
	int blue2 = color2 & 0xFF;

	double inverse_v = 1 - v;

	int redPart = (int)(red1 * inverse_v + red2 * v);
	int greenPart = (int)(green1 * inverse_v + green2 * v);
	int bluePart = (int)(blue1 * inverse_v + blue2 * v);

	return (255 << 24) + (redPart << 16) + (greenPart << 8) + (bluePart);
}

__device__ int color_from_iter(int iterCount, double znMagn, int *palette, int paletteLength) {
	double temp_i = iterCount;

	// sqrt of inner term removed using log simplification rules.
	double log_zn = log2(znMagn) / 2;
	double nu = log2(log_zn / log2(2.0)) / log2(2.0);
	// Rearranging the potential function.
	// Dividing log_zn by log(2) instead of log(N = 1<<8)
	// because we want the entire palette to range from the
	// center to radius 2, NOT our bailout radius.
	temp_i = temp_i + 1 - nu;
	// Grab two colors from the pallete
	int color1 = palette[(int)temp_i % (paletteLength - 1)];
	int color2 = palette[(int)(temp_i + 1) % (paletteLength - 1)];

	// Lerp between both colors
	int final = lerp_colors(color1, color2, fmod(temp_i, 1.0));

	// Return the result.
	return final;
}

extern "C" {
	__global__ void perturbation(
		int *out, int *palette, int paletteLength,
		cuDoubleComplex *points, int pointCount,
		int cell_x, int cell_y,
		int cellWidth, int cellHeight,
		int totalCells_x, int totalCells_y,
		double xMax, double yMax,
		int chunkSize, int maxChunkSize) {

		unsigned int x_dim = cell_x * cellWidth + blockIdx.x * blockDim.x + threadIdx.x;
		unsigned int y_dim = cell_y * cellHeight + blockIdx.y * blockDim.y + threadIdx.y;

		if (x_dim % chunkSize != 0 || y_dim % chunkSize != 0)
			return;

		unsigned int frameWidth = cellWidth * totalCells_x;
		unsigned int frameHeight = cellHeight * totalCells_y;

		int index = frameWidth * y_dim + x_dim;


		double x_origin = map(x_dim, 0, frameWidth, -xMax, xMax);
		double y_origin = map(y_dim, 0, frameHeight, -yMax, yMax);

		int iter = 0;

		int max_iter = pointCount;

		// Initialize some variables...
		cuDoubleComplex zn;

		cuDoubleComplex d0 = make_cuDoubleComplex(x_origin, y_origin);

		cuDoubleComplex dn = d0;

		double zn_r = 0;
		double zn_i = 0;

		double zn_magn = 0;

		// Mandelbrot algorithm
		if ((x_dim / chunkSize) % 2 == 0 && (y_dim / chunkSize) % 2 == 0 && chunkSize != maxChunkSize)
			return;
		do
		{
			// dn *= iter_list[iter] + dn
			dn = cuCmul(dn, cuCadd(points[iter], dn));

			// dn += d0
			dn = cuCadd(dn, d0);

			iter++;

			// zn = x[iter] * 0.5 + dn
			zn = cuCadd(cuCmul(points[iter], make_cuDoubleComplex(0.5, 0)), dn);

			zn_r = cuCreal(zn);
			zn_i = cuCimag(zn);

			zn_magn = zn_r * zn_r + zn_i * zn_i;

		} while (zn_magn < 256 && iter < max_iter);

		int color = 0;
		if (iter == max_iter) {
			color = 255 << 24;
		}
		else {
			color = color_from_iter(iter, zn_magn, palette, paletteLength);
		}
		for (int j = y_dim; j < y_dim + chunkSize; j++)
		{
			for (int i = x_dim; i < x_dim + chunkSize; i++)
			{
				int index = i + j * frameWidth;
				if(index < frameWidth * frameHeight)
					out[i + j * frameWidth] = color;
			}
		}
	}

	__global__ void traditional(
		int *out, int *palette,
		int paletteLength,
		int cell_x, int cell_y,
		int cellWidth, int cellHeight,
		int totalCells_x, int totalCells_y,
		double xMax, double yMax,
		double offset_x, double offset_y,
		int max_iteration, 
		int chunkSize, int maxChunkSize) {

		unsigned int x_dim = cell_x * cellWidth + blockIdx.x * blockDim.x + threadIdx.x;
		unsigned int y_dim = cell_y * cellHeight + blockIdx.y * blockDim.y + threadIdx.y;

		if (x_dim % chunkSize != 0 || y_dim % chunkSize != 0)
			return;

		unsigned int frameWidth = cellWidth * totalCells_x;
		unsigned int frameHeight = cellHeight * totalCells_y;

		double x_origin = map(x_dim, 0, frameWidth, -xMax, xMax) + offset_x;
		double y_origin = map(y_dim, 0, frameHeight, -yMax, yMax) + offset_y;

		double x = 0.0;
		double y = 0.0;

		double xx = 0.0;
		double yy = 0.0;

		int iteration = 0;

		if ((x_dim / chunkSize) % 2 == 0 && (y_dim / chunkSize) % 2 == 0 && chunkSize != maxChunkSize)
			return;

		while (xx + yy <= 4 && iteration < max_iteration) {
			double xtemp = xx - yy + x_origin;
			double ytemp = 2 * x * y + y_origin;

			if (x == xtemp && y == ytemp)
			{
				iteration = max_iteration;
				break;
			}

			x = xtemp;
			y = ytemp;

			xx = x * x;
			yy = y * y;

			iteration++;
		}
		int color = 0;
		if (iteration == max_iteration) {
			color = 255 << 24;
		}
		else {
			color = color_from_iter(iteration, xx + yy, palette, paletteLength);
		}
		for (int j = y_dim; j < y_dim + chunkSize; j++)
		{
			for (int i = x_dim; i < x_dim + chunkSize; i++)
			{
				int index = i + j * frameWidth;
				if(index < frameWidth * frameHeight)
					out[index] = color;
			}
		}
	}
}