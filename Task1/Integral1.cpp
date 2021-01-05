#include <iostream>
#include <thread>
#include <string>
#include <vector>
#include <mutex>
#include <math.h>

using namespace std;

double integral{0.0};

void integral_thread(double x1, double x2, double local_dx)
{
    double local_integral{0.0};
    for(double x = x1; x <+ x2; x += local_dx)
    {
        integral += 3 * pow(x, 3.0) + cos(7.0*x) - log(2.0*x);
    }
}
int main()
{
    const double e = 2.71828182845904523536;
    vector<thread> threads;
    int thread_number{20};
    double x_max{40};
    double thread_dx = x_max/thread_number;
    double dx = pow(e, -5.0);
    for (int i = 0; i < thread_number; i++)
    {
        threads.push_back(thread(integral_thread, 1+i*thread_dx, (i+1)*thread_dx, dx));
    }
    for (auto &int_thread : threads) int_thread.join();
    integral *= dx;
    cout << integral << endl;
}
