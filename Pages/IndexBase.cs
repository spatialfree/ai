using OpenAI;
using OpenAI.Completions;

namespace ai;

public class IndexBase : ComponentBase {
	[Inject] protected Mono mono { get; set; } = default!;
	protected bool Compass = false;

	[Parameter] public string Url { get; set; } = "";

	protected override void OnInitialized() {
		pointers = new Pointer[] { new Pointer(this), new Pointer(this) };
	}

	OpenAIClient API = default!;
	protected bool ValidKey = false;
	string apikey = "";
	protected string Prompter = "";
	protected string ApiKey { 
		get { return apikey; }
		set { apikey = value; }
	}

	string lastTry = "";
	protected bool TryingKey = false;
	protected async Task TryKey() {
		TryingKey = true;
		if (ApiKey == lastTry)
			return;
		lastTry = ApiKey;

		try {
			API = new OpenAIClient(new OpenAIAuthentication(ApiKey));
			var endpoint = API.EmbeddingsEndpoint;
			await endpoint.CreateEmbeddingAsync("TryKey");
			ValidKey = true;
		}
		catch(Exception ex) {
			API = default!;
			ValidKey = false;
			Console.WriteLine($"{Prompter} apikey ex: {ex.Message}");
		}

		TryingKey = false;
		StateHasChanged();
	}
	protected bool Menu  = true;


	string pattern = "";
	protected string Pattern {
		get { return pattern; }
		set { 
			pattern = value;
			Cloud = pattern.Trim() == mono.Pattern || Url == mono.Pattern;
		}
	}
	protected bool Cloud = false;

