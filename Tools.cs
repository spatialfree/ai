public static class Tools {
  public static string? Formatted(string? str, string? append) {
		return string.IsNullOrEmpty(str) ? "" : str + append;
	}
}