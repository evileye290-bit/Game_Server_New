var querystring = require('querystring');
var net = require('net');
var url = require('url');
var http = require('http');
var HOST  = "10.116.225.40";
var GAME_HOST = "10.116.225.40";
var GAME_PORT = 8999;

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
                //console.log(data);
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
    'info' :10001,
    'sendProps' :10002
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
        var len = Buffer.byteLength(str, 'utf8');
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

var routeRequest = function(url, data, res)
{
    switch (url.pathname)
    {
        case "/user/info":
            sendPacketToGameServer(data, msgList.info,res);
            break;
        case "/user/sendProps":
            sendPacketToGameServer(data, msgList.sendProps,res);
            break;
        default :
            console.warn("got invalid url path: " + url.pathname);
            res.end('{"ret":4001,"msg":"參數錯誤"}');
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
            + date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds()  + " " +post+"\n");
        //var str = querystring.parse(post);
        /*
        if(post == null || post.length == 0)
        {
            return;
        }
        */
        try
        {
            //post = JSON.parse(post);
            var arg = url.parse(req.url, true).query
            routeRequest(reqUrl, arg, res);
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
server.listen(8010);
server.on('error', function(err) {console.log("server got err " + err) });
