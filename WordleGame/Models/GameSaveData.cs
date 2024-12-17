namespace WordleGame;

public class GameSaveData
{
    public string Username { get; set; }

    // Have yet to add password functionality
    public string Password { get; set; }
    public string Word { get; set; }
    public int Attempts { get; set; }
    public DateTime Timestamp { get; set; }

}