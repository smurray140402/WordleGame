﻿using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;


namespace WordleGame
{
    public partial class MainPage : ContentPage
    {
        // ViewModels
        WordViewModel wordModel;
        GameSaveDataViewModel gameSaveDataViewModel;

        // Constants
        private const int MaxAttempts = 6;
        private const int WordLength = 5;

        // Game States
        private bool hasPrompted = false;
        private bool hasSetupGame = false;
        private bool hasSetupGrid = false;
        private bool isNewGame = false;
        private bool isCompleted = false;

        // Variables
        private string userName;
        private string fullName;
        private string targetWord; 
        private int currentAttempt = 0;
        double maxHeight;

        public MainPage()
        {
            InitializeComponent();

            wordModel = new WordViewModel();
            gameSaveDataViewModel = new GameSaveDataViewModel();
            this.LayoutChanged += OnWindowChange;

            SetMode();
        }

        private void SetMode()
        {
            bool isDarkMode = Preferences.Default.Get("isDarkMode", false);

            // Set the application theme based on the saved preference
            Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        private void OnWindowChange(object sender, EventArgs e)
        {
            double maxWidth = this.Width - 10;
            maxHeight = this.Height - Icons.Height - FeedbackLabel.Height - StartGameBtn.Height - (this.Height / 5);

            // Makes sure grid is equal and max size
            if (maxWidth < maxHeight)
            {
                WordGrid.HeightRequest = maxWidth;
                WordGrid.WidthRequest = maxWidth;
            }
            else
            {
                WordGrid.HeightRequest = maxHeight;
                WordGrid.WidthRequest = maxHeight;
            }

            FeedbackLabel.FontSize = WordGrid.HeightRequest / 20;

            // Iterate through each label in the grid and adjust the font size
            foreach (var child in WordGrid.Children)
            {
                if (child is Label label)
                {
                    label.FontSize = WordGrid.HeightRequest/10;
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Prompt for username followed by grid and game setup.
            await Task.Delay(1000);
            await PromptUsername();
            SetupGrid();
            SetupGame();
           
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(1000);
                UserInput.Focus();
            });
        }

        // Prompts the user to enter their username when the app starts
        private async Task PromptUsername()
        {
            // If username has already been asked for don't ask again
            if (hasPrompted) return;

            hasPrompted = true;

            while (string.IsNullOrWhiteSpace(userName))
            {
                userName = await DisplayPromptAsync("Welcome!", "Enter your username:");

                if (string.IsNullOrWhiteSpace(userName))
                {
                    await DisplayAlert("Error", "Username cannot be empty. Please enter a valid username.", "OK");
                }
            }

            // Check if user already exists
            var userProgress = gameSaveDataViewModel.GetSaveDataByUser(userName);

            if (userProgress.Count == 0)
            {
                // New user
                fullName = await DisplayPromptAsync("New User", "Enter your full name:");
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    fullName = "Player"; // Default name if no input
                }

                await DisplayAlert("Welcome!", $"Hi {fullName}, your account has been created!", "OK");
            }
            else
            {
                // Existing user
                fullName = userProgress.First().Name;
                await DisplayAlert("Welcome Back!", $"Hi {fullName}, resuming your game!", "OK");
            }

        }

