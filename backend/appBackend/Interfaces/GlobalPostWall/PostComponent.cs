namespace appBackend.Interfaces.GlobalPostWall
{
    public abstract class PostComponent
    {
        public abstract string Type { get; }

        public virtual void Add(PostComponent component) { throw new NotImplementedException(); }

    }

}
