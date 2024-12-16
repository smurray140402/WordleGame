using System;
using System.Diagnostics;
using WordleGame.Exceptions;

namespace WordleGame;

public class WordViewModel : ContentPage
{
	// Create list of strings to store words
	public List<string> WordList { get; set; }
	private readonly HttpClient httpClient;

	// Define filename for where words will be stored in local storage
	private const string WordsFileStorageName = "wordlewords.txt";

	public WordViewModel()
	{
		httpClient = new HttpClient();
		WordList = new List<string>();
	}

    // Loads the wordle words
	// I used the website below to help me with the string splitting
    // https://stackoverflow.com/questions/46624144/read-words-from-file-into-list
    public async Task LoadWords()
	{
		// DirectorySeparatorChar is used instead of "\" as this makes it cross platform as Linux/Mac "/" is used to separate paths
		string localFilePath = FileSystem.AppDataDirectory + Path.DirectorySeparatorChar + WordsFileStorageName;

		if (File.Exists(localFilePath))
		{
			var fileContent = File.ReadAllText(localFilePath);

			if (string.IsNullOrWhiteSpace(fileContent))
			{
				throw new EmptyWordListException();
			}
			WordList = fileContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            Debug.WriteLine("\n\n\nRead words from file\n\n\n");

        }
        else
		{
			await DownloadWords(localFilePath);
		}
	}

	// Read from webpage and write to file if localFilePath is empty
	// My brother helped me with some of this
	public async Task DownloadWords(string localFilePath)
	{
		try
		{
			var webpageContent = await httpClient.GetStringAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");

			if (string.IsNullOrWhiteSpace(webpageContent))
			{
				// Custom exception
				throw new EmptyWordListException("Downloaded word list is empty or not loaded correctly.");
			}

			File.WriteAllText(localFilePath, webpageContent);

			WordList = webpageContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            Debug.WriteLine("\n\n\nDownloaded words\n\n\n");
        }
		catch (Exception ex)
		{
			Debug.WriteLine($"Error downloading word list: {ex.Message}");
			throw new EmptyWordListException("Failed to download or save word list.");
		}
	}


	public string GetRandomWord()
	{
		if (WordList == null || !WordList.Any())
			throw new EmptyWordListException("Can't get random word as WordList is empty or not loaded correctly.");

        var random = new Random();
        return WordList[random.Next(WordList.Count)];
    }
}



