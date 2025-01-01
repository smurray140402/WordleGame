using System.Diagnostics;

namespace WordleGame;

public partial class SettingsPage : ContentPage
{
	private string currentUsername;

	// Directory where saved game files are stored
	private static string SaveDirectory = Path.Combine(FileSystem.AppDataDirectory, "UserSaveData");

	// ViewModel that manages the game save data
	private GameSaveDataViewModel gameSaveDataViewModel;

	// 
	public SettingsPage(string username, GameSaveDataViewModel viewModel)
	{
		InitializeComponent();
		currentUsername = username;
		gameSaveDataViewModel = viewModel;
	}

	// Resets game data for user
	private async void OnResetDataClicked(object sender, EventArgs e)
	{
		bool confirm = await DisplayAlert("Confirm Reset", "Are you sure you want to delete all previous save games? This action cannot be undone.", "Yes", "No");

		if (confirm)
		{
			try
			{
				// Stores the file path for the users game save data json file
				string saveFilePath = Path.Combine(SaveDirectory, $"{currentUsername}_save_data.json");

				if (File.Exists(saveFilePath))
				{
					File.Delete(saveFilePath);

					await DisplayAlert("Success", $"All game data for {currentUsername} has been successfully reset.", "Ok");

					// Clear any data stored in memory
					gameSaveDataViewModel.ClearData();

                    Debug.WriteLine("\n\nJust after display alert");

                    //https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0
                    await Shell.Current.GoToAsync("//MainPage");

					// Call StartNewGame if currently on mainpage
					if (Shell.Current.CurrentPage is MainPage mainPage)
					{
						Debug.WriteLine("\n\nMainPage found. Calling StartNewGame");

						mainPage.StartNewGame();
					}
					else
					{
						Debug.WriteLine("\n\nMainPage not found.");
					}
				}
				else
				{
					// If no data is found display alert and go back to mainpage
					await DisplayAlert("No Data Found", $"No save game data exists for {currentUsername}.", "OK");
                    await Shell.Current.GoToAsync("//MainPage");
                }
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"An error occurred while resetting data: {ex.Message}", "Ok");
			}
		}
	}
}