	bool publicPage = false;
	protected bool Public {
		get { 
			if (Url == mono.Pattern) {
				if (!publicPage) {
					// Temp Key
					ApiKey = "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";
					TryKey();
				}
				Cloud = publicPage = true;
			} else {
				publicPage = false;
			}
			return publicPage;
		}
	}

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
	width: -webkit-fill-available;
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
			name = "<h1>",
			text = "Test",
			pos = new Vec(10, 10),
			area = new Vec(90, 20),
		},
		new Scroll { 
			name = "read<p>",
			text = "Say this is a {x}",
			pos = new Vec(10, 90), 
			area = new Vec(220, 20),
		},
		new Scroll { 
			name = "x<input>",
			text = "mario!",
			pos = new Vec(10, 170),
			area = new Vec(90, 20),
		},
		new Scroll { 
			name = "say<button>",
			text = "read><complete",
			pos = new Vec(130, 170),
			area = new Vec(100, 20),
		},
		new Scroll { 
			name = "complete<p>", 
			text = "",
			pos = new Vec(10, 250), 
			area = new Vec(220, 200), 
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
		corners[2] = scroll.pos + scroll.area + new Vec(20 - inset, 50 - inset);
		corners[3] = scroll.pos + new Vec(inset, scroll.area.y + 50 - inset);
		return corners;
	}
	// bounds of all scrolls 0,0 is top left, so the bottom right is the bounds
	public Vec Bounds {
		get {
			Vec bounds = new Vec(0, 0);
			foreach (Scroll scroll in Scrolls) {
				Vec bottomRight = scroll.pos + scroll.area + new Vec(20, 50);
				if (bottomRight.x > bounds.x)
					bounds.x = bottomRight.x;
				if (bottomRight.y > bounds.y)
					bounds.y = bottomRight.y;
			}
			return bounds + new Vec(10, 10); // padding
		}
	}


	protected void Rotate() {
		// go fullscreen and toggle between portrait and landscape

	}

	protected void Wheel(WheelEventArgs e) {
		// if not over scroll, zoom
		bool clear = true;
		foreach (Scroll scroll in Scrolls) {
			if (scroll.Contains(pointers[0].canvas)) {
				clear = false;
				break;
			}
		}

		if (clear)
			Zoom(pointers[0].screen, Scale - (e.DeltaY / 1000));
	}

	void Zoom(Vec center, double newScale) {
		canvasOffset = Canvas - (center / Scale);

		Scale = newScale;
		Scale = Math.Clamp(Scale, 0.1, 1);

		Canvas = (center / Scale) + canvasOffset;
		// snap to 10px grid if zoomed out
		if (Scale == 0.1) {
			Canvas.x = (int)(Canvas.x / 10) * 10;
			Canvas.y = (int)(Canvas.y / 10) * 10;
		} else {
			Canvas.x = (int)Canvas.x;
			Canvas.y = (int)Canvas.y;
		}
		StateHasChanged();
	}

	protected void PointerMove(PointerEventArgs e) {
		for (int i = 0; i < pointers.Length; i++) {
			if (e.PointerId == pointers[i].id) {
				pointers[i].Move(e.ClientX, e.ClientY);
				break;
			}
		}

		if (drag) {
			Canvas = (pointers[0].screen / Scale) + canvasOffset;
			// Canvas.x = (int)Canvas.x;
			// Canvas.y = (int)Canvas.y;
		}

		// pinch zoom
		if (pointers[0].dwn && pointers[1].dwn) {
			Vec newDelta = pointers[0].screen - pointers[1].screen;
			double newScale = oldScale * (newDelta.Mag / pointerDelta.Mag);
			Zoom(
				pointers[0].screen,
				newScale
			);
		}

		if (e.PointerId != pointers[0].id) return;

		Scroll scroll = TopScroll;

		if (drag) {

		} else if (held) { 
			Vec newPos = pointers[0].canvas + offset;
			scroll.pos = newPos.Stepped();
		} else if (pull) {
			Vec newArea = (pointers[0].canvas + offset) - Corners(scroll)[0];
			newArea -= new Vec(0, 25);

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

		if (pointers[0].dwn && pointers[1].dwn) {
			pointerDelta = pointers[0].screen - pointers[1].screen;
			oldScale = Scale;

			canvasOffset = Canvas - (pointers[0].screen / Scale);
			drag = true;
		}

		if (!pointers[0].dwn || pointers[1].dwn) return;
		// Console.WriteLine($"screen {pointers[0].screen}\ncanvas {pointers[0].canvas}\n");

		if (pointers[0].index == -1) {
			if (pointers[0].dbl) {
				drag = pull = false;
				held = true;
				
				Scroll newScroll = new Scroll();
				newScroll.area = new Vec(60, 20);
				newScroll.pos = pointers[0].canvas.Stepped() - newScroll.area - new Vec(0, 25);
				offset = newScroll.pos - pointers[0].canvas;
				Scrolls.Add(newScroll);
				StateHasChanged();
			} else {
				canvasOffset = Canvas - (pointers[0].screen / Scale);
				drag = true;
			}
		} else {
			Scroll scroll = Scrolls[pointers[0].index];
			// Console.WriteLine($"{scroll.name}\npos  {scroll.pos}\narea {scroll.area}\n");

			// check if pointer is not over textarea
			Vec localPos = pointers[0].canvas - scroll.pos;
			if (localPos.y < 30)
				return;

			if (pointers[0].dbl) {
				scroll.edit = !scroll.edit;
			}
			
			if (!scroll.edit) {
				// Lift
				if (Scrolls.Count > 1) {
					Scrolls.RemoveAt(pointers[0].index);
					Scrolls.Add(scroll);
				}

				if ((pointers[0].canvas - Corners(scroll)[2]).Mag < 30.0) {
					offset = new Vec(0, 0);
					pull = true;
				} else {
					offset = scroll.pos - pointers[0].canvas;
					held = true;
				}
			}
			StateHasChanged();
		}
	}

	public void PointerLong() {
		for (int i = Scrolls.Count-1; i >= 0; i--) {
			Scrolls[i].edit = false;
		}
		StateHasChanged();
	}

	protected void PointerUp(PointerEventArgs e) {
		for (int i = 0; i < pointers.Length; i++) {
			if (e.PointerId == pointers[i].id) {
				pointers[i].Up();
				break;
			}
		}

		if (pointers[1].dwn) {
			pointers[0] = pointers[1];
			pointers[1] = new Pointer(this);
			canvasOffset = Canvas - (pointers[0].screen / Scale);
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

		Canvas.x = (int)Canvas.x;
		Canvas.y = (int)Canvas.y;
		// snap to 10px grid if zoomed out
		if (Scale == 0.1) {
			Canvas.x = (int)(Canvas.x / 10) * 10;
			Canvas.y = (int)(Canvas.y / 10) * 10;
		}

		drag = held = pull = cull = false;
		StateHasChanged();
	}

	public Pointer[] pointers = new Pointer[2];
	public Pointer MainPointer { get => pointers[0]; }
	Vec pointerDelta = new Vec();

	Vec offset = new Vec(0, 0);

	Vec canvasOffset = new Vec(0, 0);
	public Vec Canvas = new Vec(0, 0);
	public double Scale = 1;
	double oldScale = 1;

	protected bool drag = false;
	protected bool held = false;
	protected bool pull = false;
	protected bool cull = false;


	protected bool Running = false;
	// replace with scroll.running? as the important thing is that they don't overlap?
	protected bool Error = false;

	string runText = "";
	public async Task Run(Scroll scroll) {
		if (Running) {
			// Cancel
			Running = false;
			scroll.text = runText;
		} else {
			Running = true;

			runText = scroll.text;
			scroll.text = $"{scroll.text}...";

			stream = "";
			await Read(GetScroll(runText.Split("><")[0]));
			// Console.WriteLine(stream);

			if (!Running) return;
			await Complete(stream, GetScroll(runText.Split("><")[1]));
			Running = false;

			scroll.text = runText;
			StateHasChanged();
		}
	}

	string stream = "";
	async Task Read(Scroll scroll) {
		bool read = false;
		string reference = "";
		string text = scroll.text;
		for (int i = 0; i < text.Length; i++) {
			if (!Running) break;

			scroll.text = text.Insert(i, "[").Insert(i+2, "]");
			StateHasChanged();
			await Task.Delay(20);

			if (text[i] == '{') { 
				read = true; 
				continue;
			}
			if (text[i] == '}') { 
				await Read(GetScroll(reference));
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

	Scroll GetScroll(string name) {
		foreach (Scroll scroll in Scrolls) {
			if (scroll.taglessName.Trim().ToLower() == name.Trim().ToLower()) {
				return scroll;
			}
		}
		Console.WriteLine($"Scroll {name} not found");
		return new Scroll();
	}

	protected async Task Complete(string prompt, Scroll scroll) {
		string oldText = scroll.text;
		scroll.text = "";

		try {
			CompletionRequest request = new CompletionRequest();
			request.Model = OpenAI.Models.Model.Davinci;
			request.Prompt = prompt;
			request.MaxTokens = MaxTokens;
			request.Temperature = Math.Clamp(Temperature, 0.0, 1.0);
			request.PresencePenalty = Math.Clamp(Contrast, -2.0, 2.0);
			request.FrequencyPenalty = Math.Clamp(-Cyclical, -2.0, 2.0);

			var endpoint = API.CompletionsEndpoint;
			int tokens = 0;
			await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
				if (!Running) break;

				scroll.text += token.Completions[0].Text;
				scroll.text = scroll.text.TrimStart('\n');
				tokens++;
				StateHasChanged();
			}
			Console.WriteLine(tokens);

			// Console.WriteLine($"++ {Prompter}");
		}
		catch (Exception ex) {
			Running = true;
			Error = true;
			StateHasChanged();
			await Task.Delay(2000);
			Error = false;

			Console.WriteLine($"-- {Prompter} : {ex.Message}");
		}

		if (scroll.text == "") {
			scroll.text = oldText;
		}
	}
	protected int   MaxTokens   = 256;
	protected double Temperature = 1.0f;
	protected double Contrast    = 0.0f;
	protected double Cyclical    = 0.0f;
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
