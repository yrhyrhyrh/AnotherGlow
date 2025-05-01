using appBackend.Interfaces.GlobalPostWall;

namespace appBackend.Models.PostChildrenComponents
{
    public class AttachmentComponent : PostComponent
    {
        public Attachment Attachment { get; set; }

        public override string Type => "Attachment";
        public AttachmentComponent(Attachment attachment) { Attachment = attachment; }
    }
}
