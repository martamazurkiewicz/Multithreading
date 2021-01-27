namespace Task3
{
    public abstract class Info
    {
        public InternalNode parent;
        public INode leaf;

        protected Info(InternalNode parent, INode leaf)
        {
            this.leaf = leaf;
            this.parent = parent;
        }
    }
}