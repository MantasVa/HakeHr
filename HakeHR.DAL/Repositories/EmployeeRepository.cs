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
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext dbContext;

        public EmployeeRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ICollection<Employee>> GetRecordsAsync(int? recordsPerPage, int pageNumber)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    ICollection<Employee> employees = new List<Employee>();
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_List]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@recordsPerPage", SqlDbType.Int).Value = recordsPerPage ?? int.MaxValue;
                        command.Parameters.Add("@pageNumber", SqlDbType.Int).Value = recordsPerPage is null || pageNumber < 1 ? 1 : pageNumber;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                employees.Add(new Employee
                                {
                                    Id = (int)reader[0],
                                    Firstname = (string)reader[1],
                                    Lastname = (string)reader[2],
                                    Email = reader[3] as string,
                                    Birthdate = reader[4] == DBNull.Value ? null : (DateTime?)(DateTime)reader[4],
                                    PhoneNumber = (string)reader[5],
                                    Address = reader[6] as string,
                                    Certifications = reader[7] as string,
                                    ManagerId = reader[8] as int?,
                                    PhotoPath = reader[9] as string
                                });
                            }
                        }
                    }
                    Log.Information($"Returning employees list from database, with parameters records per page {recordsPerPage}, page number {pageNumber}");
                    return employees;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, GetRecordsAsync(records per page:{recordsPerPage}, page number:{pageNumber}) method");
                throw;
            }

        }

        public async Task<Employee> GetRecordByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_GetById]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Log.Information($"Got employee with id - {id} from database", id);
                                return new Employee
                                {
                                    Id = (int)reader[0],
                                    Firstname = (string)reader[1],
                                    Lastname = (string)reader[2],
                                    Email = reader[3] as string,
                                    Birthdate = reader[4] == DBNull.Value ? null : (DateTime?)(DateTime)reader[4],
                                    PhoneNumber = (string)reader[5],
                                    Address = reader[6] as string,
                                    Certifications = reader[7] as string,
                                    ManagerId = reader[8] as int?,
                                    PhotoPath = reader[9] as string
                                };
                            }
                        }

                        throw new ArgumentException($"Employee with id:{id} was not found", "id");
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
                Log.Error(ex, $"Exception occured in Employee repository, GetRecordByIdAsync(id) method with id - {id}", id);
                throw;
            }

        }

        public async Task InsertRecordAsync(Employee record)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_Insert]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@firstname", SqlDbType.NVarChar, 50).Value = record.Firstname;
                        command.Parameters.Add("@lastname", SqlDbType.NVarChar, 50).Value = record.Lastname;
                        command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = record.Email;
                        command.Parameters.Add("@birthdate", SqlDbType.Date).Value = record.Birthdate;
                        command.Parameters.Add("@address", SqlDbType.NVarChar, 200).Value = record.Address;
                        command.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 15).Value = record.PhoneNumber;
                        command.Parameters.Add("@certifications", SqlDbType.NVarChar, 1000).Value = record.Certifications;
                        command.Parameters.Add("@managerId", SqlDbType.Int).Value = record.ManagerId == 0 ? null : record.ManagerId;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Employee is inserted into database");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, InsertRecordAsync(employee) method", record);
                throw;
            }

        }

        public async Task BulkInsertAsync(ICollection<Employee> employees)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    string tableName = "Employee";
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter($"SELECT TOP 0 * FROM [{tableName}]", connection))
                    {
                        adapter.Fill(table);
                    };

                    foreach (Employee employee in employees)
                    {
                        DataRow row = table.NewRow();
                        row["Firstname"] = employee.Firstname;
                        row["Lastname"] = employee.Lastname;
                        row["Email"] = employee.Email;
                        row["Birthdate"] = employee.Birthdate;
                        row["Address"] = employee.Address;
                        row["PhoneNumber"] = employee.PhoneNumber;
                        row["Certifications"] = employee.Certifications;
                        row["ManagerId"] = employee.ManagerId is null || employee.ManagerId == 0
                                ? DBNull.Value : (object)employee.ManagerId;
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
                Log.Error(ex, $"Exception occured in Employee repository, BulkInsertAsync(employees) method", employees);
                throw;
            }
        }

        public async Task<bool> UpdateRecordAsync(Employee record)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = record.Id;
                        command.Parameters.Add("@firstname", SqlDbType.NVarChar, 50).Value = record.Firstname;
                        command.Parameters.Add("@lastname", SqlDbType.NVarChar, 50).Value = record.Lastname;
                        command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = record.Email;
                        command.Parameters.Add("@birthdate", SqlDbType.Date).Value = record.Birthdate;
                        command.Parameters.Add("@address", SqlDbType.NVarChar, 200).Value = record.Address;
                        command.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 15).Value = record.PhoneNumber;
                        command.Parameters.Add("@certifications", SqlDbType.NVarChar, 1000).Value = record.Certifications;
                        command.Parameters.Add("@managerId", SqlDbType.Int).Value = record.ManagerId;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Employee update completed, Rows affected count: {rowsAffected}", rowsAffected, record.Id);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, UpdateRecordAsync(employee) method", record);
                throw;
            }
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_Delete]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Employee delete completed, Rows affected count: {rowsAffected}", rowsAffected);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, DeleteRecordAsync({id}) method", id);
                throw;
            }
        }

        public async Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> employeeIds)
        {
            try
            {
                var tvp = new DataTable();
                tvp.Columns.Add("Id", typeof(int));
                if (employeeIds != null)
                {
                    foreach (int id in employeeIds)
                    {
                        tvp.Rows.Add(id);
                    }
                }

                using (SqlConnection connection = dbContext.GetConnection())
                {
                    await connection.OpenWithRetryAsync();
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_BulkDelete]", connection))
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
                Log.Error(ex, $"Exception occured in Employee repository," +
                    $" BulkDeleteAsync(startIndex, endIndex, employeeids) method," +
                    $" start:{startIndex}, end: {endIndex}, ids count: {employeeIds.Count}",
                    startIndex, endIndex, employeeIds.Count);
                throw;
            }
        }

        public async Task AddContractAsync(EmployeeContract employeeContract)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_AddContract]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@employeeId", SqlDbType.Int).Value = employeeContract.EmployeeId;
                        command.Parameters.Add("@contractId", SqlDbType.Int).Value = employeeContract.ContractId;
                        command.Parameters.Add("@isCurrent", SqlDbType.Int).Value = employeeContract.IsCurrent;

                        await connection.OpenWithRetryAsync();
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"Employee add contract completed");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, AddContractAsync(employeeContract) method", employeeContract);
                throw;
            }
        }

        public async Task<IList<Employee>> GetTeamMembersAsync(int teamId)
        {
            try
            {
                IList<Employee> teamMembers = new List<Employee>();
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_GetByTeamId]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@teamId", SqlDbType.Int).Value = teamId;

                        await connection.OpenWithRetryAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                teamMembers.Add(new Employee
                                {
                                    Id = (int)reader[0],
                                    Firstname = (string)reader[1],
                                    Lastname = (string)reader[2],
                                    Email = reader[3] as string,
                                    Birthdate = reader[4] == DBNull.Value ? null : (DateTime?)(DateTime)reader[4],
                                    PhoneNumber = (string)reader[5],
                                    Address = reader[6] as string,
                                    Certifications = reader[7] as string,
                                    ManagerId = reader[8] as int?,
                                    PhotoPath = reader[9] as string
                                });
                            }
                        }
                    }
                }
                Log.Information("Team members returned by team id");
                return teamMembers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, GetTeamMembersAsync({teamId}) method", teamId);
                throw;
            }
        }

        public async Task<bool> AddPhotoPathAsync(int employeeId, string photoPath)
        {
            try
            {
                using (SqlConnection connection = dbContext.GetConnection())
                {
                    using (var command = new SqlCommand("[usp_HakeHr_Employee_Update]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = employeeId;
                        command.Parameters.Add("@photoPath", SqlDbType.NVarChar, 125).Value = photoPath;

                        await connection.OpenWithRetryAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Log.Information($"Employee add photo completed, Rows affected count: {rowsAffected}", rowsAffected, employeeId, photoPath);
                        return rowsAffected != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception occured in Employee repository, AddPhotoAsync({employeeId},{photoPath}) method", employeeId, photoPath);
                throw;
            }
        }
    }
}
