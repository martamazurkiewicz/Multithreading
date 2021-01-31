using System;
using System.Threading;


namespace Task3
{
    public class BST
    {
        public readonly InternalNode root;

        public BST()
        {
            var leftChild = new InternalNode(int.MaxValue-1, null, new LeafNode(int.MaxValue-1));
            var rightChild = new LeafNode(int.MaxValue);
            root = new InternalNode(int.MaxValue, leftChild, rightChild);
        }

        private bool IsDeletionPermitted(int key) => key != int.MaxValue-1 && key != int.MaxValue;

        public override string ToString() => ToStringRec(root);
        private string ToStringRec(Node tmpRoot)
        {
            if (tmpRoot is LeafNode)
                return tmpRoot.key + " ";
            string result = "";
            result += tmpRoot.key + " ";
            result += ToStringRec(((InternalNode)tmpRoot).left);
            result += ToStringRec(((InternalNode)tmpRoot).right);
            return result;
        }

        public bool IsBSTCorrect(int bstMinValue) => IsBSTCorrectRec(root, bstMinValue, Int32.MaxValue);
        private bool IsBSTCorrectRec(Node node, int min, int max) 
        {
            if (node.key < min || node.key > max)
                return false;
            if (node is LeafNode)
                return true;
            return IsBSTCorrectRec(((InternalNode)node).left, min, node.key - 1) && IsBSTCorrectRec(((InternalNode)node).right, node.key, max); 
        }  
        
        private (InternalNode grandparent, InternalNode parent, LeafNode leaf, Update pUpdate, Update gpUpdate) Search(int key)
        {
            InternalNode grandparent = root;
            InternalNode parent = root;
            Node leaf = root;
            Update grandparentUpdate = new Update();
            Update parentUpdate = new Update();
            while (leaf is InternalNode)
            {
                grandparent = parent;
                parent = (InternalNode) leaf;
                grandparentUpdate = parentUpdate;
                parentUpdate = parent.update;
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
            if (!IsDeletionPermitted(key)) return false;
            while (true)
            {
                var searchResult = Search(key);
                InternalNode grandparent = searchResult.grandparent;
                InternalNode parent = searchResult.parent;
                LeafNode leaf = searchResult.leaf;
                Update parentUpdate = searchResult.pUpdate;
                Update grandparentUpdate = searchResult.gpUpdate;
                if (leaf.key != key)
                    return false;
                if (grandparentUpdate.state != State.Clean)
                    Help(grandparentUpdate);
                else if (parentUpdate.state != State.Clean)
                    Help(parentUpdate);
                else
                {
                    DeleteInfo operation = new DeleteInfo(parent, leaf, grandparent, parentUpdate);
                    Update result = Update.CAS(grandparent.update, grandparentUpdate, new Update(State.DFlag, operation));
                    if (result == grandparentUpdate)
                    {
                        if (HelpDelete(operation))
                            return true;
                    }
                    else
                    {
                        Help(result);
                    }
                }
            }
        }

        private bool HelpDelete(DeleteInfo operation)
        {
            var result = Update.CAS(operation.parent.update, operation.parentUpdate, new Update(State.Mark, operation));
            if (result == operation.parentUpdate || result == new Update(State.Mark, operation))
            {
                HelpMarked(operation);
                return true;
            }
            Help(result);
            Update.CAS(operation.grandparent.update, new Update(State.DFlag, operation),
                new Update(State.Clean, operation));
            return false;
        }

        private void HelpMarked(DeleteInfo operation)
        {
            Node other = operation.parent.right == operation.leaf ? operation.parent.left : operation.parent.right;
            if(other.key < operation.parent.key)
                Interlocked.CompareExchange<Node>(ref operation.grandparent.left, other, operation.parent);
            else
                Interlocked.CompareExchange<Node>(ref operation.grandparent.right, other, operation.parent);
            Update.CAS(operation.grandparent.update, new Update(State.DFlag, operation),
                new Update(State.Clean, operation));
        }

        private void Help(Update update)
        {
            if (update.state == State.IFlag)
                HelpInsert((InsertInfo)update.info);
            else if(update.state == State.Mark)
                HelpMarked((DeleteInfo)update.info);
            else if (update.state == State.DFlag)
                HelpDelete((DeleteInfo) update.info);
            
        }
        
        public bool Insert(int key)
        {
            Interlocked.CompareExchange(ref ((InternalNode)root.left).left, new LeafNode(key), null);
            var newNode = new LeafNode(key);
            while (true)
            {
                var searchResult = Search(key);
                InternalNode parent = searchResult.parent;
                LeafNode leaf = searchResult.leaf;
                Update parentUpdate = searchResult.pUpdate;
                if (leaf.key == key)
                    return false;
                if (parentUpdate.state != State.Clean)
                    Help(parentUpdate);
                else
                {
                    LeafNode newSibling = new LeafNode(leaf.key);
                    var newInternalNodeKey = key > leaf.key ? key : leaf.key;
                    var leftChild = newNode.key < newSibling.key ? newNode : newSibling;
                    var rightChild = newNode.key < newSibling.key ? newSibling : newNode;
                    InternalNode newInternalNode = new InternalNode(newInternalNodeKey, leftChild, rightChild);
                    InsertInfo operation = new InsertInfo(parent, leaf, newInternalNode);
                    var newUpdate = new Update(State.IFlag, operation);
                    Update result = Update.CAS(parent.update, parentUpdate, newUpdate);
                    if (result == parentUpdate)
                    {
                        HelpInsert(operation);
                        return true;
                    }
                    Help(result);
                }
            }
        }

        private void HelpInsert(InsertInfo operation)
        {
            if(operation.newInternal.key < operation.parent.key)
                Interlocked.CompareExchange(ref operation.parent.left, operation.newInternal, operation.leaf);
            else
                Interlocked.CompareExchange(ref operation.parent.right, operation.newInternal, operation.leaf);
            Update.CAS(operation.parent.update, new Update(State.IFlag, operation), new Update(State.Clean, operation));
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
        public override string ToString() => $"{base.ToString()}, leftChild: {left.key}, rightChild: {right.key}";
    }
}