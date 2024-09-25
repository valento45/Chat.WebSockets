let socket = new WebSocket("wss://localhost:7074"); // Conectar ao servidor WebSocket


let nameClient = "";

socket.onopen = function (event) {
    document.getElementById("chatbox").innerHTML += "<div>Connection established</div>";
};

socket.onmessage = function (event) {
    document.getElementById("chatbox").innerHTML += `<div>${event.data}</div>`;
};

socket.onclose = function (event) {
    document.getElementById("chatbox").innerHTML += "<div>Connection closed</div>";
};

function sendMessage() {
    let message = `${nameClient}: ${document.getElementById("message").value}`;
    socket.send(message); // Enviar mensagem para o servidor
    document.getElementById("message").value = ""; // Limpar campo de mensagem
}


function acessarChat() {
    nameClient = $("#txtNomeCliente").val();


    if (nameClient) {
        $("#pnlName").removeClass('d-flex');
        $("#pnlName").addClass('d-none');

        $("#pnlChat").removeClass('d-none');
      
    }
}