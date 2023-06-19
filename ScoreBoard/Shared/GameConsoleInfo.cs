namespace ScoreBoard.Shared
{
    public class GameConsoleInfo
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Score { get; set; }
        public string? PlayerName { get; set; }
        public Status Status { get; set; }
        public ConsoleType Type { get; set; }
        public string? Url { get; set; }
    }
    public enum ConsoleType
    {
        New,
        Old
    }
}