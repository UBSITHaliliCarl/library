using library.Models;
using Npgsql;

namespace library.Data;

// Every SQL statement about loans lives in this one class.
public class LoanRepository
{
    public const int MaxOpenLoans = 3;             // the most books one member may have out at once

    private readonly NpgsqlDataSource _dataSource;

    public LoanRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
}
