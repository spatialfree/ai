using System.Text.RegularExpressions;
using System.IO;

namespace ai;
public class Pattern { // Data focused class
	public bool cloud    = false;
	public bool recorded = false;
	public bool restored = false;
	public bool synced
		=> recorded || restored;

	public Pattern() {
		recorded = restored = Records.Restore(this);


		cloud = true; // temp for testing


		scrolls.CollectionChanged += (sender, e) => {
			recorded = restored = false;
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
		
		public bool executable 
			=> text.Contains("><"); // unoptimized


		public string name;
		public string taglessName
			=> Regex.Replace(name, @"<[^>]*>", ""); 

		public string text;

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
		public bool temp = false;

		public Vec pos;
		public Vec area;

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

	public string style = @"
.page {
  max-width: 400px;
  margin: 0 auto;
}
h1 { 
  display: block; 
  font-size: 40px;
}
p {
  display: block;
  padding-bottom: 10px;
}
label {
  display: block;
  padding-top: 20px;
}
input {
  display: block; 
  width: -webkit-fill-available;
  margin: 5px 0 10px 0; padding: 5px 10px;
  border-bottom: 1px solid black;
}
button {
  display: block;
  margin: 20px auto; padding: 10px 15px;
  border: 1px solid black;
  box-shadow: 2px 2px;
  font-weight: 700;
  letter-spacing: 1px;
}
img {
  display: block;
  width: 100%;
  margin: 0 auto;
}
.banner {
  object-fit: cover;
  height: 128px;
  overflow: hidden;
  margin-bottom: 10px;
}
footer {
	display: block;
	margin: 20px auto;
	margin-top: 100px;
	font-size: 10px;
	text-align: center;
	color: #808080;
}";

	public string styleRender(bool styling) {
		// split into lines
		// and each line with an opening bracket {
		// gets .page added to the start of the line
		string[] lines = style.Split('\n');
		for (int i = 0; i < lines.Length; i++) {
			if (lines[i].Contains("@") || lines[i].Contains(";"))	
				continue;

			if (lines[i].Contains("{") && !lines[i].Contains(".page")) {
				lines[i] = ".page>" + lines[i];
			}
		}
		string str = string.Join("\n", lines);
		return styleHeader + (styling ? ".page>*{ transition: all 0.5s ease; }" : "") + str;
	}
}