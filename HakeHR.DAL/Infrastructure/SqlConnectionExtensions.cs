using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Serilog;

namespace HakeHR.Persistence.Infrastructure
{

    /// <summary>
    /// Sql Connection type extension methods
    /// </summary>
    public static class SqlConnectionExtensions
    {

        /// <summary>
        /// Timespan Ienumerable, used in retry policy, within each retry value from this is used, every time increasing in time 
        /// </summary>
        private static readonly IEnumerable<TimeSpan> retryTimes = new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3)
        };

        /// Retry policy configured to retry on transient database failures
        private static readonly AsyncRetryPolicy retryPolicy = Policy
                                                   .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
                                                   .Or<TimeoutException>()
                                                   .OrInner<Win32Exception>(SqlServerTransientExceptionDetector.ShouldRetryOn)
                                                   .WaitAndRetryAsync(retryTimes,
                                                                  (exception, timeSpan, retryCount, context) =>
                                                                  {
                                                                      Log.Warning(
                                                                          exception,
                                                                          $"WARNING: Error talking to HakeHr database, will retry after {timeSpan}. Retry attempt {retryCount}",
                                                                          timeSpan,
                                                                          retryCount
                                                                      );
                                                                  });

        /// <summary>
        /// Extension method to open connection while trying to retry on failure
        /// </summary>
        /// <param name="conn">Sql connection</param>
        /// <returns></returns>
        public static async Task<SqlConnection> OpenWithRetryAsync(this SqlConnection conn)
        {
            await retryPolicy.ExecuteAsync(conn.OpenAsync);
            return conn;
        }
    }
}
