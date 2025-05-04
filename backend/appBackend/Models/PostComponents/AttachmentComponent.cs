using appBackend.Dtos.GlobalPostWall;
using appBackend.Interfaces.GlobalPostWall;

namespace appBackend.Models.PostChildrenComponents
{
    public class AttachmentComponent : PostComponent
    {
        private Attachment _attachment;

        public AttachmentComponent(Attachment attachment)
        {
            _attachment = attachment;
        }

        public override string Type => "Attachment";

        public override object ToDTO(Guid currentUserId)
        {
            return new AttachmentDTO
            {
                AttachmentId = _attachment.AttachmentId,
                FileName = _attachment.FileName,
                FilePath = _attachment.FilePath,
                ContentType = _attachment.ContentType,
                FileSize = _attachment.FileSize
            };
        }
    }
}
