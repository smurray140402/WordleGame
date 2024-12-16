using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace WordleGame
{
    public partial class MainPage : ContentPage
    {
        // ViewModel
        WordViewModel wordModel;

        // Constants
        private const int MaxAttempts = 6;
        private const int WordLength = 5;

        // Variables
        private string targetWord; 
        private int currentAttempt = 0;


        public MainPage()
        {
            InitializeComponent();
            wordModel = new WordViewModel();

            SetupGame();
            SetupGrid();
        }

        // Gets random word from WordViewModel
        private async void SetupGame()
        {
            try
            {
                await wordModel.LoadWords();
                targetWord = wordModel.GetRandomWord().ToUpper();
                Debug.WriteLine("\n\n\n\n\n" + targetWord + "\n\n\n\n\n"); // This is just so I can check what target word is for testing
            }
            catch (Exception ex)
            {
                FeedbackLabel.Text = $"Error loading word list: {ex.Message}";
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void SetupGrid()
        {
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
        } // SetupGrid

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

            string guess = UserInput.Text?.ToUpper();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != WordLength)
            {
                FeedbackLabel.Text = "Please enter a valid 5 letter word.";
                return;
            }

            GuessCheck(guess);
            UserInput.Text = "";

        } // OnGuessBtnClicked

        private void GuessCheck(string guess)
        {
            // Check the users guess against target word and update the grid
            // TODO: Need to fix yellow appearing if letter is already accounted for
            // IDEA: Counter for how many times each letter appears?
            // IDEA: Other possible soltion could be instead of the else if checking if the full
            // word contains the letter just check if the remaining letters (not green) contain it.
            for (int col = 0; col < WordLength; col++)
            {
                var label = (Label)WordGrid.Children[currentAttempt * WordLength + col];
                label.Text = guess[col].ToString();

                if (guess[col] == targetWord[col])
                {
                    label.BackgroundColor = Colors.Green;
                }
                else if (targetWord.Contains(guess[col]))
                {
                    label.BackgroundColor = Colors.Yellow;
                }
                else
                {
                    label.BackgroundColor = Colors.Gray;
                }
            } // for col

            if (guess == targetWord)
            {
                FeedbackLabel.Text = "Congratulations! You guessed the word.";
                FeedbackLabel.TextColor = Colors.Green;
                EndGame();
                return;
            }

            currentAttempt++;
        } // GuessCheck

        // If entry box text changes
        private void OnUserInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckGameOver()) return;

            // Gets any new text by user
            // TODO: Register deleted characters and update UI
            string typedText = e.NewTextValue?.ToUpper();

            // Limit the length to 5
            if (typedText?.Length > WordLength)
            {
                typedText = typedText.Substring(0, WordLength);
            }

            // Update the cells based on the input text
            for (int i = 0; i < typedText.Length; i++)
            {
                var label = (Label)WordGrid.Children[currentAttempt * WordLength + i];
                label.Text = typedText[i].ToString();
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private bool CheckGameOver()
        {
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
        }

    } // class

} // namespace
