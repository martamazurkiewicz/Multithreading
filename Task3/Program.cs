using System;
using System.Collections.Generic;
using System.Threading;

namespace Task3
{
    class Program
    {
        static void ModifyBST(int[] numbersArray, Func<int, bool> modifyingFunction)
        {
            foreach (var value in numbersArray)
                modifyingFunction(value);
        }
        static void Main(string[] args)
        {
            var bst = new BST();
            List<int[]> randomNumbersLists = new List<int[]>();
            Random seedGenerator = new Random();
            for (int i = 0; i < 7; i++)
            {
                randomNumbersLists.Add(new int[50000]);
                var random = new Random(seedGenerator.Next(-200000,200000));
                for (int j = 0; j < 50000; j++)
                {
                    randomNumbersLists[i][j] = random.Next(0, 50000);
                }
            }
            Thread[] threads = new Thread[8];
            for (int i = 0; i < 5; i++)
            {
                var randomNumbersList = randomNumbersLists[i];
                threads[i] = new Thread(() => ModifyBST(randomNumbersList, bst.Insert));
                threads[i].Start();
            }
            for (int i = 5; i < 7; i++)
            {
                var randomNumbersList = randomNumbersLists[i];
                threads[i] = new Thread(() => ModifyBST(randomNumbersList, bst.Delete));
                threads[i].Start();
            }
            var alreadyInsertedNumbersList = randomNumbersLists[2];
            threads[7] = new Thread(() => ModifyBST(alreadyInsertedNumbersList, bst.Delete));
            threads[7].Start();
            foreach (var thread in threads)
                thread.Join();
            bst.Insert(0);
            Console.WriteLine(bst.ToString());
            Console.WriteLine(bst.IsBSTCorrect(0));
        }
    }
}