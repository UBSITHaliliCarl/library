namespace library.Models
{
    public class Member
    {
        public long MemberId { get; set; }          // member_id   BIGINT, the primary key
        public string FullName { get; set; } = "";  // full_name   VARCHAR NOT NULL
        public string? Email { get; set; }          // email       VARCHAR, may be NULL
        public string MemberType { get; set; } = "";// member_type VARCHAR NOT NULL
        public DateTime JoinDate { get; set; }      // join_date   TIMESTAMP, filled by database
    }
}