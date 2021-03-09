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
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ICollection<Team>> GetRecordsAsync(int? recordsPerPage, int pageNumber)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    ICollection<Team> teams = new List<Team>();
                    using (var command = new SqlCommand("[usp_HakeHr_Team_List]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@recordsPerPage", SqlDbType.Int).Value = recordsPerPage ?? int.MaxValue;
                        command.Parameters.Add("@pageNumber", SqlDbType.Int).Value = recordsPerPage is null || pageNumber < 1 ? 1 : pageNumber;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                teams.Add(new Team
                                {
                                    Id = (int)reader[0],
                                    TeamName = (string)reader[1],
                                    PhotoPath = reader[2] as string
                                });
                            }
                        }
                    }
                    Log.Information($"Returning teams list from database, with parameters records per page {recordsPerPage}, page number {pageNumber}");
                    return teams;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, GetRecordsAsync(records per page:{recordsPerPage}, page number:{pageNumber}) method");
                throw;
            }
        }

        public async Task<Team> GetRecordByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_GetById]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Log.Information($"Got team with id - {id} from database", id);
                                return new Team
                                {
                                    Id = (int)reader[0],
                                    TeamName = (string)reader[1],
                                    PhotoPath = reader[2] as string
                                };
                            }
                        }
                        throw new ArgumentException($"Team with id:{id} was not found", "id");
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
                Log.Error(ex, $"Exception occured in Team repository, GetRecordByIdAsync(id) method with id - {id}", id);
                throw;
            }
        }

        public async Task InsertRecordAsync(Team record)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_Insert]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@teamName", SqlDbType.NVarChar, 50).Value = record.TeamName;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Team is inserted into database");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, InsertRecordAsync(team) method", record);
                throw;
            }
        }

        public async Task BulkInsertAsync(ICollection<Team> teams)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    string tableName = "Team";
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter($"SELECT TOP 0 * FROM [{tableName}]", connection))
                    {
                        adapter.Fill(table);
                    };

                    foreach (Team team in teams)
                    {
                        DataRow row = table.NewRow();
                        row["TeamName"] = team.TeamName;
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
                Log.Error(ex, $"Exception occured in Team repository, BulkInsertAsync(teams) method", teams);
                throw;
            }
        }

        public async Task<bool> UpdateRecordAsync(Team record)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        command.Parameters.Add("@teamName", SqlDbType.NVarChar, 50).Value = record.TeamName;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Team update completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, UpdateRecordAsync(team) method", record);
                throw;
            }
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_Delete]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Team delete completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, DeleteRecordAsync(id) method", id);
                throw;
            }
        }

        public async Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> teamIds)
        {
            try
            {
                var tvp = new DataTable();
                tvp.Columns.Add("Id", typeof(int));
                if (teamIds != null)
                {
                    foreach (int id in teamIds)
                    {
                        tvp.Rows.Add(id);
                    }
                }

                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    using (var command = new SqlCommand("[usp_HakeHr_Team_BulkDelete]", connection))
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
                Log.Error(ex, $"Exception occured in Team repository," +
                    $" BulkDeleteAsync(startIndex, endIndex, teamids) method, start:{startIndex}, end: {endIndex}, ids count: {teamIds.Count}",
                    startIndex, endIndex, teamIds.Count);
                throw;
            }
        }

        public async Task AddEmployeeAsync(TeamEmployee teamEmployee)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_AddEmployee]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@teamId", SqlDbType.Int).Value = teamEmployee.TeamId;
                        command.Parameters.Add("@employeeId", SqlDbType.Int).Value = teamEmployee.EmployeeId;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Team add employee completed");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, AddEmployeeAsync(teamEmployee) method", teamEmployee);
                throw;
            }
        }

        public async Task<IList<Team>> GetOrganizationTeamsAsync(int organizationId)
        {
            try
            {
                IList<Team> teams = new List<Team>();
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_GetByOrganizationId]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@organizationId", SqlDbType.Int).Value = organizationId;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                teams.Add(new Team
                                {
                                    Id = (int)reader[0],
                                    TeamName = (string)reader[1],
                                    PhotoPath = reader[2] as string
                                });
                            }
                        }
                    }
                }
                Log.Information("Organization teams returned by organization id");
                return teams;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, GetOrganizationTeamsAsync(organizationId) method", organizationId);
                throw;
            }
        }

        public async Task<bool> AddPhotoPathAsync(int teamId, string photoPath)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Team_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = teamId;
                        command.Parameters.Add("@photoPath", SqlDbType.NVarChar, 75).Value = photoPath;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Team add photo completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Team repository, AddPhotoAsync({teamId}, {photoPath}) method", teamId, photoPath);
                throw;
            }
        }
    }
}
