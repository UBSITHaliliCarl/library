namespace library.Models
{
    public class Note
    {
        public long NoteId { get; set; }            // note_id    BIGINT, the primary key
        public string Title { get; set; } = "";     // title      VARCHAR(100) NOT NULL
        public string? Body { get; set; }           // body       TEXT, may be NULL
        public DateOnly CreatedOn { get; set; }     // created_on DATE NOT NULL
    }
}