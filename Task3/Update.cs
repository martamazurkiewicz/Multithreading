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

        public DeleteInfo(InternalNode parent, LeafNode leaf, InternalNode grandparent, Update parentUpdate) : base(parent, leaf)
        {
            this.grandparent = grandparent;
            this.parentUpdate = parentUpdate;
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

        public static Update CAS(Update CASObject, Update oldValue, Update newValue)
        {
            //If R is a CAS object, then CAS(R, old, new) changes the value of
            //R to new if the object’s value was old, in which case we say the CAS was successful.
            //CAS always returns the value the object had prior to the operation.
            //How CAS know how to compare Update (implement IComparable ?)
            var result = Interlocked.CompareExchange<Update>(ref CASObject, newValue, oldValue);
            return result;
        }

        public override string ToString() => info == null ? state + " null" : state + " " + info;

        // public override bool Equals(object obj) =>
        //     state == ((Update) obj).state && info == ((Update) obj).info;
        //
        // public static bool operator ==(Update update1, Update update2) => update1.Equals(update2);
        //
        // public static bool operator !=(Update update1, Update update2) => !(update1 == update2);
    }
}