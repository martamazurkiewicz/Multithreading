using System;
using System.Threading;

namespace Task3
{
    public class InsertInfo : Info
    {
        public InternalNode newInternal;

        public InsertInfo(InternalNode parent, LeafNode leaf, InternalNode newInternal) : base(parent, leaf)
        {
            this.newInternal = newInternal;
        }
    }
    public class DeleteInfo : Info
    {
        public InternalNode grandparent;
        public Update parentUpdate;

        public DeleteInfo(InternalNode parent, LeafNode leaf, InternalNode grandparent) : base(parent, leaf)
        {
            this.grandparent = grandparent;
            parentUpdate = new Update(this.parent.update);
        }
    }
    public enum State
    {
        Clean,
        Mark,
        IFlag,
        DFlag
    }
    public class Update
    {

        //state and info should be one memory word
        public State state;
        public Info info;

        public Update()
        {
            state = State.Clean;
            info = null;
        }

        public Update(Update oldUpdate)
        {
            state = oldUpdate.state;
            info = oldUpdate.info;
        }
        
        public Update(State state, Info info)
        {
            this.state = state;
            this.info = info;
        }

        public Update CAS(Update oldValue, Update newValue)
        {
            //If R is a CAS object, then CAS(R, old, new) changes the value of
            //R to new if the object’s value was old, in which case we say the CAS was successful.
            //CAS always returns the value the object had prior to the operation.
            //How CAS know how to compare Update (implement IComparable ?)
            var result = Interlocked.CompareExchange<Update>(ref this, newValue, oldValue);
            return result;
        }
    }
}