        private void SetupGrid()
        {
            if (hasSetupGrid) return;

            // Define rows and columns
            for (int row = 0; row < MaxAttempts; row++)
            {
                WordGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int col = 0; col < WordLength; col++)
            {
                WordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Add labels for each cell
            for (int row = 0; row < MaxAttempts; row++)
            {
                for (int col = 0; col < WordLength; col++)
                {
                    var cellLabel = CreateCellLabel();

                    Grid.SetRow(cellLabel, row);
                    Grid.SetColumn(cellLabel, col);
                    WordGrid.Children.Add(cellLabel);
                }
            }
            hasSetupGrid = true;
        } // SetupGrid


        // Loads word list and valid guesses list
        private async void SetupGame()
        {
            if (hasSetupGame) return;

            try
            {
                await wordModel.LoadWords();

                // Retrieve saved progress for the user
                var userProgress = gameSaveDataViewModel.GetSaveDataByUser(userName);
                var latestProgress = userProgress?.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                // Check if there is any unfinished game, and mark it as finished if necessary
                if (latestProgress != null && latestProgress.Attempts >= MaxAttempts && !latestProgress.Finished)
                {
                    latestProgress.Finished = true;
                    gameSaveDataViewModel.SaveData(userName);
                }

                // Start new game 
                if (isNewGame)
                {
                    targetWord = wordModel.GetRandomWord().ToUpper();
                    isNewGame = false;
                    Debug.WriteLine($"New target word: {targetWord}");
                }
                // Resume Progress
                else if (latestProgress != null && latestProgress.Attempts < MaxAttempts && latestProgress.Guesses.Last() != latestProgress.Word)
                {
                    targetWord = latestProgress.Word;
                    currentAttempt = latestProgress.Attempts;
                    PopulateGridWithSavedGuesses(latestProgress.Guesses);
                    FeedbackLabel.Text = $"{MaxAttempts - currentAttempt} attempts left!";
                    StartGameBtn.IsVisible = true;
                }
                // For starting a new game
                else
                {
                    targetWord = wordModel.GetRandomWord().ToUpper();
                    FeedbackLabel.Text = $"You have {MaxAttempts} attempts to guess the word!";
                    Debug.WriteLine("\n\n\n\n\n" + targetWord + "\n\n\n\n\n"); // This is just so I can check what target word is for testing
                }
            }
            catch (Exception ex)
            {
                FeedbackLabel.Text = $"Error loading word list: {ex.Message}";
                Debug.WriteLine($"Error: {ex.Message}");
            }
            try
            {
                await wordModel.LoadValidGuesses();
            }
            catch (Exception ex)
            {
                FeedbackLabel.Text = $"Error loading valid guesses list: {ex.Message}";
                Debug.WriteLine($"Error: {ex.Message}");
            }

            hasSetupGame = true;
        }


        public void StartNewGame()
        {
            // Mark the previous game as finished before starting a new one
            var userProgress = gameSaveDataViewModel.GetSaveDataByUser(userName);
            var latestProgress = userProgress?.OrderByDescending(p => p.Timestamp).FirstOrDefault();

            if (latestProgress != null && !latestProgress.Finished)
            {
                latestProgress.Finished = true;
                gameSaveDataViewModel.SaveData(userName);
            }

            ResetGameState();
            ResetGrid();
            UserInput.IsVisible = true;
            //GuessBtn.IsVisible = true;
            StartGameBtn.IsVisible = false;

            Debug.WriteLine("\n\nNew Game Started\n\n");

            FeedbackLabel.Text = $"You have {MaxAttempts} attempts to guess the word!";
            SetupGame();
            OnAppearing();
        }


        private void ResetGameState()
        {
            FeedbackLabel.TextColor = (Color)Application.Current!.Resources["Primary"];
            currentAttempt = 0;
            targetWord = string.Empty;
            hasSetupGame = false;
            isNewGame = true;
            isCompleted = false;
        }

        private void ResetGrid()
        {
            foreach (var child in WordGrid.Children)
            {
                if (child is Label label)
                {
                    label.Text = string.Empty;
                    label.BackgroundColor = (Color)Application.Current!.Resources["LabelColour"];
                    label.TextColor = (Color)Application.Current.Resources["TextSecondary"];
                }
            }
        }


        // Check the users guess against target word and update the grid
        private void GuessCheck(string guess)
        {
            StartGameBtn.IsVisible = true;
            FeedbackLabel.TextColor = (Color)Application.Current!.Resources["Primary"];

            // Check the guess against the target word and get the background colours
            var backgroundColours = CheckGuessAgainstWord(guess);

            // Loop that updates the labels in the grid with the guessed letters and background colour
            for (int col = 0; col < WordLength; col++)
            {
                var label = (Label)WordGrid.Children[currentAttempt * WordLength + col];
                label.Text = guess[col].ToString();
                label.BackgroundColor = backgroundColours[col];

                label.TextColor = (Color)Application.Current.Resources["TextPrimary"];
            }

            if (guess == targetWord)
            {
                FeedbackLabel.Text = "Congratulations! You guessed the word.";
                FeedbackLabel.TextColor = (Color)Application.Current.Resources["FeedbackLabelRight"];

                isCompleted = true;
                EndGame();
            }

            gameSaveDataViewModel.AddProgress(userName, fullName, targetWord, currentAttempt + 1, guess, isCompleted);

            currentAttempt++;
        } // GuessCheck


        private void PopulateGridWithSavedGuesses(List<string> guesses)
        {

            for (int rowIndex = 0; rowIndex < Math.Min(guesses.Count, MaxAttempts); rowIndex++)
            {
                var guessText = guesses[rowIndex];
                var backgroundColours = CheckGuessAgainstWord(guessText);

                for (int col = 0; col < WordLength; col++)
                {
                    var label = (Label)WordGrid.Children[rowIndex * WordLength + col];
                    label.Text = guessText[col].ToString();
                    label.BackgroundColor = backgroundColours[col];
                    label.TextColor = (Color)Application.Current!.Resources["TextPrimary"];
                }
            }
            StartGameBtn.IsVisible = false;
        }


        // Creates a styled label for a cell in the Wordle grid
        private Label CreateCellLabel()
        {
            return new Label
            {
                Text = "",
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = (Color)Application.Current.Resources["LabelColour"],
                FontSize = WordGrid.HeightRequest / 10,
                Margin = 2
            };
        } // CreateCellLabel


        // When guess is clicked or when enter key is entered
        private void OnGuessBtnClicked(object sender, EventArgs e)
        {
            if (CheckGameOver()) return;

            string? guess = UserInput.Text?.ToUpper();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != WordLength)
            {
                FeedbackLabel.TextColor = (Color)Application.Current!.Resources["FeedbackLabelWrong"];
                FeedbackLabel.Text = "Please enter a valid 5 letter word.";
                return;
            }

            // Validate guessed word with valid guesses list to only let them guess if its a valid word
            if (!wordModel.ValidGuesses.Contains(guess.ToLower()))
            {
                FeedbackLabel.TextColor = (Color)Application.Current!.Resources["FeedbackLabelWrong"];
                FeedbackLabel.Text = "Your guess is not a valid word please try again.";
                return;
            }

            FeedbackLabel.Text = $"{MaxAttempts - currentAttempt} attempts left!";
            GuessCheck(guess);
            UserInput.Text = "";

        } // OnGuessBtnClicked



