using OpenAI;
using OpenAI.Completions;

namespace ai;

public class IndexBase : ComponentBase {
	[Inject] protected Mono mono { get; set; } = default!;

	protected override void OnInitialized() {
		pointers = new Pointer[] { new Pointer(this), new Pointer(this) };
	}

	OpenAIClient API = default!;
	protected bool ValidKey = false;
	string apikey = "";
	protected string Prompter = "";
	protected string ApiKey { 
		get { return apikey; }
		set { apikey = value; TryKey(); }
	}

	string lastTry = "";
	protected async Task TryKey() {
		Loading = true;
		if (ApiKey == lastTry)
			return;
		lastTry = ApiKey;

		try {
			API = new OpenAIClient(new OpenAIAuthentication(ApiKey));
			var endpoint = API.EmbeddingsEndpoint;
			await endpoint.CreateEmbeddingAsync("TryKey");
			ValidKey = true;
		}
		catch {
			API = default!;
			ValidKey = false;
		}

		Loading = false;
		StateHasChanged();
	}
	protected bool Menu  = true;


	string pattern = "";
	protected string Pattern {
		get { return pattern; }
		set { 
			pattern = value;
			Cloud = pattern == mono.Pattern;
		}
	}
	protected bool Cloud = false;

	string styleHeader = 
@".page>* {
	all: unset; 
	font-family: 'Atkinson Hyperlegible', Helvetica, sans-serif; 
}";
	protected string Style = 
