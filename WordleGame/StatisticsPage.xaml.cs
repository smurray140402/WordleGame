namespace WordleGame;

public partial class StatisticsPage : ContentPage
{
	GameSaveDataViewModel gameSaveDataViewModel;
	string userName;

	// StatisticsPage gets passed the username so it can just display the data related to that person
	public StatisticsPage(string username)
	{
		InitializeComponent();
		userName = username;

		gameSaveDataViewModel = new GameSaveDataViewModel();

		DisplayUserStatistics();
	}

	private void DisplayUserStatistics()
	{
		var progressList = gameSaveDataViewModel.GetSaveDataByUser(userName);
		var filteredProgressList = progressList.Where(progressItem => progressItem.Finished).ToList();

		int totalGames = progressList.Count;
		int completedGames = progressList.Count(game => game.Completed);
		int winPercentage = totalGames > 0 ? (int)((completedGames / (double)totalGames) * 100) : 0;

        // Get both current streak and max streak by calling the GetCurrentAndMaxStreak method
        var (currentStreak, maxStreak) = gameSaveDataViewModel.GetCurrentAndMaxStreak(userName);

        // Get the guess count distribution (1 to 6 guesses)
        var guessCounts = gameSaveDataViewModel.GetGuessCountsByUser(userName);

		// Update labels
		GamesPlayedLabel.Text = totalGames.ToString();
        WinPercentageLabel.Text = $"{winPercentage}%";
        CurrentStreakLabel.Text = currentStreak.ToString();
        MaxStreakLabel.Text = maxStreak.ToString();

        // Method that displays the guess distribution visually
        DisplayGuessDistribution();

        // Update Completed Games ListView
        if (filteredProgressList.Any())
        {
            CompletedGamesListView.ItemsSource = filteredProgressList.Select(progressItem => new
            {
                Word = progressItem.Word,
                Attempts = progressItem.Attempts,
                Timestamp = progressItem.Timestamp.ToString("g"),
                BackgroundColor = progressItem.Completed ? (Color)Application.Current!.Resources["GameCardComplete"] : (Color)Application.Current!.Resources["GameCardIncomplete"]
            }).ToList();
        }
        else
        {
            CompletedGamesListView.ItemsSource = null;
        }
    }

    // Create a visual representation of the guess count using progress bars
    private void DisplayGuessDistribution()
    {
        // Get the guess count distribution (1 to 6 guesses)
        var guessCounts = gameSaveDataViewModel.GetGuessCountsByUser(userName);

        // Clear any existing children
        GuessDistributionStack.Children.Clear();

        int maxCount = guessCounts.Max();

        for (int i = 0; i < guessCounts.Count; i++)
        {
            var count = guessCounts[i];
            double progress = maxCount > 0 ? (double)count / maxCount : 0;

            // Create a horizontal stack for the label and progress bar
            var horizontalStack = new HorizontalStackLayout
            {
                Spacing = 10,
                VerticalOptions = LayoutOptions.Center
            };

            // Label for the guess number
            horizontalStack.Children.Add(new Label
            {
                Text = $"{i + 1}",
                TextColor = (Color)Application.Current!.Resources["Primary"],
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center
            });

            // Progress bar for the count
            horizontalStack.Children.Add(new ProgressBar
            {
                Progress = progress,
                HeightRequest = 20,
                WidthRequest = 200,
                VerticalOptions = LayoutOptions.Center
            });

            // Count label
            horizontalStack.Children.Add(new Label
            {
                Text = $"{count}",
                TextColor = (Color)Application.Current!.Resources["Primary"],
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center
            });

            // Add the horizontal stack to the main stack (GuessDistributionStack)
            GuessDistributionStack.Children.Add(horizontalStack);
        }
    }
}