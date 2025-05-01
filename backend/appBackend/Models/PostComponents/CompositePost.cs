using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models.PostChildrenComponents;

namespace appBackend.Models.PostComponents
{
    public class CompositePost : PostComponent
    {
        public Post Post { get; set; }
        public List<PostComponent> Children { get; set; } = new();

        public override string Type => "Post";

        public CompositePost(Post post)
        {
            Post = post;

            if (post.Comments != null)
                foreach (var c in post.Comments)
                    Children.Add(new CommentComponent(c));

            if (post.Attachments != null)
                foreach (var a in post.Attachments)
                    Children.Add(new AttachmentComponent(a));

            if (post.Likes != null)
                foreach (var l in post.Likes)
                    Children.Add(new LikeComponent(l));

            if (post.Poll != null)
                Children.Add(new PollComponent(post.Poll));
        }

        public override void Add(PostComponent component)
        {
            Children.Add(component);
        }
    }

}
