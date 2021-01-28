using System;


namespace Task3
{
    public class BTS
    {
        public readonly InternalNode root;

        public BTS()
        {
            var leftChild = new LeafNode(int.MinValue);
            var rightChild = new LeafNode(int.MaxValue);
            root = new InternalNode(int.MaxValue, leftChild, rightChild);
        }

        private bool IsDeletionPermitted(int key) => key != int.MinValue && key != int.MaxValue;
        
        public (InternalNode grandparent, InternalNode parent, LeafNode leaf, Update pUpdate, Update gpUpdate) Search(int key)
        {
            //if l → key != ∞1, then the following three statements hold:
            InternalNode grandparent = root;
            InternalNode parent = root;
            Node leaf = root;
            Update grandparentUpdate = new Update(grandparent.update);
            Update parentUpdate = new Update(parent.update);
            while (leaf is InternalNode)
            {
                grandparent = parent;
                parent = (InternalNode) leaf;
                grandparentUpdate = parentUpdate;
                parentUpdate = new Update(parent.update);
                leaf = key < leaf.key ? parent.left : parent.right;
            }
            return (grandparent, parent, (LeafNode) leaf, parentUpdate, grandparentUpdate);
        }
        public LeafNode Find(int key)
        {
            var leaf = Search(key).leaf;
            return leaf.key == key ? leaf : null;
        }

        public bool Delete(int key)
        {
            if (IsDeletionPermitted(key))
            {
                return true;
            }
            return false;
        }
        
        public bool Insert(int key)
        {
            InternalNode parent;
            InternalNode newInternalNode;
            LeafNode leaf;
            LeafNode newSibling;
            var newNode = new LeafNode(key);
            Update parentUpdate;
            Update result;
            InsertInfo operation;
            while (true)
            {
                var searchResult = Search(key);
                parent = searchResult.parent;
                leaf = searchResult.leaf;
                parentUpdate = searchResult.pUpdate;
                if (leaf.key == key)
                    return false;
                if (parentUpdate.state != State.Clean)
                    Help(parentUpdate);
                else
                {
                    newSibling = new LeafNode(leaf.key);
                    var newInternalNodeKey = key > leaf.key ? key : leaf.key;
                    var leftChild = newNode.key < newSibling.key ? newNode : newSibling;
                    var rightChild = newNode.key < newSibling.key ? newSibling : newNode;
                    newInternalNode = new InternalNode(newInternalNodeKey, leftChild, rightChild);
                    operation = new InsertInfo(parent, leaf, newInternalNode);
                    //This must be converted into CAS operation
                    //result := CAS(p → update, pupdate, <IFlag, op>)
                    //if parent.update == pupdate then operation=State.IFlag
                    //If R is a CAS object, then CAS(R, old, new) changes the value of
                    //R to new if the object’s value was old, in which case we say the CAS was successful.
                    result = parentUpdate.CAS(parentUpdate, new Update(State.IFlag, operation));
                    //update must have a CAS comparer!
                    if (result == parentUpdate)
                    {
                        HelpInsert(operation);
                        return true;
                    }
                    Help(result);
                }
            }
        }

        public void HelpInsert(InsertInfo operation)
        {
            //CAS-Child(op → p, op → l, op → newInternal) ⊲ ichild CAS
            //67 CAS(op → p → update, hIFlag, opi, hClean, opi) ⊲ iunflag CAS
        }
        
    }
    
    public class LeafNode : Node
    {
        public LeafNode(int key) : base(key) { }
    }

    public class InternalNode : Node
    {
        public Node left;
        public Node right;
        public Update update;

        public InternalNode(int key, Node left, Node right)
        :base(key)
        {
            this.left = left;
            this.right = right;
            update = new Update();
        }
    }
}