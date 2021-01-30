namespace Task3
{
    public abstract class Info
    {
        public InternalNode parent;
        public LeafNode leaf;

        protected Info(InternalNode parent, LeafNode leaf)
        {
            this.leaf = leaf;
            this.parent = parent;
        }
    }
}