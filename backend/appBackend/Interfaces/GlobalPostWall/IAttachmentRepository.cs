using appBackend.Models;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface IAttachmentRepository
    {
        Task<Attachment> AddAttachmentAsync(Attachment attachment);
    }
}
