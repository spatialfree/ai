namespace ai;
using System.IO;

public class Mono {
	public string compass { get{ return @"
to = prompt patterning habitat
	branchial~
	sequenced
	programmatic
	modular
	on demand memory

from = conversational ai
		1 : 1 back and forth
		limited memory

	blocks
		pre determined
		physically connecting systems
		resembles text based code
		linear
		snaps together
		auto expand and shrink
		strong silhoeuttes

	nodes
		pre determined
		physically connecting data
		branchial


delta = (to - from)
direction = delta / delta.length


constraints
	mobile 1st
		target userbase mobile preference >50%
	text 1st
		meet the language model where it's at


works
	scroll style classes
	long press on canvas starts box select
	token usage
		auto limit by textarea area *2x the visible area
		estimates
		history
		totals
	reference.details
	arrays~
	generic encapsulation
	libraries~
	boards/patterns~


pos = from + direction * works
	canvases
		local insances (in memory)
		one cloud shared instance
	scrolls
		edit text/shape
		customizable
		dynamic
		inline references
	run
		read><complete
		completion sequences
		read
			recursive reference parser
			rendered
		complete
			stream ai output
	page
		surface scrolls
		live style editing


	
	";}}

	public OpenAIClient API = default!;
  public Mono(string? apikey) {
		API = new OpenAIClient(new OpenAIAuthentication(apikey));
    Restore();
		Scrolls.CollectionChanged += (sender, e) => {
			Recorded = Restored = false;
			// this only gets called when the collection is changed
			// not when the properties of the scrolls are changed
		};
  }

	public bool Synced {
		get { return Recorded || Restored; }
	}
	

	public string Pattern = "Pattern";


	// ObvserableCollection?
	public ObservableCollection<Scroll> Scrolls = new();

  string cd { get { return Directory.GetCurrentDirectory(); } }
  string dir { get { return $"{cd}/Records/"; } }

	public void InitRecords() {
		Directory.CreateDirectory(dir);
		for (int i = 0; i < 10; i++) {
			File.CreateText($"{dir}{i}");
		}
		File.Move($"{dir}0", $"{dir}0)");
	}

	public bool Recorded = false;
	public void Record() {
		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				int index = int.Parse(name.Remove(1));
				File.Move(path, $"{dir}{index}");
				
				index = Tools.RollOver(index, 1, files.Length);
				path = $"{dir}{index}";
		
				string contents = $"{TimeStamp}\n___";
				for (int j = 0; j < Scrolls.Count; j++) {
					contents += $"\nName  {Scrolls[j].name.Trim()}";
					contents += $"\nText  {Scrolls[j].text.Replace("\n", "\\n") }";
					contents += $"\nPos   {Scrolls[j].pos.Stepped()}";
					contents += $"\nArea  {Scrolls[j].area.Stepped()}";
					contents += $"\n";
				}

				File.WriteAllText(path, contents);
				// File.WriteAllTextAsync()
				File.Move(path, $"{path})");

				Recorded = true;
				break;
			}
		}
	}

	public bool Restored = false;
	public void Restore() {
		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				bool meta = true;
				Scrolls.Clear();
				string[] lines = File.ReadAllLines(path);
				for (int j = 0; j < lines.Length; j++) {
					if (meta) {
						meta = !lines[j].Contains("___");
					} else {
						if (lines[j].Trim() == "") {
							continue;
						}

						Scroll scroll = new Scroll {
							name  = lines[j++].Replace("Name  ", "").Trim(),
							text  = lines[j++].Replace("Text  ", "").Replace("\\n", "\n"),
							pos   = lines[j++].Replace("Pos   ", "").ToVec(),
							area  = lines[j++].Replace("Area  ", "").ToVec()
						};

						Scrolls.Add(scroll);
					}
				}
				Restored = true;
			}
		}
	}
  
	public string TimeStamp { get {
		string date = DateTime.Now.ToShortDateString();
		string time = DateTime.Now.ToShortTimeString();
		return $"{date}\n{time}";
	}}
}


/*


*/