namespace Task3
{
    public abstract class Node
    {
        public readonly int key;

        protected Node(int key)
        {
            this.key = key;
        }

        public override string ToString() => $"Key: {key}";
    }
}