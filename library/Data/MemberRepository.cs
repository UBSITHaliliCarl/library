using Npgsql;
using library.Models;
namespace library.Data;

public class MemberRepository
{
    private readonly NpgsqlDataSource _dataSource;
    public MemberRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    private static Member ReadMember(NpgsqlDataReader reader)
    {
        return new Member
        {
            MemberId = reader.GetInt64(0),
            FullName = reader.GetString(1),
            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
            // FIX: Added null check for member_type to prevent runtime crashes
            MemberType = reader.IsDBNull(4) ? null : reader.GetString(4),
            JoinDate = reader.GetFieldValue<DateOnly>(5).ToString("yyyy-MM-dd")
        };
    }

    private static object ToDb(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? DBNull.Value : text.Trim();
    }

    private static DateOnly ToDate(string? text)
    {
        return DateOnly.TryParseExact(text, "yyyy-MM-dd", out var date)
            ? date
            : DateOnly.FromDateTime(DateTime.Today);
    }

    public async Task<List<Member>> GetAllAsync()
    {
        const string sql = "SELECT member_id, full_name, email, phone, member_type, join_date " +
                           "FROM lending.member ORDER BY full_name;";
        var members = new List<Member>();
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            members.Add(ReadMember(reader));
        }
        return members;
    }

    // CREATE: insert a new member.
    public async Task AddAsync(Member member)
    {
        // FIX: Changed 'name' to 'full_name'
        const string sql = "INSERT INTO lending.member (full_name, email, phone, member_type, join_date) " +
                           "VALUES (@full_name, @email, @phone, @member_type, @join_date);";
        await using var command = _dataSource.CreateCommand(sql);
        // FIX: Changed member.Name to member.FullName
        command.Parameters.AddWithValue("full_name", member.FullName.Trim());
        command.Parameters.AddWithValue("email", ToDb(member.Email));
        command.Parameters.AddWithValue("phone", ToDb(member.Phone));
        command.Parameters.AddWithValue("member_type", ToDb(member.MemberType));
        command.Parameters.AddWithValue("join_date", ToDate(member.JoinDate));
        await command.ExecuteNonQueryAsync();
    }

    // READ: one member by id.
    public async Task<Member?> GetByIdAsync(long id)
    {
        // FIX: Changed 'name' to 'full_name'
        const string sql = "SELECT member_id, full_name, email, phone, member_type, join_date " +
                           "FROM lending.member WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadMember(reader) : null;
    }

    // UPDATE: change one member.
    public async Task UpdateAsync(Member member)
    {
        // FIX: Changed 'name' to 'full_name'
        const string sql = "UPDATE lending.member " +
                           "SET full_name = @full_name, email = @email, phone = @phone, " +
                           "    member_type = @member_type, join_date = @join_date " +
                           "WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        // FIX: Changed member.Name to member.FullName
        command.Parameters.AddWithValue("full_name", member.FullName.Trim());
        command.Parameters.AddWithValue("email", ToDb(member.Email));
        command.Parameters.AddWithValue("phone", ToDb(member.Phone));
        command.Parameters.AddWithValue("member_type", ToDb(member.MemberType));
        command.Parameters.AddWithValue("join_date", ToDate(member.JoinDate));
        command.Parameters.AddWithValue("id", member.MemberId);
        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one member.
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.member WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }
}
