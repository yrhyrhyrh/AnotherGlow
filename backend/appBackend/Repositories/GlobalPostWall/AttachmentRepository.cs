using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;

namespace appBackend.Repositories.GlobalPostWall
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly SocialMediaDbContext _dbContext;

        public AttachmentRepository(SocialMediaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Attachment> AddAttachmentAsync(Attachment attachment)
        {
            _dbContext.Attachments.Add(attachment);
            await _dbContext.SaveChangesAsync();
            return attachment;
        }
    }
}
