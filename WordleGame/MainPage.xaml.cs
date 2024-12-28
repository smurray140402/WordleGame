using System;
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

        // Variables
        private string userName;
        private string fullName;
        private string targetWord; 
        private int currentAttempt = 0;


        public MainPage()
        {
            InitializeComponent();

            wordModel = new WordViewModel();
            gameSaveDataViewModel = new GameSaveDataViewModel();

        }

        // When page appears focus on the entry box and call on PromptUsername()
        protected override async void OnAppearing()
        {
            base.OnAppearing();

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

                // Resume Progress
                if (latestProgress != null && latestProgress.Attempts < MaxAttempts && latestProgress.Guesses.Last() != latestProgress.Word)
                {
                    targetWord = latestProgress.Word;
                    currentAttempt = latestProgress.Attempts;
                    PopulateGridWithSavedGuesses(latestProgress.Guesses);
                    FeedbackLabel.Text = $"{MaxAttempts - currentAttempt} attempts left!";
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

        private void SetupGrid()
        {
            if (hasSetupGrid) return;

            // Define rows and columns
            for (int row = 0; row < MaxAttempts; row++)
            {
                WordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int col = 0; col < WordLength; col++)
            {
                WordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
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


        private void PopulateGridWithSavedGuesses(List<string> guesses)
        {
            foreach (var guess in guesses.Select((value, index) => new { value, index }))
            {
                var guessText = guess.value;
                var rowIndex = guess.index;

                // Populate each letter in the row
                for (int col = 0; col < WordLength; col++)
                {
                    var label = (Label)WordGrid.Children[rowIndex * WordLength + col];
                    label.Text = guessText[col].ToString();

                    // Determine background color for the letter
                    if (guessText[col] == targetWord[col])
                    {
                        label.BackgroundColor = Colors.Green;
                    }
                    else if (targetWord.Contains(guessText[col]))
                    {
                        label.BackgroundColor = Colors.Yellow;
                    }
                    else
                    {
                        label.BackgroundColor = Colors.Gray;
                    }
                }
            }
        }


        // Creates a styled label for a cell in the Wordle grid
        private Label CreateCellLabel()
        {
            return new Label
            {
                Text = "",
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.LightGray,
                FontSize = 20,
                WidthRequest = 40,
                HeightRequest = 40,
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
                FeedbackLabel.Text = "Please enter a valid 5 letter word.";
                return;
            }

            // Validate guessed word with valid guesses list to only let them guess if its a valid word
            if (!wordModel.ValidGuesses.Contains(guess.ToLower()))
            {
                FeedbackLabel.Text = "Your guess is not a valid word please try again.";
                return;
            }

            FeedbackLabel.Text = "";
            GuessCheck(guess);
            UserInput.Text = "";

        } // OnGuessBtnClicked



        // Check the users guess against target word and update the grid
        private void GuessCheck(string guess)
        {
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
                var label = (Label)WordGrid.Children[currentAttempt * WordLength + col];
                label.Text = guess[col].ToString();

                // Decrements the letterCount so a letter will only change to yellow if there is atleast one remaining letter after the greens are accounted for
                if (guess[col] == targetWord[col])
                {
                    letterCount[guess[col]]--;
                }
            }

            // Loop that changes the labels background colours according to the letters
            for (int col = 0; col < WordLength; col++)
            {
                var label = (Label)WordGrid.Children[currentAttempt * WordLength + col];

                // Correct position
                if (guess[col] == targetWord[col])
                {
                    label.BackgroundColor = Colors.Green;
                    continue;
                }

                // Letter in word but not that position
                if (letterCount.ContainsKey(guess[col]) && letterCount[guess[col]] > 0)
                {
                    label.BackgroundColor = Colors.Yellow;
                    letterCount[guess[col]]--;
                }

                // Letter not in word
                else
                {
                    label.BackgroundColor = Colors.Gray;
                }
            }

            gameSaveDataViewModel.AddProgress(userName, fullName, targetWord, currentAttempt + 1, guess);

            if (guess == targetWord)
            {
                FeedbackLabel.Text = "Congratulations! You guessed the word.";
                FeedbackLabel.TextColor = Colors.Green;

                // This makes sure that CheckGameOver returns true even if you guess the word in under MaxAttempts guesses
                currentAttempt = MaxAttempts;
                EndGame();
            }

            currentAttempt++;
        } // GuessCheck

        

        // If entry box text changes
        private void OnUserInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckGameOver()) return;
            FeedbackLabel.Text = "";

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
                if (i < typedText.Length)
                {
                    label.Text = typedText[i].ToString();
                }
                else
                {
                    label.Text = ""; // Clear the label if there's no new character
                }               
            }

            // Automatically checks validation of word 
            if (typedText.Length == WordLength && !wordModel.ValidGuesses.Contains(typedText.ToLower()))
            {
                FeedbackLabel.Text = "Your guess is not a valid word please try again.";
                return;
            }

            UserInput.Text = typedText;
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
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnViewStatisticsClicked(object sender, EventArgs e)
        {
            var progressList = gameSaveDataViewModel.GetSaveDataByUser(userName);

            // Add error handling here
            await Navigation.PushAsync(new StatisticsPage(userName));

        }

        private bool CheckGameOver()
        {
            if (currentAttempt == MaxAttempts + 1)
            {
                return true;
            }

            if (currentAttempt >= MaxAttempts)
            {
                FeedbackLabel.Text = $"Game over! The word was {targetWord}.";
                EndGame();
                return true;
            }
            return false;
        }

        private void EndGame()
        {
            UserInput.IsVisible = false;
            GuessBtn.IsVisible = false;

            gameSaveDataViewModel.SaveData(userName);
        }

    } // class

} // namespace
