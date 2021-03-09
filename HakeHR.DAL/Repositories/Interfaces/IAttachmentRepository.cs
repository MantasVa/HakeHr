using System.Threading.Tasks;
using HakeHR.Persistence.Infrastructure.Enums;
using HakeHR.Persistence.Models;

namespace HakeHR.Persistence.Repositories.Interfaces
{
    public interface IAttachmentRepository
    {
        Task InserAttachmentsAsync(Attachment[] attachment, AttachmentFor attachmentFor, int ownerId);
        Task<string> RemoveAttachmentAsync(AttachmentFor attachmentFor, int ownerId, int attachmentId);
    }
}
