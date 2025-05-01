using appBackend.Interfaces.GlobalPostWall;

namespace appBackend.Models.PostChildrenComponents
{
    public class PollComponent : PostComponent
    {
        public Poll Poll { get; set; }

        public override string Type => "Poll";
        public PollComponent(Poll poll) { Poll = poll; }
    }
}
