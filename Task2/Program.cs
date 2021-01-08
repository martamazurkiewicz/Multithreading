using System;
using System.Threading;

namespace Task2
{
    class DinnerTable
    {
        private readonly int _philosophersNumber;
        private SemaphoreSlim[] forks;
        private SemaphoreSlim[] books;
        private Philosopher[] philosophers;
        private Thread[] threads;

        public DinnerTable(int philosophersNumber, int booksNumber)
        {
            _philosophersNumber = philosophersNumber;
            forks = new SemaphoreSlim[philosophersNumber];
            books = new SemaphoreSlim[booksNumber];
            for (var i = 0; i < philosophersNumber; i++)
            {
                forks[i] = new SemaphoreSlim(1, 1);
            }
            for (var i = 0; i < booksNumber; i++)
            {
                books[i] = new SemaphoreSlim(1, 1);
            }

            philosophers = new Philosopher[philosophersNumber];
            threads = new Thread[philosophersNumber];
            StartDinner();
        }

        private void StartDinner()
        {
            for (var i = 0; i < _philosophersNumber; i++)
            {
                philosophers[i] = new Philosopher(i, forks, books);
                threads[i] = new Thread(philosophers[i].Dine);
                threads[i].Start();
            }
        }

        class Philosopher
            {
                private readonly int _number;
                //private readonly Random _rn;
                private int right, left;
                private SemaphoreSlim[] forks;
                private SemaphoreSlim[] books;
                private bool[] readBooks;

                public Philosopher(int number, SemaphoreSlim[] forks, SemaphoreSlim[] books)
                {
                    _number = number;
                    //_rn = new Random();
                    this.forks = forks;
                    this.books = books;
                    readBooks = new bool[books.Length];
                    for (var i = 0; i < books.Length; i++)
                    {
                        readBooks[i] = false;
                    }
                    left = number;
                    right = (number + 1) % forks.Length;
                }
        
                private void Think()
                {
                    Console.WriteLine($"Philosopher {_number} thinks...");
                    //Thread.Sleep(_rn.Next(500, 1000));
                    Console.WriteLine($"Philosopher {_number} is hungry");
                }
                
                private bool ReadAndEat()
                {
                    for (var i = 0; i < books.Length; i++)
                    {
                        if (!readBooks[i])
                        {
                            books[i].Wait();
                            Eat();
                            readBooks[i] = true;
                            books[i].Release();
                            return false;
                        }
                    }
                    return true;
                }

                private void Eat()
                {
                    Console.WriteLine($"Philosopher {_number} eats...");
                    //Thread.Sleep(_rn.Next(500, 1000));
                    for (int i = 0; i < readBooks.Length; i++)
                    {
                        Console.Write(readBooks[i]);
                    }
                    Console.WriteLine($"Philosopher {_number} finished eating");
                }
        
                public void Dine()
                {
                    var hasReadAllBooks = false;
                    while (!hasReadAllBooks)
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
                        hasReadAllBooks = ReadAndEat();
                        forks[right].Release();
                        forks[left].Release();
                    }
                }
        
            }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var dinnerTable = new DinnerTable(5, 10);
            //Console.WriteLine("Dinner finished!");
        }
    }
}