namespace ai;

public class Pointer {
  IndexBase index;
  public Pointer(IndexBase index) {
    this.index = index;
  }

  public long id = -1;
  public Vec screen = new Vec();
  public Vec canvas { get { return (screen / index.Scale) - index.Canvas; } }

  public bool dwn = false;
  public bool dbl = false;
  DateTime lastDown = DateTime.Now;
  public void Down(double x, double y, long id) {
    screen = new Vec(x, y);
    this.id = id;

    TimeSpan time = DateTime.Now - lastDown;
    dbl = (screen - lastUp).Mag < 20 && time.TotalMilliseconds < 500;
    lastDown = DateTime.Now;
    dwn = true;
  }

  public void Move(double x, double y) {
    screen = new Vec(x, y);
  }

  public Vec lastUp = new Vec();
  public void Up() {
    lastUp = screen;

    dwn = false;
  }
}