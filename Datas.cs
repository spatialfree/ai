using System.Text.RegularExpressions;

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
    => string.Format("{0:0.##},{1:0.##}", x, y);
}

public class Scroll {
	public Scroll() {
		name = "";
		text = "";
		color = RandomColor();

		pos = new Vec(0, 0);
		area = new Vec(0, 0);
	}
	public Scroll(Scroll scroll) {
		name = scroll.name;
		text = scroll.text;
		color = scroll.color;

		pos = scroll.pos;
		area = scroll.area;
	}
	
	public string name;
	public string taglessName => Regex.Replace(name, @"<[^>]*>", ""); 
	public string text;
	public string color;

	public Vec pos;
	public Vec area;

	// public string Full => $"{Tools.Formatted(Label,"\n")}{Tools.Formatted(Text,"\n\n")}";

	string RandomColor() {
		Random random = new Random();
		int v = 255;
		int b = random.Next(0, v - 64);
		v -= b;
		int g = random.Next(0, v);
		v -= g;
		int r = v;
		return $"rgb({r},{g},{b})";
	}
}

/*
	

*/