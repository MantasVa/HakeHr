using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HakeHR.Persistence.Data;
using HakeHR.Persistence.Infrastructure;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using Serilog;

namespace HakeHR.Persistence.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrganizationRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<ICollection<Organization>> GetRecordsAsync(int? recordsPerPage, int pageNumber)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    ICollection<Organization> organizations = new List<Organization>();
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_List]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@recordsPerPage", SqlDbType.Int).Value = recordsPerPage ?? int.MaxValue;
                        command.Parameters.Add("@pageNumber", SqlDbType.Int).Value = recordsPerPage is null || pageNumber < 1 ? 1 : pageNumber;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                organizations.Add(new Organization
                                {
                                    Id = (int)reader[0],
                                    OrganizationName = (string)reader[1],
                                    PhotoPath = reader[2] as string
                                });
                            }
                        }
                    }
                    Log.Information($"Returning organizations list from database, with parameters records per page {recordsPerPage}, page number {pageNumber}");
                    return organizations;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, GetRecordsAsync(records per page:{recordsPerPage}, page number:{pageNumber}) method");
                throw;
            }
        }
        public async Task<Organization> GetRecordByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_GetById]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Log.Information($"Got organization with id - {id} from database", id);
                                return new Organization
                                {
                                    Id = (int)reader[0],
                                    OrganizationName = (string)reader[1],
                                    PhotoPath = reader[2] as string
                                };
                            }
                        }

                        throw new ArgumentException($"Organization with id:{id} was not found", "id");
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Log.Information(ex.Message, id);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, GetRecordByIdAsync(id) method with id - {id}", id);
                throw;
            }
        }
        public async Task InsertRecordAsync(Organization record)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_Insert]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@organizationName", SqlDbType.NVarChar, 50).Value = record.OrganizationName;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Organization is inserted into database");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, InsertRecordAsync(organization) method", record);
                throw;
            }
        }

        public async Task BulkInsertAsync(ICollection<Organization> organizations)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    string tableName = "Organization";
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter($"SELECT TOP 0 * FROM [{tableName}]", connection))
                    {
                        adapter.Fill(table);
                    };

                    foreach (Organization organization in organizations)
                    {
                        DataRow row = table.NewRow();
                        row["OrganizationName"] = organization.OrganizationName;
                        row["CreatedAt"] = DateTime.Now;
                        table.Rows.Add(row);
                    }

                    using (var bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = tableName;
                        await bulk.WriteToServerAsync(table);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, BulkInsertAsync(organizations) method", organizations);
                throw;
            }
        }

        public async Task<bool> UpdateRecordAsync(Organization record)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        command.Parameters.Add("@organizationName", SqlDbType.NVarChar, 50).Value = record.OrganizationName;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Organization update completed, Rows affected count: {rowsAffected}", rowsAffected, record.Id);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, UpdateRecordAsync(organization) method", record);
                throw;
            }
        }
        public async Task<bool> DeleteRecordAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_Delete]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Organization delete completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, DeleteRecordAsync({id}) method", id);
                throw;
            }
        }

        public async Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> organizationIds)
        {
            try
            {
                var tvp = new DataTable();
                tvp.Columns.Add("Id", typeof(int));
                if (organizationIds != null)
                {
                    foreach (int id in organizationIds)
                    {
                        tvp.Rows.Add(id);
                    }
                }

                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_BulkDelete]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startIndex", SqlDbType.Int).Value = startIndex;
                        command.Parameters.Add("@endIndex", SqlDbType.Int).Value = endIndex;
                        command.Parameters.Add("@Ids", SqlDbType.Structured).Value = tvp;

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository," +
                    $" BulkDeleteAsync(startIndex, endIndex, organizationids) method," +
                    $" start:{startIndex}, end: {endIndex}, ids count: {organizationIds.Count}",
                    organizationIds.Count);
                throw;
            }
        }

        public async Task AddTeamAsync(TeamOrganization teamOrganization)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_AddTeam]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@organizationId", SqlDbType.Int).Value = teamOrganization.OrganizationId;
                        command.Parameters.Add("@teamId", SqlDbType.Int).Value = teamOrganization.TeamId;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Organization add team completed");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, AddTeamAsync(teamOrganization) method", teamOrganization);
                throw;
            }
        }

        public async Task<bool> AddPhotoPathAsync(int organizationId, string photoPath)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Organization_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = organizationId;
                        command.Parameters.Add("@photoPath", SqlDbType.NVarChar, 75).Value = photoPath;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Organization add photo completed, Rows affected count: {rowsAffected}", rowsAffected, organizationId, photoPath);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Organization repository, AddPhotoAsync({organizationId}, {photoPath}) method", organizationId, photoPath);
                throw;
            }
        }
    }
}
