namespace library.Models;

public class Member
    {
        public long MemberId { get; set; }          // member_id   BIGINT, the primary key
        public string FullName { get; set; } = "";   // name     VARCHAR(150) NOT NULL
        public string? Email { get; set; } = "";   // email     VARCHAR(150) NOT NULL
        public string Phone { get; set; } = "";   // phone     VARCHAR(20) NOT NULL 
        public string MemberType { get; set; } = "";   // member_type     VARCHAR(50) NOT NULL 
        public string JoinDate { get; set; } = "";   // phone     VARCHAR(20) NOT NULL 
    } 