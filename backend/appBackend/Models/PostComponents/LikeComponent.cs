using appBackend.Interfaces.GlobalPostWall;

namespace appBackend.Models.PostChildrenComponents
{
    public class LikeComponent : PostComponent
    {
        public Like Like { get; set; }

        public override string Type => "Like";
        public LikeComponent(Like like) { Like = like; }
    }
}
