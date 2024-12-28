using System.Text.Json;

namespace WordleGame;

public class GameSaveDataViewModel
{
	// Where the game data will be saved. 
	// TODO: Maybe make a save data file for each username rather than one big one
	// Also might change it all to SQLite instead of using JSON
	private static readonly string SaveFilePath = Path.Combine(FileSystem.AppDataDirectory, "gamesavedata.json");

	// List to store all saved progress and initiliases it with a new empty list
	public List<GameSaveData> SavedDataList { get; set; } = [];

	public GameSaveDataViewModel()
	{
		LoadData();
	}

	// Load data from JSON file
	private void LoadData()
	{
		if (File.Exists(SaveFilePath))
		{
			var json = File.ReadAllText(SaveFilePath);

			// Deserialise the JSON data into a list of GameSaveData objects. If it fails it initiliases an empty list
			SavedDataList = JsonSerializer.Deserialize<List<GameSaveData>>(json) ?? new List<GameSaveData>();
		}
	} // load data

	private void SaveData()
	{
		// WriteIndented formats the JSON file for better readability
		var jsonString = JsonSerializer.Serialize(SavedDataList, new JsonSerializerOptions {  WriteIndented = true });
		File.WriteAllText(SaveFilePath, jsonString);
	}

	public void AddProgress(string username, string name, string word, int attempts)
	{
		var newProgress = new GameSaveData
		{
			Username = username,
			Name = name,
			Word = word,
			Attempts = attempts,
			Timestamp = DateTime.Now
		};

		SavedDataList.Add(newProgress);

		SaveData();
	}

	// Retrieve all progress for a specific user
	public List<GameSaveData> GetSaveDataByUser (string username)
	{
		// Uses Where() to only return the entries where the username matches 
		// Returns the filtered list as a new list
		return SavedDataList.Where(progressItem => progressItem.Username == username).ToList();
	}

}