var websocket;
var wsUri = "ws://localhost:9000/";
var messageNumber = 0;
(function () {
    websocket = new WebSocket(wsUri);

    websocket.onopen = function (event) {
        document.getElementById('chat').value = ' ';
        document.getElementById('chat').value.concat('Connected!' + '\n');
    };
    websocket.onclose = function (event) {
        document.getElementById('chat').value.concat('Connection Closed.');
    };
    websocket.onmessage = function (event) {
        messageNumber += 1;
        var chatbox = document.getElementById('chat');
        if (messageNumber % 5 === 0) {
            chatbox.value = ' ';
        }
        console.log(event.data);
        //chatbox.value.concat('\n'+event.data);
    };
})();

function SendMessage() {
    //nu merge concat
    var name = document.getElementById('Name').value;
    if (name != '' || name != undefined) {
        var message = document.getElementById('Say').value;
        document.getElementById('chat').value = name + " " + message;
        websocket.send(name + ': ' + message);
    }
}