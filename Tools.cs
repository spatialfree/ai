namespace ai;

public static class Tools {

	public static int RollOver(int index, int roll, int length) {
		int rolled = index + roll;
		if (rolled < 0)
			return length - 1;
		
		if (rolled > length - 1)
			return 0;

		return rolled;
	}

	public static string RandomColor() {
		Random random = new Random();
		// rgb(255,255,255)
		return $"rgb({random.Next(0, 255)},{random.Next(0, 255)},{random.Next(0, 255)})";
	}

	public static int Stepped(this double value) {
		return (int)(value / 10) * 10;
	}
	public static Vec Stepped(this Vec value) {
		return new Vec(
			value.x.Stepped(), 
			value.y.Stepped()
		);
	}

	public static string? ToStringBias(this double value) {
		if (value > 0)
			return $"+{value:0.0}";
		else if (value < 0)
			return $" {value:0.0}";
		else
			return $"~0.0";
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

	// string to vec
	public static Vec ToVec(this string str) {
		string[] split = str.Split(',');
		return new Vec(float.Parse(split[0]), float.Parse(split[1]));
	}
}