using System;
using System.Threading;

namespace Task2
{
    class Shuffling
    {
        public static int[] ShuffleSimultaneously(int arrayLength, int numberOfThreads)
        {
            object[] mutexes = new object[arrayLength];
            var array = new int[arrayLength];
            for (var i = 0; i < arrayLength; i++)
            {
                array[i] = i;
                mutexes[i] = new object();
            }
            var shufflingThreads = new ShufflingThread[numberOfThreads];
            var threads = new Thread[numberOfThreads];
            for (var i = 0; i < numberOfThreads; i++)
            {
                shufflingThreads[i] = new ShufflingThread(array, mutexes, 1000);
                threads[i] = new Thread(shufflingThreads[i].Shuffle);
                threads[i].Start();
            }

            for (var i = 0; i < numberOfThreads; i++)
                threads[i].Join();
            return array;
        }

        private class ShufflingThread
        {
            private readonly int[] _array;
            private readonly int _numberOfShuffles;
            private readonly Random _random;
            private readonly object[] _mutexes;

            public ShufflingThread(int[] array, object[] mutexes, int numberOfShuffles)
            {
                _array = array;
                _numberOfShuffles = numberOfShuffles;
                _random = new Random();
                _mutexes = mutexes;
            }

            public void Shuffle()
            {
                int j, k, tmp_j;
                for (var i = 0; i < _numberOfShuffles; i++)
                {
                    j = _random.Next(_array.Length);
                    k = _random.Next(_array.Length);
                    if (j > k)
                    {
                        tmp_j = j;
                        j = k;
                        k = tmp_j;
                    }
                    lock(_mutexes[j])
                    {
                        lock (_mutexes[k])
                        {
                            var tmp = _array[j];
                            _array[j] = _array[k];
                            _array[k] = tmp;
                        }
                    }
                }
            }
        }
    }

    internal class DinnerTable
    {
        private readonly SemaphoreSlim[] _books;
        private readonly SemaphoreSlim[] _forks;
        private readonly Philosopher[] _philosophers;
        private readonly Thread[] _threads;

        public DinnerTable(int philosophersNumber, int booksNumber)
        {
            _forks = new SemaphoreSlim[philosophersNumber];
            _books = new SemaphoreSlim[booksNumber];
            for (var i = 0; i < philosophersNumber; i++) _forks[i] = new SemaphoreSlim(1, 1);
            for (var i = 0; i < booksNumber; i++) _books[i] = new SemaphoreSlim(1, 1);
            _philosophers = new Philosopher[philosophersNumber];
            ChooseChairs();
            _threads = new Thread[philosophersNumber];
            StartDinner();
            WaitForDinnerToFinish();
            Console.WriteLine("Dinner finished");
        }

        private void ChooseChairs()
        {
            var chairs = Shuffling.ShuffleSimultaneously(_philosophers.Length, 4);
            for (var i = 0; i < _philosophers.Length; i++)
            {
                _philosophers[i] = new Philosopher(i, _forks, _books, chairs[i]);
                Console.WriteLine($"Philosopher {i} is sitting on chair {chairs[i]}");
            }
        }

        private void StartDinner()
        {
            for (var i = 0; i < _philosophers.Length; i++)
            {
                _threads[i] = new Thread(_philosophers[i].Dine);
                _threads[i].Start();
            }
        }

        private void WaitForDinnerToFinish()
        {
            for (var i = 0; i < _philosophers.Length; i++) _threads[i].Join();
        }

        private class Philosopher
        {
            private readonly int _number;
            private readonly int _right, _left;
            private readonly SemaphoreSlim[] _books;
            private readonly SemaphoreSlim[] _forks;
            private readonly bool[] _unreadBooks;

            public Philosopher(int number, SemaphoreSlim[] forks, SemaphoreSlim[] books, int left)
            {
                _number = number;
                _forks = forks;
                _books = books;
                _unreadBooks = new bool[books.Length];
                for (var i = 0; i < books.Length; i++) _unreadBooks[i] = true;
                _left = left;
                _right = (left + 1) % forks.Length;
            }

            private void Think()
            {
                Console.WriteLine($"Philosopher {_number} thinks...");
                Console.WriteLine($"Philosopher {_number} is hungry");
            }

            private void ReadAndEat()
            {
                for (var i = 0; i < _books.Length; i++)
                    if (_unreadBooks[i] && _books[i].CurrentCount == 1)
                    {
                        Eat(i);
                        return;
                    }

                for (var i = 0; i < _books.Length; i++)
                    if (_unreadBooks[i])
                    {
                        Eat(i);
                        return;
                    }
            }

            private void Eat(int chosenBookNumber)
            {
                _books[chosenBookNumber].Wait();
                _unreadBooks[chosenBookNumber] = false;
                Console.WriteLine($"Philosopher {_number} is eating and reading book {chosenBookNumber}...");
                _books[chosenBookNumber].Release();
                Console.WriteLine($"Philosopher {_number} finished eating");
            }

            private bool HasBooksToRead()
            {
                foreach (var unreadBook in _unreadBooks)
                    if (unreadBook)
                        return true;

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
                    if (_left == _forks.Length - 1)
                    {
                        _forks[_right].Wait();
                        _forks[_left].Wait();
                    }
                    else
                    {
                        _forks[_left].Wait();
                        _forks[_right].Wait();
                    }

                    ReadAndEat();
                    _forks[_right].Release();
                    _forks[_left].Release();
                }
            }
        }
    }

     class Program
    {
        private static void Main(string[] args)
        {
            var dinnerTable = new DinnerTable(20, 50);
        }
    }
}

