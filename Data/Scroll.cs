public class Scroll {
	public string? Label { get; set; }
	public string? Text  { get; set; }
	public string? Full => $"{Tools.Formatted(Label,"\n")}{Tools.Formatted(Text,"\n\n")}";
	public string? Color { get; set; }
	// public bool IsDone { get; set; }
}