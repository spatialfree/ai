public static class Tools {
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

	public static double[] Direction(double[] toVector, double[] fromVector) {
		if (fromVector.Length != toVector.Length)
			throw new Exception("Vector dimensions do not match");

		double[] directionVector = new double[fromVector.Length];
		double magnitude = 0.0;
		for (int i = 0; i < fromVector.Length; i++) {
			directionVector[i] = toVector[i] - fromVector[i];
			magnitude += Math.Pow(directionVector[i], 2);
		}
		magnitude = Math.Sqrt(magnitude);

		for (int i = 0; i < directionVector.Length; i++) {
			directionVector[i] = directionVector[i] / magnitude;
		}
		return directionVector;
	}

	public static double DotProduct(double[] vectorA, double[] vectorB) {
		// if (vectorA.Length != vectorB.Length)
		// 	throw new Exception("Vector dimensions do not match");

		// double dotProduct = 0.0;
		// double magnitudeA = 0.0;
		// double magnitudeB = 0.0;

		// for (int i = 0; i < vectorA.Length; i++) {
		// 	dotProduct += vectorA[i] * vectorB[i];
		// 	magnitudeA += Math.Pow(vectorA[i], 2);
		// 	magnitudeB += Math.Pow(vectorB[i], 2);
		// }

		// return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
		if (vectorA.Length != vectorB.Length)
			throw new Exception("Vector dimensions do not match");

		double dotProduct = 0.0;
		for (int i = 0; i < vectorA.Length; i++) {
			dotProduct += vectorA[i] * vectorB[i];
		}

		return dotProduct;
	}
}