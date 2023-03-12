using Scroll = ai.Pattern.Scroll;

namespace ai;
public class IndexBase : ComponentBase {
	[Inject] protected IMetaMaskService MetaMask { get; set; } = default!;
	[Inject] protected Mono mono { get; set; } = default!;
	[Parameter] public string Url { get; set; } = "";

	// move these to the razor page
	protected bool compass = false;
	protected bool menu    = false; // -> settings
	protected bool page    = true;  // ->
	protected bool styling = false;

	protected override void OnInitialized() {	 // twice
		pointers = new Pointer[] { new Pointer(this), new Pointer(this) };
	}

	protected bool onchain = false;
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		if (firstRender) {  // once
			pattern = new(true, Url);
			onchain = Url == null || Url.Trim() == "";

			// bool hasMetaMask = await MetaMask.HasMetaMask();
			// if (hasMetaMask) {
			// 	onchain = await MetaMask.IsSiteConnected();
			// 	if (!onchain) {
			// 		try {
			// 			await MetaMask.ConnectMetaMask();
			// 		} 
			// 		catch {
			// 			Console.WriteLine("MetaMask not connected");
			// 		}

			// 		onchain = await MetaMask.IsSiteConnected();
			// 	}
			// }
			// Console.WriteLine($"MetaMask connected: {onchain}");
			StateHasChanged();
		}

		await Task.CompletedTask;
	}

	protected Pattern pattern = new(false);

	
	public Vec[] Corners(Scroll scroll) {
		int inset = 5;
		Vec[] corners = new Vec[4];
		corners[0] = scroll.pos + new Vec(inset, inset);
		corners[1] = scroll.pos + new Vec(scroll.area.x + 20 - inset, inset);
		corners[2] = scroll.pos + scroll.area + new Vec(20 - inset, 50 - inset);
		corners[3] = scroll.pos + new Vec(inset, scroll.area.y + 50 - inset);
		return corners;
	}
	// bounds of all pattern.scrolls 0,0 is top left, so the bottom right is the bounds
	public Vec Bounds {
		get {
			Vec bounds = new Vec(0, 0);
			foreach (Scroll scroll in pattern.scrolls) {
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
		foreach (Scroll scroll in pattern.scrolls) {
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

		Scroll scroll = pattern.top;

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
				pointers[i].Down(e.ClientX, e.ClientY, e.PointerId, pattern);			
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
			if (pointers[0].dbl && pattern.scrolls.Count < MAX_SCROLLS) {
				drag = pull = false;
				held = true;
				
				Scroll newScroll = new Scroll(pattern);
				newScroll.area = new Vec(60, 20);
				newScroll.pos = pointers[0].canvas.Stepped() - newScroll.area - new Vec(0, 25);
				offset = newScroll.pos - pointers[0].canvas;
				pattern.scrolls.Add(newScroll);
				StateHasChanged();
			} else {
				canvasOffset = Canvas - (pointers[0].screen / Scale);
				drag = true;
			}
		} else {
			Scroll scroll = pattern.scrolls[pointers[0].index];
			// Console.WriteLine(
			// 	$"new Scroll() {{\n" +
			// 	$"  name = \"{scroll.name.Trim()}\",\n" +
			// 	$"  text = \"{scroll.text.Replace("\n", "\\n")}\",\n" +
			// 	$"  pos  = new Vec({scroll.pos}),\n" +
			// 	$"  area = new Vec({scroll.area}),\n" +
			// 	$"}},"
			// );

			// check if pointer is not over textarea
			Vec localPos = pointers[0].canvas - scroll.pos;
			if (localPos.y < 30 && scroll.executable)
				return;

			if (pointers[0].dbl) {
				scroll.edit = !scroll.edit;
			}
			
			if (!scroll.edit) {
				// Lift
				if (pattern.scrolls.Count > 1) {
					pattern.scrolls.RemoveAt(pointers[0].index);
					pattern.scrolls.Add(scroll);
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
		for (int i = pattern.scrolls.Count-1; i >= 0; i--) {
			pattern.scrolls[i].edit = false;
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
			if (pattern.scrolls.Count <= 2) {
				pattern.top.name = "";
				pattern.top.text = "";
			} else {
				pattern.scrolls.RemoveAt(pattern.scrolls.Count - 1);
			}
			// Console.WriteLine("CULL");
		} else if (held || pull) {
			// check if matching the pos and area of another scroll
			// if so, then copy their name and text
			for (int i = pattern.scrolls.Count - 2; i >= 0 ; i--) {
				Scroll scroll = pattern.scrolls[i];
				if (scroll.pos == pattern.top.pos && scroll.area == pattern.top.area) {
					Console.WriteLine("Duplicate");
					pattern.top.name = scroll.name;
					pattern.top.text = scroll.text;
					break;
				}
			}
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
		if (mono.tokens <= 0)
			return;

		if (Running) {
			// Cancel
			Running = false;
			scroll.text = runText;
		} else {
			Running = true;

			runText = scroll.text.Trim();
			string[] runLines = runText.Split("\n");
			for (int i = 0; i < runLines.Length; i++) {
				string line = runLines[i].Trim();

				scroll.text = $"{scroll.text}...";

				stream = "";
				await Read(pattern.GetScroll(line.Split("><")[0]));

				if (!Running) return;
				await Complete(stream, pattern.GetScroll(line.Split("><")[1]));
			}
			scroll.text = runText;

			Running = false;
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
				await Read(pattern.GetScroll(reference));
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

			var endpoint = mono.api.CompletionsEndpoint;
			int tokens = 0;
			await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
				if (!Running) break;

				scroll.text += token.Completions[0].Text;
				scroll.text = scroll.text.TrimStart('\n');
				tokens ++;
				StateHasChanged();
			}
			mono.tokens -= tokens;
		}
		catch (Exception ex) {
			Running = true;
			Error = true;
			StateHasChanged();
			await Task.Delay(2000);
			Error = false;

			Console.WriteLine($"-- {ex.Message}");
		}

		if (scroll.text == "") {
			scroll.text = oldText;
		}
	}
	protected int    MaxTokens   = 256;
	protected double Temperature = 1.0f;
	protected double Contrast    = 0.0f;
	protected double Cyclical    = 0.0f;

	const int MAX_SCROLLS = 32;
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
