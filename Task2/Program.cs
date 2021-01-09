using System;
using System.Threading;

namespace Task2
{
    class DinnerTable
    {
        private SemaphoreSlim[] forks;
        private SemaphoreSlim[] books;
        private Philosopher[] philosophers;
        private Thread[] threads;
        private int[] chairs;

        public DinnerTable(int philosophersNumber, int booksNumber)
        {
            forks = new SemaphoreSlim[philosophersNumber];
            books = new SemaphoreSlim[booksNumber];
            chairs = new int[philosophersNumber];
            for (var i = 0; i < philosophersNumber; i++)
            {
                forks[i] = new SemaphoreSlim(1, 1);
                chairs[i] = i;
            }
            for (var i = 0; i < booksNumber; i++)
            {
                books[i] = new SemaphoreSlim(1, 1);
            }
            philosophers = new Philosopher[philosophersNumber];
            threads = new Thread[philosophersNumber];
            StartDinner();
            WaitForDinnerToFinish();
            Console.WriteLine("Dinner finished");
        }

        private void StartDinner()
        {
            ShuffleChairs(100);
            for (var i = 0; i < philosophers.Length; i++)
            {
                philosophers[i] = new Philosopher(i, forks, books, chairs[i]);
                Console.WriteLine($"Philosopher {i} is sitting on chair {chairs[i]}");
            }
            for (int i = 0; i < philosophers.Length; i++)
            {
                threads[i] = new Thread(philosophers[i].Dine);
                threads[i].Start();
            }
        }

        private void ShuffleChairs(int shuffleNumber)
        {
            var rng = new Random();
            int j, k;
            for (int i = 0; i < shuffleNumber; i++)
            {
                j = rng.Next(philosophers.Length);
                k = rng.Next(philosophers.Length);
                var tmp = chairs[j];
                chairs[j] = chairs[k];
                chairs[k] = tmp;
            }
        }

        private void WaitForDinnerToFinish()
        {
            for (var i = 0; i < philosophers.Length; i++)
            {
                threads[i].Join();
            }
        }

        class Philosopher
            {
                private readonly int _number;
                private int right, left;
                private SemaphoreSlim[] forks;
                private SemaphoreSlim[] books;
                private bool[] unreadBooks;

                public Philosopher(int number, SemaphoreSlim[] forks, SemaphoreSlim[] books, int left)
                {
                    _number = number;
                    this.forks = forks;
                    this.books = books;
                    unreadBooks = new bool[books.Length];
                    for (var i = 0; i < books.Length; i++)
                    {
                        unreadBooks[i] = true;
                    }
                    this.left = left;
                    right = (left + 1) % forks.Length;
                }
        
                private void Think()
                {
                    Console.WriteLine($"Philosopher {_number} thinks...");
                    Console.WriteLine($"Philosopher {_number} is hungry");
                }
                
                private void ReadAndEat()
                {
                    for (var i = 0; i < books.Length; i++)
                    {
                        if (unreadBooks[i] && books[i].CurrentCount == 1)
                        {
                            Eat(i);
                            return;
                        }
                    }
                    for (var i = 0; i < books.Length; i++)
                    {
                        if (unreadBooks[i])
                        {
                            Eat(i);
                            return;
                        }
                    }
                }

                private void Eat(int chosenBookNumber)
                {
                    books[chosenBookNumber].Wait();
                    unreadBooks[chosenBookNumber] = false;
                    Console.WriteLine($"Philosopher {_number} eats...");
                    Console.WriteLine($"Philosopher {_number} is reading book {chosenBookNumber}...");
                    Console.WriteLine($"Philosopher {_number} finished eating");
                    books[chosenBookNumber].Release();
                }

                private bool HasBooksToRead()
                {
                    for (int i = 0; i < unreadBooks.Length; i++)
                    {
                        if (unreadBooks[i])
                            return true;
                    }

                    return false;
                }
        
                public void Dine()
                {
                    while (true)
                    {
                        if (!HasBooksToRead())
                        {
                            Console.WriteLine($"Philosopher {_number} finished all books");
                            break;
                        }

                        Think();

                    /* if (left == forks.Length - 1)
                     {
                         forks[right].Wait();
                         forks[left].Wait();
                     }
                     else
                     {
                         forks[left].Wait();
                         forks[right].Wait();
                     }*/
                        while (true)
                        {
                            forks[left].Wait();
                            if (forks[right].Wait(0))
                            {
                                break;
                            }
                            else
                            {
                            forks[left].Release();
                            }
                        }
                       
                        ReadAndEat();
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
            var dinnerTable = new DinnerTable(10, 20);
        }
    }
}