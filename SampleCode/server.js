var express = require('express');
var app = express();

var server = require('http').createServer(app);
var io = require('socket.io').listen(server);

app.set('port', process.env.PORT || 999);

var clients = [];

io.on('connection', function (socket) {

    var currentUser;

    socket.on('USER_CONNECT', function (data){
        console.log('user connected');

        console.log(data);

        var jsonData = JSON.parse(data);

        currentUser = {
            id: socket.id,
            name: jsonData.name
        };

        console.log('user id >> ' + currentUser.id + ' is connected...');

        // 클라에 아이디 저장
        socket.emit('USER_CONNECTED', currentUser);
        clients.push(currentUser);

    });

    socket.on('GAME_OVER', function () {
        console.log('game over');

        socket.broadcast.emit('GAME_OVER', null);
    });

    socket.on('disconnect', function () {
        socket.broadcast.emit('USER_DISCONNECTED', currentUser);
        for (var i = 0; i < clients.length; i++)
        {
            if (clients[i].name === currentUser.name)
            {
                console.log('user ' + clients[i].name + ' disconnect...');
                clients.splice(i, 1);
            }
        }
    });
});

server.listen(app.get('port'), function () {
    console.log('--------- SERVER IS RUNNING -----------');
});