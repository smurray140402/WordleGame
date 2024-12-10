namespace WordleGame
{
    public partial class MainPage : ContentPage
    {
        // Constants
        private const int MaxAttempts = 6;
        private const int WordLength = 5;

        // Variables
        private string targetWord = "STEAK"; 
        private int currentAttempt = 0;


        public MainPage()
        {
            InitializeComponent();
            SetupGrid();
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
        } // setup grid

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
        }

        private void OnGuessBtnClicked(object sender, EventArgs e)
        {
            string guess = UserInput.Text?.ToUpper();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != WordLength)
            {
                FeedbackLabel.Text = "Please enter a valid 5 letter word.";
                return;
            }

            if (currentAttempt >= MaxAttempts)
            {
                FeedbackLabel.Text = $"Game over! The word was {targetWord}.";
                EndGame();
                return;
            }

            GuessCheck(guess);

            UserInput.Text = "";
        } // OnSubmitClicked


        private void GuessCheck(string guess)
        {
            // Check the users guess against target word and update the grid
            // TODO: Need to fix yellow appearing if letter is already accounted for
            // IDEA: Counter for how many times each letter appears?
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

        private void EndGame()
        {
            UserInput.IsVisible = false;
            GuessBtn.IsVisible = false;
        }

    }

}
