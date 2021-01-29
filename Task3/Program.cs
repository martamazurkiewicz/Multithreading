using System;

namespace Task3
{
    class Program
    {
        static void Main(string[] args)
        {
            var bst = new BST();
            bst.Insert(7);
            bst.Insert(32);
            bst.Insert(-38);
            Console.WriteLine(bst.ToString());
        }
    }
}