var http = require('http');
var qs = require('querystring');
var oauth_host = "pay.51wanxin.cn";
//var oauth_host = "oauth.anysdk.com";
var oauth_path = "/api/User/LoginOauth/";
var net = require('net');
var resJson = null;

var PORT = 9000;
var HOST = '10.2.1.143';
var CLIENT_PORT = 8888;
var BARRACK_HOST = '192.168.10.110';
var args = process.argv.slice(2);
if(args.length >= 4) {
    HOST = args[0];
    PORT = args[1];
    CLIENT_PORT = args[2];
    BARRACK_HOST = args[3];
}
console.info("listen ip " + HOST + " port " + PORT + " client port " + CLIENT_PORT);

var gameServer = net.createServer(function(sock) {
    var date = new Date();
    console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
        + date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
        ' CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);
    sock.on('data', function(data) {
    });
    sock.on('error', function(data) {
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
        console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
            + date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
            ' BARRACK CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);
        server.gameServerSocket.on('error', function(exc)
        {
            console.log("barrack got exception: " + exc);
        });
    }
    else
    {
        console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
            + date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() +
            ' NOT BARRACK CONNECTED: ' +        sock.remoteAddress + ':' + sock.remotePort);
    }

});
gameServer.listen(PORT, HOST);
gameServer.on('error', function(err) {console.log("game server got err " + err) });

var checkLogin = function (postData, callback) {
    var options = {
        host: oauth_host,
        path: oauth_path,
        method: "post",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
            "Content-Length": postData.length,
            //	     "User-Agent":"Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; BOIE9;ZHCN)"
        }
    };

    //console.log("#post url:\n" + oauth_host + oauth_path)
    //console.log("#post data:\n" + postData)
    var reqToAnysdk = require("http").request(options, function (resFromAnysdk) {
        resFromAnysdk.setEncoding("utf8");
        resFromAnysdk.on("data", function (data) {
            // console.log("#return data:\n" + data);
            resJson = JSON.parse(data);
            if (resJson && (resJson.status == "ok")) {
                //resJson.ext = "登陆验证成功";
                var token = Math.floor(Math.random() * 10000);
                resJson.ext = token.toString();
                callback(JSON.stringify(resJson));
                if(server.gameServerSocket != null)
                {
                    var packet = {
                        'id':resJson.common.uid,
                        'channel': resJson.common.user_sdk,
                        'token':token
                    };
                    // 转发给game server
                    var str = JSON.stringify(packet);
                    var len = str.length;
                    var buf = new Buffer(len + 6, 0);
                    buf.writeInt16LE(len, 0);
                    buf.writeInt32LE(1, 2);
                    buf.write(str, 6);
                    try
                    {
                        server.gameServerSocket.write(buf, "utf8", function () {
                            //console.log('send success....')
                        });
                    }
                    catch(e)
                    {
                        console.log(Date.now() + " " + e.message);
                    }

                }
            } else {
                console.log("error code " + resJson.data.error_no + " error " + resJson.data.error);
                callback(JSON.stringify(resJson));
            }
        });
        resFromAnysdk.on("error", function (msg) {
            console.log("requestFromAnysdk got error " + msg);
        });

    });

    reqToAnysdk.write(postData);
    reqToAnysdk.end();
};

var login = function (req, res) {
    var info ='';
    req.addListener('data', function(chunk){
        info += chunk;
    });

    req.addListener('end', function(){
        checkLogin(info, function (msg) {
            res.write(msg);
            res.end();
            res.on("error", function () {
            })
        });
    });
    req.addListener('error', function(){
        console.log("request got error");
    });
};
var server = http.createServer(login);
server.listen(CLIENT_PORT);
server.on('error', function(err) {console.log("server got err " + err) });
// Uncaught exception handler
process.on('uncaughtException', function(err) {
    console.error(' Caught exception: ' + err.stack);
});