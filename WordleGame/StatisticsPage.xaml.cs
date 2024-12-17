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

		if (progressList.Any())
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
		}
	}
}