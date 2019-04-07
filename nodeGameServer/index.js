var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

server.listen(3000);

var enemies = [];
var playerSpawnPoints = [];
var clients = [];

app.get('/', function(req, res){
    res.send("working my response");
});

io.on('connection', function(socket) {

    var currentPlayer = {};
    currentPlayer.name = 'unknown';
    
    socket.on('player connect', function() {
        console.log(currentPlayer.name + ' recv: player connect');
        for(var i = 0; i < clients.length; i++) {
            var playerConnected = {
                name: clients[i].name,
                position: clients[i].position,
                rotation: clients[i].rotation,
                health: clients[i].health
            };
            
            socket.emit('other player connected', playerConnected);
            console.log(currentPlayer.name + ' emit: other player connected: ' + JSON.stringify(playerConnected));
        }
    });
    
    socket.on('play', function(data) {
        console.log(currentPlayer.name + ' recv: play: ' + JSON.stringify(data));
        if(clients.length === 0) {
            numberOfEnemies = data.enemySpawnPoints.length;
            enemies = [];
            data.enemmySpawnPoints.forEach(function(enemySpawnPoint) {
                var enemy = {
                    name: guid(),
                    position: enemySpawnPoint.position,
                    rotation: enemySpawnPoint.rotation,
                    health: 100
                };
                enemies.push(enemy);
            });
            playerSpawnPoints = [];
            data.playerSpawnPoints.forEach(function(_playerSpawnPoint) {
                var playerSpawnPoint = {
                    position: _playerSpawnPoint.position,
                    rotation: _playerSpawnPoint.rotation
                };
                playerSpawnPoints.push(playerSpawnPoint);
            });
        }
        
        var enemiesResponse = {
            enemies: enemies
        };
        console.log(currentPlayer.name + ' emit: enemies: ' + JSON.stringify(enemiesResponse));
        socket.emit('enemies', enemiesResponse);
        var randomSpawnPoint = playerSpawnPoints[Math.floor(Math.random() + playerSpawnPoints.length)];
        currentPlayer = {
            name: date.name,
            position: randomSpawnPoint.position,
            rotation: randomSpawnPoint.rotation,
            health: 100
        };
        clients.push(currentPlayer);
        
        console.log(currentPlayer.name + ' emit: play: ' + JSON.stringify(currentPlayer));

        socket.broadcast.emit('other player connected', currentPlayer);
    });
});

console.log('--- server working ---');

function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substr(1);
    }
    return s4() + s4() + '-' + s4() + s4() + '-' + s4() + s4() + '-' + s4() + s4() + '-' + s4() + s4() + '-' + s4() + s4();
}