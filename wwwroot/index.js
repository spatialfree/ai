// document.body.style.cursor = 'none';

// function setContent(editableDiv, content) {
//   editableDiv.innerHTML = content;
// 	// console.log(editableDiv.innerHTML)
// }

function setText() {
	var divs = document.getElementsByTagName("div")
	for (var i = 0; i < divs.length; i++) {
		var div = divs[i]
		// if (reset) {
		// 	div.setAttribute("init", "1")
		// }

		if (div.getAttribute("init")) {
			div.innerHTML = div.getAttribute("init-value")
			// div.setAttribute("init", "")
		}
	}
}



document.addEventListener("input", onInput)
function onInput(e) {
	let target = e.target
	if (target.localName == "div") {
		if (!target.value && !target.__contenteditable) target.__contenteditable = true
		if (target.__contenteditable) {
			target.value = target.innerText
		}
	}
}


// console.log();
// target.setAttribute('txt', target.innerText)