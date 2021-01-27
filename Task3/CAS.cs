namespace Task3
{
    public class InsertInfo : Info
    {
        public INode newInternal;

        public InsertInfo(InternalNode parent, INode leaf, INode newInternal) : base(parent, leaf)
        {
            this.newInternal = newInternal;
        }
    }
    public class DeleteInfo : Info
    {
        public INode grandparent;
        public CAS parentUpdate;

        public DeleteInfo(InternalNode parent, INode leaf, INode grandparent) : base(parent, leaf)
        {
            this.grandparent = grandparent;
            parentUpdate = this.parent.update;
        }
    }
    public enum State
    {
        Clean,
        Mark,
        IFlag,
        DFlag
    }
    public class CAS
    {
        //support cas operation. How
        
        //CASValue is either new=0 or old=1, use for checking if cas was successful
        public byte CASValue;
        
        //state and info should be one memory word
        public State state;
        public Info info;

        public CAS()
        {
            state = State.Clean;
            info = null;
        }

        public void ChangeValue()
        {
            CASValue = CASValue == 0 ? (byte) 1 : (byte) 0;
        }
    }
}