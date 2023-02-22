namespace ai;
using System.IO;

public class Mono {
	public string compass = @"
to = prompt patterning habitat
	process/behavior
		branchial~
		sequenced
		programmatic
		modular
	on demand memory

	scrolls
	cascading completions
	user created
	expandable
	inline data connections
	context/convention control flow
		left-right top-bottom
		chained based on contact?
		how they are triggered?

from = conversational ai
		1 : 1 back and forth
		limited memory

	Blocks
		pre determined
		physically connecting systems
		resembles text based code
		linear
		snaps together
		auto expand and shrink
		strong silhoeuttes

	Nodes
		pre determined
		physically connecting data
		branchial


delta = (to - from)
direction = delta / delta.length


constraints
	Mobile 1st
		target userbase mobile preference >50%
	Text 1st
		meet the language model where it's at


works
	Token usage
		auto Limit by textarea area *2x the visible area
		estimates
		history
		totals
	Reference.details
	Arrays~
	Generic encapsulation
	Libraries~
	Boards/Patterns~


pos = from + direction * works
	Canvases
		local insances (in memory)
		one cloud shared instance
	Scrolls
		edit text/shape
		customizable
		dynamic
		inline references
	Run
		read><complete
		Read
			recursive reference parser
			rendered
		Complete
			stream ai output
	Page
		surface scrolls
		live style editing


	
	";
  public Mono() {
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