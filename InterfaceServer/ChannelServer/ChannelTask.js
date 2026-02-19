var http = require('http');
var querystring = require('querystring');
var mysql = require('mysql');
var net = require('net');
var url = require('url');
var db_config = {
	host: '127.0.0.1',
	user: 'root',
	password: '111112lst',
	database: 'accountdb',
	multipleStatements: false
};
var HOST  = "10.2.1.146";
var GAME_HOST = "10.2.1.146";
var GAME_PORT = 8999;

/*
var dbConnected = false;
var connection = mysql.createConnection(db_config);
connection.connect(function(err) {
	if(err) {
		console.error('error when connecting to db:', err);
	}
	else{
		console.log('connect to db success');
		dbConnected = true;
	}
});

connection.on('error', function(err) {
	console.error('db error', err);
	if(err.code === 'PROTOCOL_CONNECTION_LOST') {
		  dbConnected = false;
	}
});
*/
var resDict = {};
var index = 1;

var gameServer = net.createServer(function(sock) {
	var date = new Date();
	console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
		+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
		' CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);

	sock.on('error', function(data) {
		//console.log("got socket error");
	});
	sock.on('close', function(data) {
		if(sock.remoteAddress ==  GAME_HOST)
		{
			//server.gameServerSocket = null;
			console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + '-'
				+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
				'GLOBAL CLOSED: ' +  sock.remoteAddress + ' ' + sock.remotePort);
		}
		else
		{
			console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + '-'
				+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
				' NOT GLOBAL CLOSED: ' +  sock.remoteAddress + ' ' + sock.remotePort);
		}
	});

	if(sock.remoteAddress == GAME_HOST)
	{
		server.gameServerSocket = sock;
		server.gameServerSocket.on('disconnect', function(){
			console.log('receive disconnect event');
		});
		server.gameServerSocket.on('error', function(exc)
		{
			console.log("barrack got exception: " + exc);
		});
		console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
			+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
			' GLOBAL CONNECTED: ' +  sock.remoteAddress + ':' + sock.remotePort);

		server.gameServerSocket.on('data', function(data) {
			try
			{
				console.log(data);
				var str = data.toString();
				var obj = JSON.parse(data.toString());
				var resIndex = obj.resIndex;
				if(resIndex == undefined)
				{
					console.error("can not find resIndex");
					return;
				}
				if( resDict[resIndex] == undefined)
				{
					console.error("can not find res by index " + resIndex);
					return;
				}
				var res = resDict[obj.resIndex];
				delete  resDict[resIndex];
				delete obj["resIndex"];
				res.end(JSON.stringify(obj));
			}
			catch(e)
			{
				console.error(e.message);
			}

		});
	}
	else
	{
		console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
			+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
			'NOT GLOBAL CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);
	}

});
gameServer.listen(GAME_PORT, HOST);
gameServer.on('error', function(err) {console.log("game server got err " + err) });


var recordIndex = function (res) {
	resDict[index] = res;
	index ++;
	if(index >10000)
	{
		index = 1;
	}
};

var msgList =
{
	'CHARACTER_LIST' :1,
	'CAN_RECEIVE_REWARD' :2,
	'RECEIVE_REWARD':3
};
// packet 发送给游戏server的包
// msg_id 协议号
// res http返回，待游戏进程返回结果后转发
var sendPacketToGameServer = function (packet,msg_id, res) {
	if(server.gameServerSocket != null)
	{
		// 转发给game server
		packet["resIndex"] = index;
		console.log("res index = " + index);
		recordIndex(res);
		var str = JSON.stringify(packet);
		var len = str.length;
		var buf = new Buffer(len + 6, 0);
		buf.writeInt16LE(len, 0);
		buf.writeInt32LE(msg_id, 2);
		buf.write(str, 6);
		try
		{
			server.gameServerSocket.write(buf, "utf8", function () {
				//console.log('send success....');
			});
		}
		catch(e)
		{
			console.log(Date.now() + " " + e.message);
		}
	}
	else
	{
		var response = '{ "errcode": 4002, "errmsg": "服务器未开启" }';
		res.end(response);
	}
};

var routeRequest = function(url,post, res)
{
	switch (url.pathname)
	{
		case "/Receive/Roles":
			// TODO md5
			sendPacketToGameServer(post, msgList.CHARACTER_LIST,res);
			break;
		case "/Receive/Conditions":
			sendPacketToGameServer(post, msgList.CAN_RECEIVE_REWARD,res);
			break;
		case "/Receive/Awards":
			break;
		default :
			console.warn("got invalid url path: " + url.pathname);
			res.end('{"errcode":4001,"errmsg":"系统错误"}');
			break;
	}
};
var notice = function(req, res){
    var post ='';
	var reqUrl = url.parse(req.url, true);
    req.addListener('data', function(chunk){
        post += chunk;
    });
    req.addListener('end', function(){
		var date = new Date();
		console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
			+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds()  + " " +　post+"\n");
		//var str = querystring.parse(post);
		if(post == null || post.length == 0)
		{
			return;
		}
		try
		{
			post = JSON.parse(post);
			routeRequest(reqUrl, post, res);
		}
		catch(e)
		{
		}
    });
};

// Uncaught exception handler
process.on('uncaughtException', function(err) {
	console.error(' Caught exception: ' + err.stack);
});

var server = http.createServer(notice);
server.listen(80);
server.on('error', function(err) {console.log("server got err " + err) });