        // If entry box text changes
        private void OnUserInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckGameOver()) return;
            FeedbackLabel.TextColor = (Color)Application.Current!.Resources["Primary"];
            FeedbackLabel.Text = $"{MaxAttempts - currentAttempt} attempts left!";

            // Gets any new text by user
            string? typedText = e.NewTextValue?.ToUpper();

            // Only allow letters
            if (!string.IsNullOrEmpty(typedText))
            {
                typedText = new string(typedText.Where(c => char.IsLetter(c)).ToArray());
            }

            // Update the cells based on the input text
            for (int i = 0; i < WordLength; i++)
            {
                var label = (Label)WordGrid.Children[currentAttempt * WordLength + i];


                // Set the label text if we have new text for that index, or clear it if we're deleting
                if (i < typedText!.Length)
                {
                    label.Text = typedText[i].ToString();
                }
                else
                {
                    label.Text = ""; // Clear the label if there's no new character
                }               
            }

            // Automatically checks validation of word 
            if (typedText!.Length == WordLength && !wordModel.ValidGuesses.Contains(typedText.ToLower()))
            {
                FeedbackLabel.TextColor = (Color)Application.Current.Resources["FeedbackLabelWrong"];
                FeedbackLabel.Text = "Your guess is not a valid word please try again.";
                return;
            }

