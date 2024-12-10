namespace WordleGame
{
    public partial class MainPage : ContentPage
    {
        // Constants
        private const int MaxAttempts = 6;
        private const int WordLength = 5;

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

    }

}
