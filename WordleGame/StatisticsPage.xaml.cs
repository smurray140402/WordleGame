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

		int totalGames = progressList.Count;
		int completedGames = progressList.Count(game => game.Completed);
		int winPercentage = totalGames > 0 ? (int)((completedGames / (double)totalGames) * 100) : 0;

        // Get both current streak and max streak by calling the GetCurrentAndMaxStreak method
        var (currentStreak, maxStreak) = gameSaveDataViewModel.GetCurrentAndMaxStreak(userName);

        StatisticsFeedbackLabel.Text = $"Games Played: {totalGames}\nWin %: {winPercentage}%\nCurrent Streak: {currentStreak}\nMax Streak: {maxStreak}";

        /*if (progressList.Any())
		{
			// Iterates through every item in progressList and binds to StatisticsListView
			StatisticsListView.ItemsSource = progressList.Select(progressItem => new
			{
				Word = progressItem.Word,
				Attempts = progressItem.Attempts,
				Date = progressItem.Timestamp.ToString("g")
			}).ToList();
		}
		else
		{
			StatisticsFeedbackLabel.Text = "No game statistics available";
		}*/
    }
}