@".page {
  max-width: 400px;
  margin: 0 auto;
  margin-top: 40px;
}
h1 { 
  display: block; 
  font-size: 40px;
}
p {
  display: block; 
}
input {
  display: block; 
  margin: 10px 0; padding: 5px;
  border-bottom: 1px solid black;
}
button {
  display: block;
  margin: 10px 0; padding: 10px 15px;
  border: 1px solid black;
  box-shadow: 2px 2px;
  font-weight: 700;
  letter-spacing: 1px;
}";
	protected string Styled {
		get { 
			// split into lines
			// and each line with an opening bracket {
			// gets .page added to the start of the line
			string[] lines = Style.Split('\n');
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Contains("{") && !lines[i].Contains(".page")) {
					lines[i] = ".page>" + lines[i];
				}
			}
			string str = string.Join("\n", lines);
			return styleHeader + str;
		}
	}
	protected bool Page  = false;

	ObservableCollection<Scroll> scrolls { get; set; } = new() {
		new Scroll { 
			pos = new Vec(20, 80),
			area = new Vec(80, 20),
			name = "<h1>",
			text = "Test" 
		},
		new Scroll { 
			pos = new Vec(150, 80),
			area = new Vec(80, 20),
			name = "x<input>",
			text = "mario"
		},
		new Scroll { 
			pos = new Vec(280, 80),
			area = new Vec(100, 20),
			name = "run<button>",
			text = ""
		},
		new Scroll { 
			pos = new Vec(20, 180), 
			area = new Vec(100, 40),
			name = "read<p>",
			text = "Say this is a {x}" 
		},
		new Scroll { 
			pos = new Vec(140, 180), 
			area = new Vec(210, 200), 
			name = "complete<p>", 
			text = "" 
		},
	};
	public ObservableCollection<Scroll> Scrolls {
		get { return Cloud ? mono.Scrolls : scrolls; }
	}
	public ObservableCollection<Scroll> SortedScrolls {
		// sort top to bottom and left to right using scroll.pos
		get { return new(Scrolls.OrderBy(s => s.pos.y).ThenBy(s => s.pos.x)); }
	}
	public Scroll TopScroll {
		get { return Scrolls[Scrolls.Count - 1]; }
	}
	public Scroll NextScroll {
		get { return Scrolls[Scrolls.Count - 2]; }
	}
	public Vec[] Corners(Scroll scroll) {
		int inset = 5;
		Vec[] corners = new Vec[4];
		corners[0] = scroll.pos + new Vec(inset, inset);
		corners[1] = scroll.pos + new Vec(scroll.area.x + 20 - inset, inset);
		corners[2] = scroll.pos + scroll.area + new Vec(20 - inset, 40 - inset);
		corners[3] = scroll.pos + new Vec(inset, scroll.area.y + 40 - inset);
		return corners;
	}
	// closest corner pair for Top and Next Scroll
	public Vec[] Closest {
		get {
			Vec[] closest = new Vec[2];
			Vec[] corners = Corners(TopScroll);
			Vec[] nextCorners = Corners(NextScroll);
			double minDist = double.MaxValue;
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					double dist = (corners[i] - nextCorners[j]).Mag;
					if (dist < minDist) {
						minDist = dist;
						closest[0] = corners[i];
						closest[1] = nextCorners[j];
					}
				}
			}
			return closest;
		}
	}


	protected void PointerMove(PointerEventArgs e) {
		for (int i = 0; i < pointers.Length; i++) {
			if (e.PointerId == pointers[i].id)
				pointers[i].Move(e.ClientX, e.ClientY);
		}




		if (e.PointerId != pointers[0].id) return;




		Scroll scroll = TopScroll;

		if (drag) {
			Canvas = pointers[0].screen + canvasOffset;
			Canvas.x = (int)Canvas.x;
			Canvas.y = (int)Canvas.y;

			// pinch zoom is always more complicated than it seems


		} else if (held) { 
			Vec newPos = pointers[0].canvas + offset;
			scroll.pos = newPos.Stepped();
		} else if (pull) {
			Vec newArea = (pointers[0].canvas + offset) - Corners(scroll)[0];
			newArea -= new Vec(0, 15);

			cull = newArea.x < 0 && newArea.y < 0;
			
			newArea.x = Math.Max(newArea.x.Stepped(), 60);
			newArea.y = Math.Max(newArea.y.Stepped(), 20);
			scroll.area = newArea;
		}

		StateHasChanged();
	}

	protected void PointerDown(PointerEventArgs e) {
		for (int i = 0; i < pointers.Length; i++) {
			if (!pointers[i].dwn) {
				pointers[i].Down(e.ClientX, e.ClientY, e.PointerId);
				break;
			}
		}
		if (!pointers[0].dwn) return;

		for (int i = Scrolls.Count-1; i >= 0; i--) {
			Scroll scroll = Scrolls[i];

			Vec localPos = pointers[0].canvas - scroll.pos;
			bool inXMin = localPos.x >= 0;
			bool inXMax = localPos.x < scroll.area.x + 20;
			int x = (inXMin ? 0 : -1) + (inXMax ? 0 : 1);

			bool inYMin = localPos.y >= 0;
			bool inYMax = localPos.y < scroll.area.y + 40;
			int y = (inYMin ? 0 : -1) + (inYMax ? 0 : 1);

			// print 0 for inside both and - for outside min and + for outside max
			// string xstr = x == 0 ? "0" : x < 0 ? "-" : "+";
			// string ystr = y == 0 ? "0" : y < 0 ? "-" : "+";
			// Console.WriteLine($"{xstr}{ystr} {(int)e.ClientX - scroll.pos.x}");
			if (inXMin && inXMax && inYMin && inYMax) {
				if (Loading || !edit) return;

				// Lift
				if (Scrolls.Count > 1) {
					Scrolls.RemoveAt(i);
					Scrolls.Add(scroll);
				}

				if ((pointers[0].canvas - Corners(scroll)[2]).Mag < 30.0) {
					offset = new Vec(0, 0);
					pull = true;
				} else {
					offset = scroll.pos - pointers[0].canvas;
					held = true;
				}

				StateHasChanged();
				return;
			}
		}

		if (edit && pointers[0].dbl) {
			Scroll newScroll = new Scroll();
			newScroll.pos = pointers[0].canvas.Stepped();
			newScroll.area = new Vec(60, 20);
			offset = new Vec(0, 0);
			pull = true;
			Scrolls.Add(newScroll);
		} else {
			canvasOffset = Canvas - pointers[0].screen;
			drag = true;
		}
	}

	protected void PointerUp(PointerEventArgs e) {
		for (int i = 0; i < pointers.Length; i++) {
			if (e.PointerId == pointers[i].id) {
				pointers[i].Up();
				break;
			}
		}

		if (e.PointerId != pointers[0].id) return;

		if (cull) {
			if (Scrolls.Count <= 2) {
				TopScroll.name = "";
				TopScroll.text = "";
			} else {
				Scrolls.RemoveAt(Scrolls.Count - 1);
			}
			// Console.WriteLine("CULL");
		} 

		drag = held = pull = cull = false;
		StateHasChanged();
	}

	Pointer[] pointers = new Pointer[2];
	class Pointer {
		IndexBase index;
		public Pointer(IndexBase index) {
			this.index = index;
		}

		public long id = -1;
		public Vec screen = new Vec();
		public Vec canvas { get { return screen - index.Canvas; } }

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

	Vec offset = new Vec(0, 0);

	Vec canvasOffset = new Vec(0, 0);
	protected Vec Canvas = new Vec(0, 0);

	protected bool drag = false;
	protected bool held = false;
	protected bool pull = false;
	protected bool cull = false;

	protected bool edit  = false;


	protected bool Loading = false;
	protected bool Error = false;

	protected async Task Run() {
		if (Loading) {
			// Cancel
			Loading = false;
		} else {
			Loading = true;
			edit = false;
			stream = "";
			await Read(NextScroll);
			Console.WriteLine(stream);

			if (!Loading) return;
			await Complete(stream);
			Loading = false;
		}
	}

	string stream = "";
	async Task Read(Scroll scroll) {
		bool read = false;
		string reference = "";
		string text = scroll.text;
		for (int i = 0; i < text.Length; i++) {
			if (!Loading) break;

			scroll.text = text.Insert(i, "[").Insert(i+2, "]");
			StateHasChanged();
			await Task.Delay(50);

			if (text[i] == '{') { 
				read = true; 
				continue;
			}
			if (text[i] == '}') { 
				await Read(GetScrollText(reference));
				reference = "";
				read = false; 
				continue;
			}
			if (read) { 
				reference += text[i]; 
			} else {
				stream += text[i]; 
			}
		}
		scroll.text = text;
		StateHasChanged();
	}

	Scroll GetScrollText(string name) {
		foreach (Scroll scroll in Scrolls) {
			if (scroll.taglessName.Trim().ToLower() == name.Trim().ToLower()) {
				return scroll;
			}
		}
		Console.WriteLine($"Scroll {name} not found");
		return new Scroll();
	}

	protected async Task Complete(string prompt) {
		string oldText = TopScroll.text;
		TopScroll.text = "";

		try {
			CompletionRequest request = new CompletionRequest();
			request.Model = OpenAI.Models.Model.Davinci;
			request.Prompt = prompt;
			request.MaxTokens = MaxTokens;
			request.Temperature = Temperature;
			request.PresencePenalty = Contrast;
			request.FrequencyPenalty = -Repeat;

			var endpoint = API.CompletionsEndpoint;
			int tokens = 0;
			await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
				if (!Loading) break;

				TopScroll.text += token.Completions[0].Text;
				TopScroll.text = TopScroll.text.TrimStart('\n');
				tokens++;
				StateHasChanged();
			}
			Console.WriteLine(tokens);

			// Console.WriteLine($"{DateTime.Now.ToShortTimeString()} ++ {Prompter}");
		}
		catch (Exception ex) {
			Loading = true;
			Error = true;
			StateHasChanged();
			await Task.Delay(2000);
			Error = false;

			Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -- {Prompter} : {ex.Message}");
		}

		if (TopScroll.text == "") {
			TopScroll.text = oldText;
		}
	}
	protected int   MaxTokens   = 256;
	protected float Temperature = 1.0f;
	protected float Contrast    = 0.0f;
	protected float Repeat      = 0.0f;
}

