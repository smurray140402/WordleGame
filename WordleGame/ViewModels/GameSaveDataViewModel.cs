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

	private void SaveData(string username)
	{
        string SaveFilePath = Path.Combine(SaveDirectory, $"{username}_save_data.json");

        // WriteIndented formats the JSON file for better readability
        var jsonString = JsonSerializer.Serialize(SavedDataList, new JsonSerializerOptions {  WriteIndented = true });
		File.WriteAllText(SaveFilePath, jsonString);
	}

	public void AddProgress(string username, string name, string word, int attempts)
	{
		LoadData(username);

		var newProgress = new GameSaveData
		{
			Username = username,
			Name = name,
			Word = word,
			Attempts = attempts,
			Timestamp = DateTime.Now
		};

		SavedDataList.Add(newProgress);

		SaveData(username);
	}

	// Retrieve all progress for a specific user
	public List<GameSaveData> GetSaveDataByUser (string username)
	{
		LoadData(username);
		return SavedDataList;

	}

}