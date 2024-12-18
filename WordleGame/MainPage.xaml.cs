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

        // Hardcoded for now. Will add login/signup functionality later
        private string userName = "jeff";

        // Variables
        private string targetWord; 
        private int currentAttempt = 0;


        public MainPage()
        {
            InitializeComponent();

            wordModel = new WordViewModel();
            gameSaveDataViewModel = new GameSaveDataViewModel();

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

            FeedbackLabel.Text = "";
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


                // Save Progress
                // TODO: At the minute it only saves progress for successful tries. Need to add it for unsuccessful tries/
                //       Also need to implement a way that if I leave halfway through a go and come back it is saved to be resumed.

                gameSaveDataViewModel.AddProgress(userName, targetWord, currentAttempt + 1);

                // This makes sure that CheckGameOver returns true even if you guess the word in under MaxAttempts guesses
                currentAttempt = 6;
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
            string typedText = e.NewTextValue?.ToUpper();

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

            UserInput.Text = typedText;
        }

        // When page appears Focus on the entry box
        protected override void OnAppearing()
        {
            base.OnAppearing();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(1000);
                UserInput.Focus();
            });
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
