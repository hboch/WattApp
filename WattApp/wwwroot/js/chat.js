"use strict";

const connection = new signalR.HubConnectionBuilder()
	.withUrl("/chatHub")
	.build();

const spanGlyphiconUser = "<span class='glyphicon glyphicon-user'></span>&nbsp;";
const spanGlyphiconUserTyping = " <span class='glyphicon glyphicon-pencil'></span>";

function IsNullOrWhiteSpace(str) {
	return str == null || str.replace(/\s/g, '').length < 1;
}

function GetElementValueById(elementId) {
	return document.getElementById(elementId).value;
}

function GetUserId() {
	var userId = $("#hiddenUserId").val();	
	return userId;
}

function GetUserName() {
	var userName = $("#hiddenUserName").val();
	return userName;
}

function GetMessage() {
	return GetElementValueById("inputMsg");
}

function ClearMessageInput() {
	document.getElementById("inputMsg").value = "";
}

function ClearTypingIcon(userId, userName) {
	document.getElementById(userId).innerHTML = spanGlyphiconUser + userName;
}

connection.on("onMessageReceive", function (userId, userName, message) {
	ClearTypingIcon(userId, userName)

	var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
	var encodedMsg = userName + ": " + msg;
	var li = document.createElement("li");
	li.textContent = encodedMsg;
	document.getElementById("messagesList").appendChild(li);
});

connection.on("onMessageTyping", function (userId, userName) {
	document.getElementById(userId).innerHTML = spanGlyphiconUser + userName + spanGlyphiconUserTyping;
});

connection.on("onConnectedThisUser", (userId, userName) => {
	$('#hiddenUserId').val(userId);
	$('#hiddenUserName').val(userName);
});

connection.on("onConnected", (users) => {	
	//Empty logged in user list before filling it again 
	$("#usersList").empty();
	for (var i = 0; i < users.length; ++i) {
		$("#usersList").append($("<li class='list-group-item list-group-item-info' Id='" + users[i].userId + "'>").html(spanGlyphiconUser + users[i].name));
	}
});

//connection.on("onConnectedNewUser", (userId, userName) => {
//	$("#usersList").append($("<li class='list-group-item list-group-item-info' Id='" + userId + "'>").html(spanGlyphiconUser + userName));
//});

connection.on("onDisconnected", (userId) => {
	//$("#usersList li:contains('" + name + "')").remove();
	document.getElementById(userId).remove();
});

connection.start().then(function () {
	connection.invoke("Connect").catch(err => console.error(err.toString()));
});

document.getElementById("inputMsg").addEventListener("input", function (event) {
	if (IsNullOrWhiteSpace(this.value)) {
		ClearTypingIcon(GetUserId(), GetUserName());
	}
	else {
		connection.invoke("MessageTyping", GetUserId(), GetUserName()).catch(function (err) {
			return console.error(err.toString());
		});
	}
});

document.getElementById("btnSend").addEventListener("click", function (event) {	
	connection.invoke("MessageSend", GetUserId(), GetUserName(), GetMessage()).catch(function (err) {
		return console.error(err.toString());
	});
	event.preventDefault();

	ClearTypingIcon(GetUserId(), GetUserName());
	ClearMessageInput();
});

//document.getElementById("btnExitChat").addEventListener("click", function (event) {
//	connection.invoke("Disconnect").catch(err => console.error(err.toString()));
//	connection.stop()
//	$('#hiddenId').val('');
//	$('#hiddenUserName').val('');
//	$("#usersList").empty();
//	$("#messagesList").empty();
//});
