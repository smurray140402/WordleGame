using System.Text.Json;

namespace WordleGame;

public class GameSaveDataViewModel
{
	// Directory where game save data for each user will be saved
	private static readonly string SaveDirectory = Path.Combine(FileSystem.AppDataDirectory, "UserSaveData");

	// List to store all saved progress and initiliases it with a new empty list
	public List<GameSaveData> SavedDataList { get; set; } = [];

	public GameSaveDataViewModel()
	{
		Directory.CreateDirectory(SaveDirectory);
	}

	// Load data from JSON file
	private void LoadData(string username)
	{
		string SaveFilePath = Path.Combine(SaveDirectory, $"{username}_save_data.json");

		if (File.Exists(SaveFilePath))
		{
			var json = File.ReadAllText(SaveFilePath);

			// Deserialise the JSON data into a list of GameSaveData objects. If it fails it initiliases an empty list
			SavedDataList = JsonSerializer.Deserialize<List<GameSaveData>>(json) ?? new List<GameSaveData>();
		}

	} // load data

	public void SaveData(string username)
	{
        string SaveFilePath = Path.Combine(SaveDirectory, $"{username}_save_data.json");

        // WriteIndented formats the JSON file for better readability
        var jsonString = JsonSerializer.Serialize(SavedDataList, new JsonSerializerOptions {  WriteIndented = true });
		File.WriteAllText(SaveFilePath, jsonString);
	}

	public void AddProgress(string username, string name, string word, int attempts, string guess, bool isCompleted)
	{
		// This is the mostly the same as the GetCurrentAndMaxStreak method
		// To reduce overlapping code we could create a helper method that both access
        LoadData(username);

        // Sort data in ascending order
        var userGames = SavedDataList.Where(game => game.Finished).OrderBy(game => game.Timestamp).ToList();

        int currentStreak = 0;
        int maxStreak = 0;

		// Loop though each game in users game history 
        foreach (var game in userGames)
        {
            if (game.Completed)
            {
                currentStreak++;

				// If currentStreak is higher update maxStreak
                if (currentStreak > maxStreak)
                {
                    maxStreak = currentStreak;
                }
            }
            else
            {
				// If game was lost reset current streak
				currentStreak = 0; ;
            }
        }


        // Check if the user already has saved progress for this word
        var existingProgress = SavedDataList.FirstOrDefault(data => data.Username == username && data.Word == word);

		// If not make a new progress
		if (existingProgress == null)
		{
			var newProgress = new GameSaveData
			{
				Username = username,
				Name = name,
				Word = word,
				Attempts = attempts,
				Timestamp = DateTime.Now,
				Guesses = new List<string> { guess },
				Completed = isCompleted,
				MaxStreak = maxStreak
			};

			SavedDataList.Add(newProgress);
		}

		// If they do just update the attempts, timestamp and add each guess
		else
		{
			existingProgress.Attempts++;
			existingProgress.Timestamp = DateTime.Now;
			existingProgress.Guesses.Add(guess);
			existingProgress.Completed = isCompleted;

            if (currentStreak > existingProgress.MaxStreak)
            {
                existingProgress.MaxStreak = currentStreak;
            }
        }

		SaveData(username);
	}

	// Retrieve all progress for a specific user
	public List<GameSaveData> GetSaveDataByUser (string username)
	{
		LoadData(username);
		return SavedDataList;
	}

	// Returns a tuple for the StatisticsPage
	public (int currentStreak, int maxStreak) GetCurrentAndMaxStreak(string username)
	{
		LoadData(username);

        // Sort data in ascending order
        var userGames = SavedDataList.Where(game => game.Finished).OrderBy(game => game.Timestamp).ToList();

		int currentStreak = 0;
		int maxStreak = 0;

        // Loop though each game in users game history 
        foreach (var game in userGames)
		{
			if (game.Completed)
			{
				currentStreak++;

                // If currentStreak is higher update maxStreak
                if (currentStreak > maxStreak)
				{
					game.MaxStreak = currentStreak;
					maxStreak = currentStreak;
				}
            }
			else
			{
                // If game was lost reset current streak
                currentStreak = 0; ;
            }
		}

		SaveData(username);

		// Return a tuple with the current streak and the max streak
		return (currentStreak, maxStreak);
	}

	// Gets the distribution of guess attempts 
    public List<int> GetGuessCountsByUser(string username)
    {
        LoadData(username);

        // Initialize a list to hold counts for the number of guesses
        List<int> guessCounts = new List<int> { 0, 0, 0, 0, 0, 0 };  

        // Loop through each game
        foreach (var game in SavedDataList)
        {
			// Only takes into account completed games
            if (game.Completed)
            {
                // Check how many attempts the user made and update the count
                if (game.Attempts >= 1 && game.Attempts <= 6)
                {
                    guessCounts[game.Attempts - 1]++;
                }
            }
        }

        return guessCounts;
    }

    // Clears all 'in-memory data', ensuring that any previously cached or stored game data is deleted.
    public void ClearData()
    {
        SavedDataList.Clear();
        // If you have other data members, clear them as well
    }

}