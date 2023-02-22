using System.Text.RegularExpressions;

public class Vec {
	public double x { get; set; }
	public double y { get; set; }

	public Vec(double x = 0, double y = 0) {
		this.x = x;
		this.y = y;
	}

	public static Vec operator *(Vec a, double b) 
		=> new Vec(a.x * b, a.y * b);

	public static Vec operator /(Vec a, double b) 
		=> new Vec(a.x / b, a.y / b);

	public static Vec operator +(Vec a, Vec b) 
		=> new Vec(a.x + b.x, a.y + b.y);

	public static Vec operator -(Vec a, Vec b) 
		=> new Vec(a.x - b.x, a.y - b.y);


  public double Mag
		=> Math.Sqrt(x * x + y * y);

	public double Angle
		=> Math.Atan2(y, x) * 360 / Math.Tau;

	

  public override string ToString()
    => string.Format("{0:0.##},{1:0.##}", x, y);
}

public class Scroll {
	public Scroll() {
		name = "";
		text = "";

		pos = new Vec(0, 0);
		area = new Vec(0, 0);
	}
	
	// cloud
	public string name;
	public string taglessName => Regex.Replace(name, @"<[^>]*>", ""); 
	public string text;

	public bool executable {
		get {
			return text.Contains("><"); // unoptimized
		}
	}

	// local
	public bool edit = false;

	public Vec pos;
	public Vec area;

	// public string Full => $"{Tools.Formatted(Label,"\n")}{Tools.Formatted(Text,"\n\n")}";

	public bool Contains(Vec pointer) {
		Vec localPos = pointer - pos;
		bool inXMin = localPos.x >= 0;
		bool inXMax = localPos.x < area.x + 20;
		int x = (inXMin ? 0 : -1) + (inXMax ? 0 : 1);

		bool inYMin = localPos.y >= 0;
		bool inYMax = localPos.y < area.y + 50;
		int y = (inYMin ? 0 : -1) + (inYMax ? 0 : 1);
		// print 0 for inside both and - for outside min and + for outside max
		// if (scroll == TopScroll) {
		// 	string xstr = x == 0 ? "0" : x < 0 ? "-" : "+";
		// 	string ystr = y == 0 ? "0" : y < 0 ? "-" : "+";
		// 	Console.WriteLine($"{xstr}{ystr} {(int)pointers[0].canvas.x - scroll.pos.x}");
		// }
		return inXMin && inXMax && inYMin && inYMax;
	}

	public string color {
		get {
			int b = 120 + (executable ? 60 : 0);
			int g = 80  + (name != taglessName ? 80 : 0);
			int r = 100 + (text.Contains("{") && text.Contains("}") ? 100 : 0);
			return $"rgb({r},{g},{b})";
		}
	}
}

/*
	

*/