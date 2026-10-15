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
    // Turns the current row into a Member. Column order must match the SELECT lists below:
    // member_id, name, email, phone, member_type, join_date
    // NULL email or phone becomes "", and the DATE becomes text like 2026-01-08.
    private static Member ReadMember(NpgsqlDataReader reader)
    {
        return new Member
        {
            MemberId = reader.GetInt64(0),
            name = reader.GetString(1),
            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
            MemberType = reader.GetString(4),
            JoinDate = reader.GetFieldValue<DateOnly>(5).ToString("yyyy-MM-dd")
        };
    }
    // Blank text is stored as NULL. This matters for email: it is UNIQUE, and
    // two members with "" would clash, while any number of NULLs are fine.
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
    // READ: every member, sorted by name.
    public async Task<List<Member>> GetAllAsync()
    {
        const string sql = "SELECT member_id, name, email, phone, member_type, join_date " +
                           "FROM lending.member ORDER BY name;";
        var members = new List<Member>();
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            members.Add(ReadMember(reader));
        }
        return members;
    }
    // CREATE: insert a new member. The database chooses member_id.
    public async Task AddAsync(Member member)
    {
        const string sql = "INSERT INTO lending.member (name, email, phone, member_type, join_date) " +
                           "VALUES (@name, @email, @phone, @member_type, @join_date);";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("name", member.name.Trim());
        command.Parameters.AddWithValue("email", ToDb(member.Email));
        command.Parameters.AddWithValue("phone", ToDb(member.Phone));
        command.Parameters.AddWithValue("member_type", member.MemberType);
        command.Parameters.AddWithValue("join_date", ToDate(member.JoinDate));
        await command.ExecuteNonQueryAsync();
    }
    // READ: one member by id, or null if there is none.
    public async Task<Member?> GetByIdAsync(long id)
    {
        const string sql = "SELECT member_id, name, email, phone, member_type, join_date " +
                           "FROM lending.member WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadMember(reader) : null;
    }
    // UPDATE: change one member.
    public async Task UpdateAsync(Member member)
    {
        const string sql = "UPDATE lending.member " +
                           "SET name = @name, email = @email, phone = @phone, " +
                           "    member_type = @member_type, join_date = @join_date " +
                           "WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("name", member.name.Trim());
        command.Parameters.AddWithValue("email", ToDb(member.Email));
        command.Parameters.AddWithValue("phone", ToDb(member.Phone));
        command.Parameters.AddWithValue("member_type", member.MemberType);
        command.Parameters.AddWithValue("join_date", ToDate(member.JoinDate));
        command.Parameters.AddWithValue("id", member.MemberId);
        await command.ExecuteNonQueryAsync();
    }
    // DELETE: remove one member. Fails if the member still has loans (ON DELETE RESTRICT).
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.member WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }
}