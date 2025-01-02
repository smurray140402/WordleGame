namespace WordleGame;

public class GameSaveData
{
    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Word { get; set; }
    public int Attempts { get; set; }
    public DateTime Timestamp { get; set; }
    public List<string> Guesses { get; set; } = new List<string>();
    public bool Completed { get; set; }
    public int MaxStreak { get; set; }
    public bool Finished { get; set; }

}