public class Scroll {
	public string Text  { get; set; } = default!;
	// public string Full => $"{Tools.Formatted(Label,"\n")}{Tools.Formatted(Text,"\n\n")}";
	public string Color { get; set; } = default!;
	public Vec Pos { get; set; } = default!;
	public Vec Area { get; set; } = default!;
	// public bool IsDone { get; set; }
}

public class Vec {
	public double x { get; set; }
	public double y { get; set; }

	public Vec(double x, double y) {
		this.x = x;
		this.y = y;
	}

	public static Vec operator +(Vec a, Vec b) 
		=> new Vec(a.x + b.x, a.y + b.y);

	public static Vec operator -(Vec a, Vec b) 
		=> new Vec(a.x - b.x, a.y - b.y);


  public double Mag
		=> Math.Sqrt(x * x + y * y);

	


  public override string ToString()
    => string.Format("[{0:0.##}, {1:0.##}]", x, y);
}




public class Pattern {
	public Post[] posts = new Post[6]; // temp length
}

public class Post {
	public bool active = false;

	// does every post need a name
	// auto named button
	// user facing label vs packaged with the prompt for the ai
	public string name = "";
	public string prompt = "";

	// post's do not link to one another
	// rather we have threads (w/splitters and other control flow logic)
	// that exist on top

	// color?
	public string color = "";
}