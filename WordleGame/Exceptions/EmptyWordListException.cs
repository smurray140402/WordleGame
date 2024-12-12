namespace WordleGame.Exceptions;

public class EmptyWordListException : Exception
{
    // Default message
	public EmptyWordListException() : base("Word list is empty or not loaded correctly.") { }

    // Custom message
    public EmptyWordListException(string message) : base(message) { }

}