﻿using System;
using System.Threading;


namespace Task3
{
    public class BST
    {
        public readonly InternalNode root;

        public BST()
        {
            var leftChild = new LeafNode(int.MinValue);
            var rightChild = new LeafNode(int.MaxValue);
            root = new InternalNode(int.MaxValue, leftChild, rightChild);
        }

        private bool IsDeletionPermitted(int key) => key != int.MinValue && key != int.MaxValue;

        public override string ToString() => ToStringRecursive(root);

        private string ToStringRecursive(Node tmpRoot)
        {
            string result = "";
            if (tmpRoot is LeafNode)
                return tmpRoot.key.ToString();
            result += (ToStringRecursive(((InternalNode)tmpRoot).left) + " ");
            result += (ToStringRecursive(((InternalNode)tmpRoot).right) + " ");
            result += tmpRoot.key;
            return result;
        }
        
        private (InternalNode grandparent, InternalNode parent, LeafNode leaf, Update pUpdate, Update gpUpdate) Search(int key)
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
                        result = Update.CAS(grandparent.update, grandparentUpdate, new Update(State.DFlag, operation));
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
            var result = Update.CAS(operation.parent.update, operation.parentUpdate, new Update(State.Mark, operation));
            if (result == operation.parentUpdate || result == new Update(State.Mark, operation))
            {
                HelpMarked(operation);
                return true;
            }
            else
            {
                Help(result);
                Update.CAS(operation.grandparent.update, new Update(State.DFlag, operation),
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
            InternalNode.CASChild(operation.grandparent, operation.parent, other);
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
                Console.WriteLine(searchResult);
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
                    Console.WriteLine("newInternalNode");
                    Console.WriteLine(newInternalNode);
                    operation = new InsertInfo(parent, leaf, newInternalNode);
                    //This must be converted into CAS operation
                    //result := CAS(p → update, pupdate, <IFlag, op>)
                    //if parent.update == pupdate then operation=State.IFlag
                    //If R is a CAS object, then CAS(R, old, new) changes the value of
                    //R to new if the object’s value was old, in which case we say the CAS was successful.
                    result = Update.CAS(parent.update, parentUpdate, new Update(State.IFlag, operation));
                    Console.WriteLine("result");
                    Console.WriteLine(result);
                    Console.WriteLine("parentUpdate");
                    Console.WriteLine(parentUpdate);
                    Console.WriteLine("parentUpdate == result");
                    Console.WriteLine(parentUpdate == result);
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
            //CAS-Child(op → p, op → l, op → newInternal) ⊲ ichild CAS
            Console.WriteLine(operation.parent.ToString() + " " + operation.leaf.ToString() + " " + operation.newInternal.ToString());
            InternalNode.CASChild(operation.parent, operation.leaf, operation.newInternal);
            //67 CAS(op → p → update, hIFlag, opi, hClean, opi) ⊲ iunflag CAS
            Update.CAS(operation.parent.update, new Update(State.IFlag, operation), new Update(State.Clean, operation));
            Console.WriteLine(operation.parent.update);
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

        public static void CASChild(InternalNode parent, Node oldNode, Node newNode)
        {
            if(newNode.key < parent.key)
                Interlocked.CompareExchange<Node>(ref parent.left, oldNode, newNode);
            else
                Interlocked.CompareExchange<Node>(ref parent.right, oldNode, newNode);
        }

        public override string ToString() => base.ToString() + " " + left.key + " " + right.key;
    }
}