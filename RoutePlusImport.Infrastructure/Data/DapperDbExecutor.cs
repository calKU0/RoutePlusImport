using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RoutePlusImport.Infrastructure.Data
{
    public class DapperDbExecutor : IDbExecutor
    {
        private readonly string _connectionsString;

        public DapperDbExecutor(string connectionsString)
        {
            _connectionsString = connectionsString;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null)
        {
            using var connection = new SqlConnection(_connectionsString);
            await connection.OpenAsync();

            return await connection.QueryAsync<T>(sql, param, commandType: commandType);
        }

        public async Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond>(string sql, Func<TFirst, TSecond, TFirst> map, string splitOn, object? param = null, CommandType? commandType = null)
        {
            using var connection = new SqlConnection(_connectionsString);
            await connection.OpenAsync();

            var result = await connection.QueryAsync<TFirst, TSecond, TFirst>(sql, map, param, commandType: commandType, splitOn: splitOn);

            return result.FirstOrDefault();
        }

        public async Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond, TThird>(string sql, Func<TFirst, TSecond, TThird, TFirst> map, string splitOn, object? param = null, CommandType? commandType = null)
        {
            using var connection = new SqlConnection(_connectionsString);
            await connection.OpenAsync();

            var result = await connection.QueryAsync<TFirst, TSecond, TThird, TFirst>(
                sql,
                map,
                param,
                splitOn: splitOn,
                commandType: commandType
            );

            return result.FirstOrDefault();
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null)
        {
            using var connection = new SqlConnection(_connectionsString);
            await connection.OpenAsync();

            return await connection.QuerySingleOrDefaultAsync<T>(sql, param, commandType: commandType);
        }
    }
}