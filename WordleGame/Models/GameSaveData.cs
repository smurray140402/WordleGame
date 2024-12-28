namespace WordleGame;

public class GameSaveData
{
    public string Username { get; set; }
    public string Name { get; set; }
    public string Word { get; set; }
    public int Attempts { get; set; }
    public DateTime Timestamp { get; set; }
    public List<string> Guesses { get; set; } = new List<string>();

}