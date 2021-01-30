using System;
using System.Collections.Generic;
using System.Threading;

namespace Task3
{
    class Program
    {
       static void InsertValuesIntoBST(BST bst, int[] numbersArray)
        {
            foreach (var value in numbersArray)
                bst.Insert(value);
        }
        static void Main(string[] args)
        {
            var bst = new BST();
            List<int[]> randomNumbersLists = new List<int[]>();
            Random seedGenerator = new Random();
            for (int i = 0; i < 5; i++)
            {
                randomNumbersLists.Add(new int[50]);
                var random = new Random(seedGenerator.Next(-200000,200000));
                for (int j = 0; j < 5; j++)
                {
                    randomNumbersLists[i][j] = random.Next(0, 999);
                }
            }
            Thread[] threads = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                var randomNumbersList = randomNumbersLists[i];
                threads[i] = new Thread(() => InsertValuesIntoBST(bst, randomNumbersList));
                threads[i].Start();
            }
            foreach (var thread in threads)
                thread.Join();
            bst.Insert(0);
            Console.WriteLine(bst.ToString());
            Console.WriteLine(bst.IsBSTCorrect());
        }
    }
}