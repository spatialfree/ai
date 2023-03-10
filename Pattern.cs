using System.Text.RegularExpressions;
using System.IO;

namespace ai;
public class Pattern { // Data focused class
	public bool synced = false;

	public Pattern(bool restore, string name = "") {
		this.name = name;
		if (restore) {
			Records.Restore(this);
		}
		
		scrolls.CollectionChanged += (sender, e) => {
			synced = false;
			// warning
			// this only gets called when the collection is changed
			// not when other persistent properties are changed
		};
	}

	public string name = "";
	public ObservableCollection<Scroll> scrolls = new();
	public ObservableCollection<Scroll> sorted {
		// sort top to bottom and left to right using scroll.pos
		get { return new(scrolls.OrderBy(s => s.pos.y).ThenBy(s => s.pos.x)); }
	}
	public Scroll top => scrolls[scrolls.Count - 1];


	public Scroll GetScroll(string name) {
		foreach (Scroll scroll in scrolls) {
			if (scroll.taglessName.Trim().ToLower() == name.Trim().ToLower()) {
				return scroll;
			}
		}
		Console.WriteLine($"Scroll {name} not found");
		return new Scroll(this);
	}


	public class Scroll {
		Pattern pattern;
		public Scroll(Pattern pattern) {
			this.pattern = pattern;
			name = "";
			text = "";

			pos = new Vec(0, 0);
			area = new Vec(0, 0);
		}

		// stored
		public bool temp   { get; set; }
		public string name { get; set; }
		public string text { get; set; }
		public Vec pos     { get; set; }
		public Vec area    { get; set; }
		
		public bool executable 
			=> text.Contains("><"); // unoptimized

		public string taglessName
			=> Regex.Replace(name, @"<[^>]*>", ""); 


		string stream = "";
		public string fulltext {
			get {
				stream = "";
				Read(this);
				return stream;
			}
		}

		public int tokens { // fast and loose approximation
			get {
				if (text.Contains("><")) {
					string[] lines = text.Split('\n');
					int total = 0;
					for (int i = 0; i < lines.Length; i++) {
						if (lines[i].Contains("><")) {
							total += pattern.GetScroll(lines[i].Split("><")[0]).tokens;
						}
					}
					return total;
				}
				if (text.Contains("{") && text.Contains("}")) {
					return fulltext.Length / 4;
				}
				return text.Length / 4;
			}
		}

		void Read(Scroll scroll) {
			bool read = false;
			string reference = "";
			string txt = scroll.text;
			for (int i = 0; i < txt.Length; i++) {
				if (txt[i] == '{') { 
					read = true; 
					continue;
				}
				if (txt[i] == '}') { 
					Read(pattern.GetScroll(reference));
					reference = "";
					read = false; 
					continue;
				}
				if (read) { 
					reference += txt[i]; 
				} else {
					stream += txt[i]; 
				}
			}
		}

		public bool edit = false;


		

		// public string Full => $"{Tools.Formatted(Label,"\n")}{Tools.Formatted(Text,"\n\n")}";

		public bool Contains(Vec pointer) {
			Vec localPos = pointer - pos;
			bool inXMin = localPos.x >= 0;
			bool inXMax = localPos.x < area.x + 20;
			int x = (inXMin ? 0 : -1) + (inXMax ? 0 : 1);

			bool inYMin = localPos.y >= 0;
			bool inYMax = localPos.y < area.y + 50;
			int y = (inYMin ? 0 : -1) + (inYMax ? 0 : 1);
			// print 0 for inside both and - for outside min and + for outside max
			// if (scroll == TopScroll) {
			// 	string xstr = x == 0 ? "0" : x < 0 ? "-" : "+";
			// 	string ystr = y == 0 ? "0" : y < 0 ? "-" : "+";
			// 	Console.WriteLine($"{xstr}{ystr} {(int)pointers[0].canvas.x - scroll.pos.x}");
			// }
			return inXMin && inXMax && inYMin && inYMax;
		}

		public string color {
			get {
				int b = 120 + (executable ? 60 : 0);
				int g = 80  + (name != taglessName ? 80 : 0);
				int r = 100 + (text.Contains("{") && text.Contains("}") ? 100 : 0);
				return $"rgb({r},{g},{b})";
			}
		}
	}

	string styleHeader
		=> @"
		.page>* { all: unset; font-family: 'Atkinson Hyperlegible', Helvetica, sans-serif; }
		.page { overflow-y: auto; touch-action: pan-y; }
	";

	public string style = "";

	string lastStyle = "";
	public string styleRender(bool styling) {
		// split into lines
		// and each line with an opening bracket {
		// gets .page added to the start of the line
		string[] lines = style.Split('\n');
		for (int i = 0; i < lines.Length; i++) {
			if (lines[i].Contains(":") && !lines[i].Contains(";"))
				return lastStyle;

			if (lines[i].Trim().Length > 0 && !lines[i].Contains(":") && !lines[i].Contains("{") && !lines[i].Contains("}"))
				return lastStyle;

			if (lines[i].Contains("@") || lines[i].Contains(";"))	
				continue;

			if (lines[i].Contains("{") && !lines[i].Contains(".page")) {
				lines[i] = ".page>" + lines[i];
			}
		}

		return lastStyle = styleHeader
			+ (styling ? ".page>*{ transition: all 0.5s ease; }" : "")
			+ string.Join("\n", lines);;
	}
}