/*
	GRAVEYARD
	
	if (Outputs[OutputIndex].Length > 0) {
		if (OutputIndex < Outputs.Count - 1)
			Outputs.RemoveRange(OutputIndex + 1, Outputs.Count - (OutputIndex + 1));
		
		Outputs.Add("");
		OutputIndex++;
	}

	protected string OutputLabel = "";
	protected int OutputIndex = 0;
	protected List<string> Outputs = new() { "" };
	protected void Back() { OutputIndex--; }
	protected void Next() { OutputIndex++; }

	protected void Swap() {
		var txt = Nodes[1].text;
		Nodes[1].text = Outputs[OutputIndex];
		Outputs[OutputIndex] = txt;
	}

		<h3>Todo (@todos.Count(todo => !todo.IsDone))</h3>

	[Inject] ILocalStorageService localStorage { get; set; } = default!;
	[Inject] IJSRuntime ijsruntime { get; set; } = default!;
	private IExampleService ExampleService { get; set; } = default!;

	IEnumerable<string> Keys { get; set; } = new List<string>();

	protected override async Task OnAfterRenderAsync(bool firstRender) {
		// Keys = await localStorage.KeysAsync();
		if (firstRender) {
			
			// await LoadKey();
			// StateHasChanged();
		}

		// await SaveKey();
	}

	protected async Task LoadKey() {
		// ApiKey = await localStorage.GetItemAsync<string>("apikey");
		if (string.IsNullOrEmpty(ApiKey)) { ApiKey = ""; }
	}

	protected async Task Embed() {
		var endpoint = API.EmbeddingsEndpoint;
		// (+), 0, (-)
		EmbeddingsResponse white = await endpoint.CreateEmbeddingAsync("north");
		EmbeddingsResponse grey = await endpoint.CreateEmbeddingAsync("south");
		EmbeddingsResponse black = await endpoint.CreateEmbeddingAsync("west");
		double[] vWhite = GetVector(white);
		double[] vGrey  = GetVector(grey);
		double[] vBlack = GetVector(black);


		// double w2g = Similarity(vWhite, vGrey);
		// double g2b = Similarity(vGrey, vBlack);
		// Console.WriteLine($"{w2g} : {g2b}");

		// double[] w2g = Tools.Direction(vWhite, vGrey);
		// double[] g2b = Tools.Direction(vGrey, vBlack);
		// Console.WriteLine($"{Tools.DotProduct(w2g, g2b)}");
	}
	double[] GetVector(EmbeddingsResponse er) { return er.Data[0].Embedding.ToArray(); }

	Nodes.Count(todo => !todo.shelf) <= 2

*/
