namespace ai;
public class Mono {
	public string compass { get{ return @"
to = apt(aptitude) platform
	prompt composition
	html expositions
	css 
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
	deeply integrate the stragegy pattern
		abstraction
		encapsulation
		interface
	
	the same way we make terms
		we make patterns

	not sure what to call the thing as a whole
		site, app, program, tool, service, platform, etc

	but that is the core piece that needs to be done well
		to build at scale

	recycling decoder using c# reflection 

	entirely separate directory with it's own git
	
	1 file foreach pattern->habit->

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


built in operations
	largely handled by LLM
variables
	ref scroll text by name
branching
	conditional (if else)
	handled by LLM?
looping
functions
	run scroll text by name
input/output
	html

Records:
idea-assist
assesment-tool
samantha
blank

paul and simon can help

nested text docs
	*what we are doing now/here
vvv
in context docs
	long press?
	by embedding doc info into the elements themselves?
vvv
docs apt




web free
	waste not want not
	take not give not

reputation based
	access

I want to be able to export a pattern
so implement pattern.raw

conditionals
	branching
		name|name
	
	";}}

	public OpenAIClient api = default!;
  public Mono(string? secretkey, string? envKey) {
		api = new OpenAIClient(
			new OpenAIAuthentication(envKey != null ? envKey : secretkey)
		);
  }
	
	public int tokens = 100_000; // ~2$ per 100k tokens
	// manual reup, by relaunching the app
	// for now...
}


/*


*/