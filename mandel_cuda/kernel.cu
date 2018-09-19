
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "cuComplex.h"
#include "math.h"

extern "C" {
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

	__global__ void render(int *out, int *palette, int paletteLength, int width, int height, double offset_x, double offset_y, double zoom, int max_iteration) {
		unsigned int x_dim = blockIdx.x*blockDim.x + threadIdx.x;
		unsigned int y_dim = blockIdx.y*blockDim.y + threadIdx.y;
		int index = width * y_dim + x_dim;

		double scaleFactor = ((double)width / (double)height) * 2;


		double x_origin = map(x_dim, 0, width, -scaleFactor / zoom, scaleFactor / zoom) + offset_x;
		double y_origin = map(y_dim, 0, height, -2 / zoom, 2 / zoom) + offset_y;

		double x = 0.0;
		double y = 0.0;

		int iteration = 0;
		while (x*x + y * y <= 4 && iteration < max_iteration) {
			double xtemp = x * x - y * y + x_origin;
			y = 2 * x*y + y_origin;
			x = xtemp;
			iteration++;
		}

		if (iteration == max_iteration) {
			out[index] = 0;
		}
		else {
			out[index] = color_from_iter(iteration, x * x + y * y, palette, paletteLength);
		}
	}
}