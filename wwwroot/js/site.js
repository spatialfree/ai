// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$('.btn').click(function() {
  $(this).toggleClass('pressed')
  setTimeout(()=> {
    $(this).toggleClass('pressed')
  }, 100);
})

var davinci = ''
var stringArray = ''
$(function() {


})

ideate = () => {
  fetch('/mono', {
    method: 'POST',
    body: 'Once upon a time ',
  })
    .then(response => response.body)
    .then(async rs => {
      const decoder = new TextDecoder()
      const reader = rs.getReader()
      while (true) {
        const { done, value } = await reader.read()
        var token = decoder.decode(value)
        stringArray += token
        let json = JSON.parse(
          stringArray + (stringArray.endsWith(']') ? '' : ']')
        )
        
        $('#p2').text(json.join(''))
        if (done) {
          console.log('done')
          break
        }
      }
    })
}
// var davinci = ''
// $(function() {
//   fetch('/mono')
//     .then(response => response) // .text()
//     .then(data => {
//       let json = JSON.parse(data)
//       console.log(json)
//       davinci = json.prompt
//       $('#dalle').attr('src', json.image)
//     })
// })

var ticker = 0
step = (delta) => {
  ticker += delta
  if (ticker > 0.0666) {
    tick()
    ticker = 0
  }
}

tick = () => {
  // if (davinci.length > 0) {
  //   let text = $('#mission').text()
  //   text += davinci[0]
  //   davinci = davinci.slice(1)
  //   $('#mission').text(text)
  // }
}

loop = (timestamp) => {
  step((timestamp - lastRender) / 1000)

  lastRender = timestamp
  window.requestAnimationFrame(loop)
}
var lastRender = 0
window.requestAnimationFrame(loop)