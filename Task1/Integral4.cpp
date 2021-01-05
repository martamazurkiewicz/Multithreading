#include <iostream>
#include <thread>
#include <vector>
#include <mutex>
#include <math.h> 
#include <tbb/parallel_reduce.h>
#include <tbb/blocked_range.h>

using namespace tbb;
using namespace std;

const double e = 2.71828182845904523536;
int thread_number{4};
double dx = pow(e, -5.0);
double thread_dx = 40.0/thread_number;
int main()
{
    double integral = parallel_reduce(blocked_range<int>(0, thread_number), 0.0,
    [](const blocked_range<int> &r, double local_integral) 
    { 
        double thread_dx = 40.0/thread_number;
        for(double x = 1+r.begin()*thread_dx; x <= (r.begin()+1)*thread_dx; x += dx)
        {
            local_integral += 3 * pow(x, 3.0) + cos(7.0*x) - log(2.0*x);
        }
        return local_integral;
    },
    [](double x, double y) 
    {
        return x + y;
    });
    integral *= dx;
    cout << integral << endl;
}
