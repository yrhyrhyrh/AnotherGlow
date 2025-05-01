using appBackend.Interfaces.GlobalPostWall;

namespace appBackend.Models.PostChildrenComponents
{
    public class CommentComponent : PostComponent
    {
        public Comment Comment { get; set; }

        public override string Type => "Comment";
        public CommentComponent(Comment comment) { Comment = comment; }
    }
}
