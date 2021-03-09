using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HakeHR.Persistence.Data;
using HakeHR.Persistence.Infrastructure;
using HakeHR.Persistence.Infrastructure.Enums;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using Serilog;

namespace HakeHR.Persistence.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly ApplicationDbContext dbContext;
        public AttachmentRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task InserAttachmentsAsync(Attachment[] attachments, AttachmentFor attachmentFor, int ownerId)
        {
            try
            {
                var tvp = new DataTable();
                tvp.Columns.Add("Name", typeof(string));
                if (attachments != null)
                {
                    foreach (Attachment attachment in attachments)
                    {
                        tvp.Rows.Add(attachment.Filename);
                    }
                }

                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Attachment_BulkInsert]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@nameList", SqlDbType.Structured).Value = tvp;
                        command.Parameters.Add("@attachmentFor", SqlDbType.NVarChar, 50).Value = attachmentFor.ToString();
                        command.Parameters.Add("@ownerId", SqlDbType.Int).Value = ownerId;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Attachments are inserted into database, count:{attachments.Length}, for:{attachmentFor}, owner: {ownerId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"Exception occured in Attachment repository, InsertAttachemntsAsync method", attachments);
                throw;
            }
        }

        public async Task<string> RemoveAttachmentAsync(AttachmentFor attachmentFor, int ownerId, int attachmentId)
        {
            try
            {
                using(SqlConnection connection = dbContext.GetConnection())
                {
                    using(var command = new SqlCommand("[usp_HakeHr_Attachment_DeleteFromOwner]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ownerId", SqlDbType.Int).Value = ownerId;
                        command.Parameters.Add("@attachmentId", SqlDbType.Int).Value = attachmentId;
                        command.Parameters.Add("@attachmentFor", SqlDbType.NVarChar, 50).Value = attachmentFor.ToString();

                        await connection.OpenWithRetryAsync();
                        string filename = (string)await command.ExecuteScalarAsync();
                        Log.Information($"{attachmentFor} attachment is removed from database with owner id: {ownerId}");
                        return filename;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Warning(ex, $"Exception occured in Attachment repository, RemoveAttachmentAsync method", attachmentFor, ownerId);
                throw;
            }
        }
    }
}
