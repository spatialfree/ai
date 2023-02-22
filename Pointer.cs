namespace ai;

public class Pointer {
  IndexBase hack;
  public Pointer(IndexBase indexBase) {
    this.hack = indexBase;
  }

  public long id = -1;
  public Vec screen = new Vec();
  public Vec canvas { get { return (screen / hack.Scale) - hack.Canvas; } }

  public int index = -1;

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
		index = -1;
    for (int i = hack.Scrolls.Count - 1; i >= 0 ; i--) {
      Scroll scroll = hack.Scrolls[i];
			if (scroll.Contains(hack.pointers[0].canvas)) {
				index = i;
				break;
			}
		}

		if (index == -1) {
      cts.Cancel();
      cts = new CancellationTokenSource();
      cts.Token.ThrowIfCancellationRequested();
      longPress = LongPress(cts.Token);
    }
  }

  CancellationTokenSource cts = new CancellationTokenSource();
  Task longPress = Task.CompletedTask;
  async Task LongPress(CancellationToken token) {
    await Task.Delay(800);
    if (token.IsCancellationRequested) return;
    hack.PointerLong();
  }

  public void Move(double x, double y) {
    screen = new Vec(x, y);
    if (dwn && (screen - lastDown).Mag > 20) {
      cts.Cancel();
    }
  }

  public Vec lastUp = new Vec();
  public void Up() {
    lastUp = screen;
    cts.Cancel();

    dwn = false;
  }
}