namespace Task3
{
    public class BTS
    {
        public INode root;

        public INode find(int key, INode tmpRoot)
        {
            while (true)
            {
                if (tmpRoot == null || tmpRoot.Key == key) 
                    return tmpRoot;
                tmpRoot = tmpRoot.Key < key ? ((InternalNode) tmpRoot).right : ((InternalNode) tmpRoot).left;
            }
        }

        public bool delete(int key)
        {
            
        }
    }




    
    public class LeafNode : INode
    {
        public int Key { get; }

        public LeafNode(int keyValue)
        {
            Key = keyValue;
        }
    }

    public class InternalNode : INode
    {
        public int Key { get; }
        public INode left;
        public INode right;
        public CAS update;
        
        
        public InternalNode(int keyValue, INode left, INode right)
        {
            Key = keyValue;
            this.left = left;
            this.right = right;
            update = new CAS();
            
        }

    }
}