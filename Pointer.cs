namespace ai;

public class Pointer {
  IndexBase index;
  public Pointer(IndexBase index) {
    this.index = index;
  }

  public long id = -1;
  public Vec screen = new Vec();
  public Vec canvas { get { return (screen / index.Scale) - index.Canvas; } }

  public bool clear = true;

  public bool dwn = false;
  public bool dbl = false;
  public Vec lastDown = new Vec();
  DateTime downTime = DateTime.Now;
  public void Down(double x, double y, long id) {
    lastDown = screen = new Vec(x, y);
    this.id = id;

    TimeSpan time = DateTime.Now - downTime;
    dbl = (screen - lastUp).Mag < 20 && time.TotalMilliseconds < 500;
    downTime = dbl ? DateTime.MinValue : DateTime.Now;
    dwn = true;

    // if clear of the scrolls then try for a long press
		clear = true;
		foreach (Scroll scroll in index.Scrolls) {
			if (scroll.Contains(index.pointers[0].canvas)) {
				clear = false;
				break;
			}
		}

		if (clear) {
      cts.Cancel();
      cts = new CancellationTokenSource();
      cts.Token.ThrowIfCancellationRequested();
      longPress = LongPress(cts.Token);
    }
  }

  CancellationTokenSource cts = new CancellationTokenSource();
  Task longPress;
  async Task LongPress(CancellationToken token) {
    await Task.Delay(500);
    if (token.IsCancellationRequested) return;
    index.PointerLong();
  }

  public void Move(double x, double y) {
    screen = new Vec(x, y);
    if (dwn && (screen - lastDown).Mag > 20) {
      cts.Cancel();
      clear = false;
    }
  }

  public Vec lastUp = new Vec();
  public void Up() {
    lastUp = screen;
    cts.Cancel();

    dwn = false;
  }
}