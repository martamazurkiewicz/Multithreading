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

        public override string ToString() => ToStringRecursive(root);

        public bool IsBSTCorrect() => isBSTCorrectRec(root, 0, Int32.MaxValue);
        private bool isBSTCorrectRec(Node node, int min, int max) 
        { 
            /* false if this node violates the min/max constraints */
            if (node.key < min || node.key > max)
                return false;
            /* an empty tree is BST */
            if (node is LeafNode)
                return true;
            /* otherwise check the subtrees recursively  
            tightening the min/max constraints */
            // Allow only distinct values  
            return isBSTCorrectRec(((InternalNode)node).left, min, node.key - 1) && isBSTCorrectRec(((InternalNode)node).right, node.key, max); 
        }  

        private string ToStringRecursive(Node tmpRoot)
        {
            if (tmpRoot is LeafNode)
                return tmpRoot.key + " ";
            string result = "";
            result += tmpRoot.key + " ";
            result += (ToStringRecursive(((InternalNode)tmpRoot).left) + " ");
            result += (ToStringRecursive(((InternalNode)tmpRoot).right) + " ");
            return result;
        }
        
        private (InternalNode grandparent, InternalNode parent, LeafNode leaf, Update pUpdate, Update gpUpdate) Search(int key)
        {
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
            if (IsDeletionPermitted(key))
            {
                InternalNode grandparent;
                InternalNode parent;
                LeafNode leaf;
                Update parentUpdate, grandparentUpdate, result;
                DeleteInfo operation;
                while (true)
                {
                    var searchResult = Search(key);
                    grandparent = searchResult.grandparent;
                    parent = searchResult.parent;
                    leaf = searchResult.leaf;
                    parentUpdate = searchResult.pUpdate;
                    grandparentUpdate = searchResult.gpUpdate;
                    if (leaf.key != key)
                        return false;
                    if (grandparentUpdate.state != State.Clean)
                        Help(grandparentUpdate);
                    else if (parentUpdate.state != State.Clean)
                        Help(parentUpdate);
                    else
                    {
                        operation = new DeleteInfo(parent, leaf, grandparent, parentUpdate);
                        result = Update.CAS(ref grandparent.update, grandparentUpdate, new Update(State.DFlag, operation));
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
            return false;
        }

        private bool HelpDelete(DeleteInfo operation)
        {
            var result = Update.CAS(ref operation.parent.update, operation.parentUpdate, new Update(State.Mark, operation));
            if (result == operation.parentUpdate || result == new Update(State.Mark, operation))
            {
                HelpMarked(operation);
                return true;
            }
            else
            {
                Help(result);
                Update.CAS(ref operation.grandparent.update, new Update(State.DFlag, operation),
                    new Update(State.Clean, operation));
                return false;
            }
        }

        private void HelpMarked(DeleteInfo operation)
        {
            Node other;
            if (operation.parent.right == operation.leaf)
                other = operation.parent.left;
            else
                other = operation.parent.right;
            //InternalNode.CASChild(ref operation.grandparent, operation.parent, other);
            if(other.key < operation.parent.key)
                Interlocked.CompareExchange<Node>(ref operation.grandparent.left, other, operation.parent);
            else
                Interlocked.CompareExchange<Node>(ref operation.grandparent.right, other, operation.parent);
            Update.CAS(ref operation.grandparent.update, new Update(State.DFlag, operation),
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
                    var newUpdate = new Update(State.IFlag, operation);
                    result = Update.CAS(ref parent.update, parentUpdate, newUpdate);
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

        private void HelpInsert(InsertInfo operation)
        {
            if(operation.newInternal.key < operation.parent.key)
                Interlocked.CompareExchange(ref operation.parent.left, operation.newInternal, operation.leaf);
            else
                Interlocked.CompareExchange(ref operation.parent.right, operation.newInternal, operation.leaf);
            Update.CAS(ref operation.parent.update, new Update(State.IFlag, operation), new Update(State.Clean, operation));
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

        // public static void CASChild(ref InternalNode parent, Node oldNode, Node newNode)
        // {
        //     if(newNode.key < parent.key)
        //         Interlocked.CompareExchange<Node>(ref parent.left, oldNode, newNode);
        //     else
        //         Interlocked.CompareExchange<Node>(ref parent.right, oldNode, newNode);
        // }

        public override string ToString() => base.ToString() + " " + left.key + " " + right.key;
    }
}