using System;
using System.Diagnostics;
using WordleGame.Exceptions;

namespace WordleGame;

public class WordViewModel : ContentPage
{
	// Lists to store word list and valid guesses
    // WordList stores possible wordle answers (3103 words)
    // ValidGuesses stores possible guesses (12972 words)
	public List<string> WordList { get; set; }
	public List<string> ValidGuesses { get; set; }

	private readonly HttpClient httpClient;

	// Define filenames for where both sets of words will be stored in local storage
	private const string WordsFileStorageName = "wordlewords.txt";
    private const string ValidGuessesFileStorageName = "wordlevalidguesses.txt";

    public WordViewModel()
	{
		httpClient = new HttpClient();
		WordList = new List<string>();
		ValidGuesses = new List<string>();
	}

    // Loads the possible correct words
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

    // Loads the valid guess words
    public async Task LoadValidGuesses()
    {
        string localFilePathGuesses = FileSystem.AppDataDirectory + Path.DirectorySeparatorChar + ValidGuessesFileStorageName;

        if (File.Exists(localFilePathGuesses))
        {
            var fileContent = File.ReadAllText(localFilePathGuesses);

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new EmptyWordListException();
            }

            // Use skip to skip the first line as this is not a word
            ValidGuesses = fileContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

            Debug.WriteLine("\n\n\nRead valid guesses from file\n\n\n");

        }
        else
        {
            await DownloadValidGuesses(localFilePathGuesses);
        }
    }

    // Downloads valid guesses from official wordle bank
    public async Task DownloadValidGuesses(string localFilePathGuesses)
    {
        try
        {
            var webpageContent = await httpClient.GetStringAsync("https://raw.githubusercontent.com/Kinkelin/WordleCompetition/refs/heads/main/data/official/combined_wordlist.txt");

            if (string.IsNullOrWhiteSpace(webpageContent))
            {
                throw new EmptyWordListException("Downloaded valid guesses list is empty or not loaded correctly.");
            }

            File.WriteAllText(localFilePathGuesses, webpageContent);

            ValidGuesses = webpageContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

            Debug.WriteLine("\n\n\nDownloaded valid guesses\n\n\n");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error downloading valid guesses list: {ex.Message}");
            throw new EmptyWordListException("Failed to download or save valid guesses list.");
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