            UserInput.Text = typedText;
        }


        private bool CheckGameOver()
        {
            if (isCompleted)
            {
                EndGame();
                return true;
            }

            if (currentAttempt >= MaxAttempts)
            {
                FeedbackLabel.TextColor = (Color)Application.Current!.Resources["FeedbackLabelWrong"];
                FeedbackLabel.Text = $"Game over! The word was {targetWord}.";
                EndGame();
                return true;
            }
            return false;
        }


        private void EndGame()
        {
            // Set the game as finished when it's ended
            var userProgress = gameSaveDataViewModel.GetSaveDataByUser(userName);
            var latestProgress = userProgress?.OrderByDescending(p => p.Timestamp).FirstOrDefault();

            if (latestProgress != null)
            {
                latestProgress.Finished = true;
                gameSaveDataViewModel.SaveData(userName);
            }

            UserInput.IsVisible = false;
            //GuessBtn.IsVisible = false;
            StartGameBtn.IsVisible = true;

            gameSaveDataViewModel.SaveData(userName);
        }



        // Refocus the Entry box if it loses focus
        private void UserInput_Unfocused(object sender, FocusEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                UserInput.Focus();
            });
        }


        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            var settingsPage = new SettingsPage(userName, gameSaveDataViewModel);
            await Navigation.PushAsync(settingsPage);
        }


        private async void OnViewStatisticsClicked(object sender, EventArgs e)
        {
            var progressList = gameSaveDataViewModel.GetSaveDataByUser(userName);

            // Add error handling here
            await Navigation.PushAsync(new StatisticsPage(userName));
        }


        private void OnNewGameClicked(object sender, EventArgs e)
        {
            StartNewGame();
        }

        private void OnHowToPlayClicked(object sender, EventArgs e)
        {
            // Show the custom alert
            CustomAlert.IsVisible = true;
        }

        private void OnCloseAlert(object sender, EventArgs e)
        {
            // Hide the custom alert
            CustomAlert.IsVisible = false;
        }


        // Function to check guess against the target word
        // Returns list of colours for the game grid and the emoji grid to use
        private List<Color> CheckGuessAgainstWord(string guess)
        {

            var backgroundColours = new List<Color>(new Color[WordLength]);

            // Dictionary to store the count of each letter in the target word
            var letterCount = new Dictionary<char, int>();

            // Counter for letters in the targetWord
            foreach (var letter in targetWord)
            {
                if (letterCount.ContainsKey(letter))
                {
                    letterCount[letter]++;
                }
                else
                {
                    letterCount[letter] = 1;
                }
            }

            // Loop that checks for letters in the correct position
            for (int col = 0; col < WordLength; col++)
            {
                // Decrements the letterCount so a letter will only change to yellow if there is atleast one remaining letter after the greens are accounted for
                if (guess[col] == targetWord[col])
                {
                    letterCount[guess[col]]--;
                    backgroundColours[col] = (Color)Application.Current!.Resources["CorrectLetter"];
                }
            }

            // Loop that changes the labels background colours according to the letters
            for (int col = 0; col < WordLength; col++)
            {

                // Letter in word but not that position
                if (guess[col] != targetWord[col] && letterCount.ContainsKey(guess[col]) && letterCount[guess[col]] > 0)
                {
                    backgroundColours[col] = (Color)Application.Current!.Resources["WrongPlaceLetter"];
                    letterCount[guess[col]]--;
                }

                // Letter not in word
                else if (backgroundColours[col] != (Color)Application.Current!.Resources["CorrectLetter"])
                {
                    backgroundColours[col] = (Color)Application.Current.Resources["WrongLetter"];
                }
            }

            return backgroundColours;

        } // CheckGuessAgainstWord

    } // class

} // namespace
