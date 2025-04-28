namespace appBackend.Dtos.GlobalPostWall
{
    public class PollDto
    {
    }

    public class CreatePollDTO
    {
        public Guid PostId { get; set; } = Guid.NewGuid();
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public bool IsGlobal { get; set; }
        public bool AllowMultipleSelections { get; set; }
    }
}
