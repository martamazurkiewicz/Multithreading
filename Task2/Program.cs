using System;
using System.Threading;

namespace Task2
{
    class Philosopher
    {
        private readonly int _number;
        private readonly Random _rn;
        private int right, left;
        private SemaphoreSlim[] forks;

        public Philosopher(int number, SemaphoreSlim[] forks)
        {
            _number = number;
            _rn = new Random();
            this.forks = forks;
            left = number;
            right = (number + 1) % forks.Length;
        }

        private void Think()
        {
            Console.WriteLine($"Philosopher {_number} thinks...");
            //Thread.Sleep(_rn.Next(500, 1000));
            Console.WriteLine($"Philosopher {_number} is hungry");
        }
        
        private void Eat()
        {
            Console.WriteLine($"Philosopher {_number} eats...");
            //Thread.Sleep(_rn.Next(500, 1000));
            Console.WriteLine($"Philosopher {_number} finished eating");
        }

        public void Dine()
        {
            for (;;)
            {
                Think();
                if (_number == forks.Length - 1)
                {
                    forks[right].Wait();
                    forks[left].Wait();
                }
                else
                {
                    forks[left].Wait();
                    forks[right].Wait();
                }

                Eat();
                forks[right].Release();
                forks[left].Release();
            }
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            const int philosopherNumber = 5;
            SemaphoreSlim[] forks = new SemaphoreSlim[philosopherNumber];
            for (int i = 0; i < philosopherNumber; i++)
            {
                forks[i] = new SemaphoreSlim(1, 1);
            }
            Philosopher[] philosophers = new Philosopher[philosopherNumber];
            Thread[] threads = new Thread[philosopherNumber];
            for (int i = 0; i < philosopherNumber; i++)
            {
                philosophers[i] = new Philosopher(i, forks);
                threads[i] = new Thread(philosophers[i].Dine);
                threads[i].Start();
            }
        }
    }
}