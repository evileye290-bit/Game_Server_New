/**
 nodejs 接收anysdk支付通知demo
 注意：根据自己游戏及代码要求进行代码优化调整
 */

var http = require('http');
var querystring = require('querystring');
var util = require('util');
var crypto = require('crypto');
var net = require('net');

//此IP为第三方服务器所在IP 勿动！！！
var ANYSDK_HOST = "123.59.116.59";


var resJson = null;
var PORT = 9010;
var HOST = '10.2.1.146';
var BARRACK_HOST = '10.2.1.146';

var args = process.argv.slice(2);
if(args.length >= 4) {
	HOST = args[0];
	PORT = args[1];
	CLIENT_PORT = args[2];
	BARRACK_HOST = args[3];
}

var gameServer = net.createServer(function(sock) {
	var date = new Date();
	console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
		+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
		' CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);
	sock.on('data', function(data) {
	});
	sock.on('error', function(data) {
		//console.log("got socket error");
	});
	sock.on('close', function(data) {
		if(sock.remoteAddress == BARRACK_HOST)
		{
			//server.gameServerSocket = null;
			console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + '-'
				+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
				' BARRACK CLOSED: ' +  sock.remoteAddress + ' ' + sock.remotePort);
		}
		else
		{
			console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + '-'
				+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
				' NOT BARRACK CLOSED: ' +  sock.remoteAddress + ' ' + sock.remotePort);
		}
	});
	if(sock.remoteAddress == BARRACK_HOST)
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
			' BARRACK CONNECTED: ' +  sock.remoteAddress + ':' + sock.remotePort);
	}
	else
	{
		console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
			+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
			'NOT BARRACK CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);
	}

});
gameServer.listen(PORT, HOST);
gameServer.on('error', function(err) {console.log("game server got err " + err) });

//anysdk privatekey
var private_key = '385A4B04B026049B386B022C30B7B2EF';
//anysdk 增强密钥
var enhanced_key = 'ZjZjMGU1MjI3NTkwNWRlZTljMjA';

//md5
var my_md5 = function(data){
	//中文字符处理
	data = new Buffer(data).toString("binary");
	return crypto.createHash('md5').update(data).digest('hex').toLowerCase();
};

//通用验签
var check_sign = function(post,private_key){
	var source_sign = post.sign;
	delete post.sign;
	var new_sign = get_sign(post,private_key);

	if(source_sign == new_sign){
		return true;
	}
	return false;
};

//增强验签
var check_enhanced_sign = function(post,enhanced_key){
	var source_enhanced_sign = post.enhanced_sign;
	delete post.enhanced_sign;
	delete post.sign;
	var new_sign = get_sign(post,enhanced_key);

	if(source_enhanced_sign == new_sign){
		return true;
	}
	return false;
};

//获取签名
var get_sign = function(post,sign_key){
	var keys = [];

	for(key in post){
		//console.log("Key:"+key+"\tVaule:"+post[key]);
		keys.push(key);

	}
	keys = keys.sort();
	var paramString = '';
	for(i in keys){
		paramString += post[keys[i]];
	}
	//console.log("拼接的字符串:"+paramString);
	//console.log("第一次md5:"+my_md5(paramString));
	//console.log("加入密钥:"+my_md5(paramString)+sign_key);
	//console.log("第二次md5:"+my_md5(my_md5(paramString)+sign_key));

	return  my_md5(my_md5(paramString)+sign_key);
};

//接收支付通知
var notice = function(req, res){
	var post ='';
	req.addListener('data', function(chunk){
		post += chunk;
	});
	req.addListener('end', function(){
		var date = new Date();
		//console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
		//	+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds()  + " " +　post+"\n");
		post = querystring.parse(post);
		if(res.socket.remoteAddress.indexOf(ANYSDK_HOST) < 0){
			try {
				console.warn("got invalid pay notice " + res.socket.remoteAddress);
				res.end('ok');
				return;
			}
			catch(e){
				console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
					+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() + " " + "req got exception " + e.message);
			}
			return;
		}
		if(check_enhanced_sign(post,enhanced_key)){
			//if(check_enhanced_sign(post,enhanced_key)){
			//异步处理游戏支付发放道具逻辑
			try
			{
				res.end('ok');
			}
			catch(e)
			{
				console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
					+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() + " " + "req got exception " + e.message);
			}

			if(server.gameServerSocket != null)
			{
				var packet = {
					'orderId':post.order_id,
					'amount':post.amount,
					'channelId':post.channel_number,
					'channelUid':post.user_id,
					'serverId':post.server_id,
					'uid':post.game_user_id,
					'payTime':post.pay_time,
					'status':post.pay_status,
					'productId':post.product_id,
					'ext':post.private_data
				};
				// 转发给game server
				var str = JSON.stringify(packet);
				var len = str.length;
				var buf = new Buffer(len + 6, 0);
				buf.writeInt16LE(len, 0);
				buf.writeInt32LE(2, 2);
				buf.write(str, 6);
				try
				{
					server.gameServerSocket.write(buf, "utf8", function () {
						//console.log('send success....')
					});
				}
				catch(e)
				{
					console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
						+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() + " " + e.message);
				}
			}
		}else{
			res.end(util.inspect(post));
		}
	});
};

// Uncaught exception handler
process.on('uncaughtException', function(err) {
	console.error(' Caught exception: ' + err.stack);
});

var server = http.createServer(notice);
server.listen(8889);
server.on('error', function(err) {console.log("server got err " + err) });
