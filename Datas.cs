namespace ai;

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

	public static bool operator ==(Vec a, Vec b)
		=> a.x == b.x && a.y == b.y;

	public static bool operator !=(Vec a, Vec b)
		=> a.x != b.x || a.y != b.y;

	public override bool Equals(object? obj)
		=> obj is Vec vec && x == vec.x && y == vec.y;

	public override int GetHashCode()
		=> HashCode.Combine(x, y);
	

  public override string ToString()
    => string.Format("{0:0.##},{1:0.##}", x, y);
}

/*
	

*/