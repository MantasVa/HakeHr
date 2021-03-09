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
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ContractRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ICollection<Contract>> GetRecordsAsync(int? recordsPerPage, int pageNumber)
        {

            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    var contracts = new List<Contract>();
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_List]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@recordsPerPage", SqlDbType.Int).Value = recordsPerPage ?? int.MaxValue;
                        command.Parameters.Add("@pageNumber", SqlDbType.Int).Value = recordsPerPage is null || pageNumber < 1 ? 1 : pageNumber;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                contracts.Add(new Contract
                                {
                                    Id = (int)reader[0],
                                    Salary = (decimal)reader[1],
                                    StartDate = (DateTime)reader[2],
                                    ExpireDate = reader[3] == DBNull.Value ? null : (DateTime?)(DateTime)reader[3],
                                    StatusId = reader[4] as int?
                                });
                            }
                        }
                    }
                    Log.Information($"Returning contracts list from database, with parameters records per page {recordsPerPage}, page number {pageNumber}");
                    return contracts;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Contract repository, GetRecordsAsync(records per page:{recordsPerPage}, page number:{pageNumber}) method");
                throw;
            }


        }

        public async Task<Contract> GetRecordByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_GetById]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                Log.Information($"Got contract with id - {id} from database", id);
                                return new Contract
                                {
                                    Id = (int)reader[0],
                                    Salary = (decimal)reader[1],
                                    StartDate = (DateTime)reader[2],
                                    ExpireDate = reader[3] == DBNull.Value ? null : (DateTime?)(DateTime)reader[3],
                                    StatusId = reader[4] as int?
                                };
                            }
                        }

                        throw new ArgumentException($"Contract with id:{id} was not found", "id");
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
                Log.Error(ex, $"Exception occured in Contract repository, GetRecordByIdAsync(id) method with id - {id}", id);
                throw;
            }

        }

        public async Task InsertRecordAsync(Contract contract)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_Insert]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Salary", SqlDbType.Decimal).Value = contract.Salary;
                        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = contract.StartDate;
                        command.Parameters.Add("@ExpireDate", SqlDbType.Date).Value = contract.ExpireDate;
                        command.Parameters.Add("@StatusId", SqlDbType.Int).Value = contract.StatusId;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Contract is inserted into database");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Contract repository, InsertRecordAsync(contract) method", contract);
                throw;
            }
        }

        public async Task BulkInsertAsync(ICollection<Contract> contracts)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    string tableName = "Contract";
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter($"SELECT TOP 0 * FROM [{tableName}]", connection))
                    {
                        adapter.Fill(table);
                    };

                    foreach (Contract contract in contracts)
                    {
                        DataRow row = table.NewRow();
                        row["Salary"] = contract.Salary;
                        row["StartDate"] = contract.StartDate;
                        row["ExpireDate"] = contract.ExpireDate;
                        row["StatusId"] = contract.StatusId;
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
                Log.Error(ex, $"Exception occured in Contract repository, BulkInsertAsync(contracts) method", contracts);
                throw;
            }
        }

        public async Task<bool> UpdateRecordAsync(Contract contract)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = contract.Id;
                        command.Parameters.Add("@Salary", SqlDbType.Decimal).Value = contract.Salary;
                        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = contract.StartDate;
                        command.Parameters.Add("@ExpireDate", SqlDbType.Date).Value = contract.ExpireDate;
                        command.Parameters.Add("@StatusId", SqlDbType.Int).Value = contract.StatusId;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Contract update completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Contract repository, UpdateRecordAsync(contract) method", contract);
                throw;
            }
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_Delete]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Contract delete completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Contract repository, DeleteRecordAsync(id) method", id);
                throw;
            }
        }

        public async Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> contractIds)
        {
            try
            {
                var tvp = new DataTable();
                tvp.Columns.Add("Id", typeof(int));
                if (contractIds != null)
                {
                    foreach (int id in contractIds)
                    {
                        tvp.Rows.Add(id);
                    }
                }

                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_BulkDelete]", connection))
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
                Log.Error(ex, $"Exception occured in Contract repository," +
                    $" BulkDeleteAsync(startIndex, endIndex, contractids) method," +
                    $" start:{startIndex}, end: {endIndex}, ids count: {contractIds.Count}",
                    startIndex, endIndex, contractIds.Count);
                throw;
            }
        }

        public async Task<IList<Contract>> GetEmployeeContractsAsync(int employeeId)
        {
            try
            {
                IList<Contract> contracts = new List<Contract>();
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Contract_GetByEmployeeId]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@employeeId", SqlDbType.Int).Value = employeeId;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                contracts.Add(new Contract
                                {
                                    Id = (int)reader[0],
                                    Salary = (decimal)reader[1],
                                    StartDate = (DateTime)reader[2],
                                    ExpireDate = reader[3] == DBNull.Value ? null : (DateTime?)(DateTime)reader[3],
                                    StatusId = (int)reader[4]
                                });
                            }
                        }
                    }
                }
                Log.Information("Employee contracts returned by employee id");
                return contracts;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Contract repository, GetEmployeeContractsAsync(employeeId) method", employeeId);
                throw;
            }
        }
    }
}
