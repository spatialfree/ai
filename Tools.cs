public static class Tools {

	public static int RollOver(int index, int roll, int length) {
		int rolled = index + roll;
		if (rolled < 0)
			return length - 1;
		
		if (rolled > length - 1)
			return 0;

		return rolled;
	}

  public static string? Formatted(string? str, string? append) {
		return string.IsNullOrEmpty(str) ? "" : str + append;
	}

	public static double Similarity(double[] vectorA, double[] vectorB) {
		if (vectorA.Length != vectorB.Length)
			throw new Exception("Vector dimensions do not match");

		double dotProduct = 0.0;
		double magnitudeA = 0.0;
		double magnitudeB = 0.0;

		for (int i = 0; i < vectorA.Length; i++) {
			dotProduct += vectorA[i] * vectorB[i];
			magnitudeA += Math.Pow(vectorA[i], 2);
			magnitudeB += Math.Pow(vectorB[i], 2);
		}

		return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
	}
}