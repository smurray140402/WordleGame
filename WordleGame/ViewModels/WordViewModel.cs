using System;
using System.Diagnostics;
using WordleGame.Exceptions;

namespace WordleGame;

public class WordViewModel : ContentPage
{
	// Create list of strings to store words
	public List<string> WordList { get; set; }
	private readonly HttpClient httpClient;

	public WordViewModel()
	{
		httpClient = new HttpClient();
		WordList = new List<string>();
	}

    // Loads the words from the link
    // https://stackoverflow.com/questions/46624144/read-words-from-file-into-list
    public async Task LoadWords()
	{

		var response = await httpClient.GetStringAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");

        if (string.IsNullOrWhiteSpace(response))
        {
			// Custom exception
            throw new EmptyWordListException(); 
        }

        WordList = response.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

	}


	public string GetRandomWord()
	{
		if (WordList == null || !WordList.Any())
			throw new EmptyWordListException("Can't get random word as WordList is empty or not loaded correctly.");

        var random = new Random();
        return WordList[random.Next(WordList.Count)];
    }
}



