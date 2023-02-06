namespace ai;

public class IndexBase : ComponentBase {
	[Inject] protected Mono mono { get; set; } = default!;

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


	string pattern = "";
	protected string Pattern {
		get { return pattern; }
		set { 
			pattern = value;
			Cloud = pattern == mono.Pattern;
		}
	}
	protected bool Cloud = false;
	protected bool Menu  = true;

	ObservableCollection<Node> nodes { get; set; } = new() {
		new Node { pos = new Vec(60, 100), area = new Vec(80, 20), name = "read", text = "Lorem ipsum " },
		new Node { pos = new Vec(200, 100), area = new Vec(150, 100), name = "write", text = "" },
	};
	public ObservableCollection<Node> Nodes {
		get { return Cloud ? mono.Nodes : nodes; }
	}
	public Node TopNode {
		get { return Nodes[Nodes.Count - 1]; }
	}
	public Node NextNode {
		get { return Nodes[Nodes.Count - 2]; }
	}
	public Vec[] Corners(Node node) {
		int inset = 10;
		Vec[] corners = new Vec[4];
		corners[0] = node.pos + new Vec(inset, inset);
		corners[1] = node.pos + new Vec(node.area.x + 20 - inset, inset);
		corners[2] = node.pos + node.area + new Vec(20 - inset, 40 - inset);
		corners[3] = node.pos + new Vec(inset, node.area.y + 40 - inset);
		return corners;
	}
	// closest corner pair for Top and Next Node
	public Vec[] Closest {
		get {
			Vec[] closest = new Vec[2];
			Vec[] corners = Corners(TopNode);
			Vec[] nextCorners = Corners(NextNode);
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


	DateTime lastDown = DateTime.Now;
	protected void PointerMove(PointerEventArgs e) {
		// Console.WriteLine($"PointerMove {e.PointerId}");
		Cursor = new Vec(e.ClientX, e.ClientY);

		Node node = Nodes[Nodes.Count - 1];

		if (held) { 
			Vec newPos = LocalCursor + offset;
			node.pos = newPos.Stepped();
		} else if (pull) {
			Vec newArea = (LocalCursor + offset) - node.pos;
			newArea -= new Vec(5, 20);

			cull = newArea.x < 0 && newArea.y < 0;
			
			newArea.x = Math.Max(newArea.x.Stepped(), 60);
			newArea.y = Math.Max(newArea.y.Stepped(), 20);
			node.area = newArea;
		} else if (down) {
			Canvas = Cursor + canvasOffset;
		}

		StateHasChanged();
	}

	protected void PointerDown(PointerEventArgs e) {
		// Console.WriteLine($"PointerDown : {e.Button}");
		Cursor = new Vec(e.ClientX, e.ClientY);

		TimeSpan time = DateTime.Now - lastDown;
		bool doubleDown = (Cursor - oldCursor).Mag < 20 &&time.TotalMilliseconds < 500;
		lastDown = DateTime.Now;

		for (int i = Nodes.Count-1; i >= 0; i--) {
			Node node = Nodes[i];

			Vec localPos = LocalCursor - node.pos;
			bool inXMin = localPos.x >= 0;
			bool inXMax = localPos.x < node.area.x + 20;
			// print 0 for inside both and - for outside min and + for outside max
			int x = (inXMin ? 0 : -1) + (inXMax ? 0 : 1);
			string xstr = x == 0 ? "0" : x < 0 ? "-" : "+";

			bool inYMin = localPos.y >= 0;
			bool inYMax = localPos.y < node.area.y + 40;
			int y = (inYMin ? 0 : -1) + (inYMax ? 0 : 1);
			string ystr = y == 0 ? "0" : y < 0 ? "-" : "+";

			// Console.WriteLine($"{xstr}{ystr} {(int)e.ClientX - node.pos.x}");
			if (inXMin && inXMax && inYMin && inYMax) {
				if (Loading || !edit) return;


				oldPos = node.pos;

				// Lift
				if (Nodes.Count > 1) {
					Nodes.RemoveAt(i);
					Nodes.Add(node);
				}

				if ((LocalCursor - Corners(node)[2]).Mag < 10.0) {
					offset = Corners(node)[2] - LocalCursor;
					pull = true;
				} else {
					offset = node.pos - LocalCursor;
					held = true;
				}


				StateHasChanged();
				return;
			}
		}

		if (edit && doubleDown) {
			Node newNode = new Node();
			newNode.pos = LocalCursor.Stepped();
			newNode.area = new Vec(60, 20);
			pull = true;
			Nodes.Add(newNode);
		} else {
			canvasOffset = Canvas - Cursor.Stepped();
			down = true;
		}
	}

	protected void PointerUp(PointerEventArgs e) {
		// Console.WriteLine($"PointerUp : {e.Button}");
		Cursor = new Vec(e.ClientX, e.ClientY);

		if (cull) {
			if (Nodes.Count <= 2) {
				Nodes[Nodes.Count - 1].name = "";
				Nodes[Nodes.Count - 1].text = "";
			} else {
				Nodes.RemoveAt(Nodes.Count - 1);
			}
			// Console.WriteLine("CULL");
		} else if (down) { // need a new bool for grabbing the canvas
			Canvas = Cursor.Stepped() + canvasOffset;
		}

		oldCursor = Cursor;
		down = held = pull = cull = false;
		StateHasChanged();
	}

	Vec oldCursor = new Vec(0, 0);
	Vec Cursor 		= new Vec(0, 0);
	Vec LocalCursor { get { return Cursor - Canvas; } }

	Vec offset = new Vec(0, 0);
	Vec canvasOffset = new Vec(0, 0);
	protected Vec Canvas = new Vec(0, 0);

	Vec oldPos = new Vec(0, 0); // vague variable name 
	
	protected bool down = false;
	protected bool held = false;
	protected bool pull = false;
	protected bool cull = false;

	protected bool edit  = false;


	protected async Task Run() {
		Node node = Nodes[Nodes.Count - 2];

		string prep = "";

		bool read = false;
		string reference = "";
		for (int i = 0; i < node.text.Length; i++) {
			if (node.text[i] == '}') { read = false; 
				Node refNode = GetNodeByName(reference);
				prep += refNode.text;

				reference = "";
				continue;
			}
			if (read) { reference += node.text[i]; }
			if (node.text[i] == '{') { read = true; }

			if (!read) { prep += node.text[i]; }
		}

		// Console.WriteLine(prep);
		// Nodes[Nodes.Count - 1].text = completion;
		await Complete(prep);
		// Console.WriteLine(Nodes[Nodes.Count - 1].text);
	}

	Node GetNodeByName(string name) {
		foreach (Node node in Nodes) {
			if (node.name.Trim().ToLower() == name.Trim().ToLower()) {
				return node;
			}
		}
		Console.WriteLine($"Node {name} not found");
		return new Node();
	}

	protected bool Loading = false;
	protected bool Error = false;
	// string completion = "";
	protected async Task Complete(string prompt) {
		Nodes[Nodes.Count - 1].text = "";
		if (!Loading) {
			Loading = true;
			edit = false;

			try {
				CompletionRequest request = new CompletionRequest();
				request.Model = OpenAI.Models.Model.Davinci;
				request.Prompt = prompt;
				request.MaxTokens = MaxTokens;
				request.Temperature = Temperature;
				request.PresencePenalty = Contrast;
				request.FrequencyPenalty = -Repeat;

				var endpoint = API.CompletionsEndpoint;
				await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
					Nodes[Nodes.Count - 1].text += token.Completions[0].Text;
					Nodes[Nodes.Count - 1].text = Nodes[Nodes.Count - 1].text.TrimStart('\n');
					
					StateHasChanged();
				}

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

			Loading = false;
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
