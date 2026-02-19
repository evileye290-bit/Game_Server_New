var http = require('http');
var querystring = require('querystring');
var mysql = require('mysql');
var db_config = {
	host: '10.164.0.21',
	user: 'mcwygame',
	password: 'cfq6DrjJrtC7KoPK',
	database: 'accountdb',
	multipleStatements: false
};
var appId = 1128715458;
var dbConnected = false;

var connection = mysql.createConnection(db_config);

var reg=/^[0-9a-zA-Z\-]+$/;
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

var notice = function(req, res){
    var post ='';
    req.addListener('data', function(chunk){
        post += chunk;
    });
    req.addListener('end', function(){
		var date = new Date();
		console.log(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
			+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds()  + " " +　post+"\n");
		post = querystring.parse(post);
		if(post.idfa == null){
			return;
		}
		if(reg.test(post.idfa) == false){
			console.error("got invalid idfa " + post.idfa);
			return;
		}
		if(post.receive == 1){
			// 领取任务
			if(dbConnected == false)
			{
				res.end("{msg:false}");
				return;
			}
			try
			{
				var sql = 'INSERT INTO  `idfa_quest` VALUES (' + "'"+post.idfa.toString()+"'" + ',0) ' +
					' ON DUPLICATE KEY UPDATE receive= 0;';
				connection.query(
					sql,
					function selectCb(err, results, fields) {
						if (err) {
							console.error(err);
							res.end("{msg:false}");
						}
						else{
							res.end("{msg:true}")
						}
					}
				);
			}
			catch(e)
			{
				console.error(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
					+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() + " " + "req got exception " + e.message);
			}
		}
		else if(post.receive == 2){
			if(dbConnected == false){
				return;
			}
			try
			{
				var sql = 'INSERT INTO  `idfa_quest` VALUES (' +"'"+ post.idfa.toString()+"',1" + ') ON DUPLICATE KEY' +
					' UPDATE receive=1';
				connection.query(
					sql,
					function selectCb(err, results, fields) {
						if (err) {
							console.error(err);
						}
					}
				);
			}
			catch(e)
			{
				console.error(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
					+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() + " " + "req got exception " + e.message);
			}
		}
		else{
			// 查询任务
			if(dbConnected == false)
			{
				// 与db断开连接 默认已领取任务 防止刷单
				var response = {};
				response[post.idfa] = 1;
				res.end(JSON.stringify(response));
				return;
			}

			try
			{
				var sql = 'SELECT `idfa` FROM `idfa_quest` WHERE idfa=' + "'"+post.idfa.toString()+"'" + " AND receive=1;"
				connection.query(
					sql,
					function selectCb(err, results, fields) {
						var response = {};
						if (err) {
							console.error(err);
						}
						if(results && results.length > 0)
						{
							response[post.idfa] = 1;
							res.end(JSON.stringify(response));
						}
						else{
							// 没有记录
							response[post.idfa] = 0;
							res.end(JSON.stringify(response));
						}
					}
				);
			}
			catch(e)
			{
				console.error(date.getFullYear() + '-' + (date.getMonth()+1) + '-' + date.getDate() + ' '
					+ date.getHours() + ':' + date.getMinutes() +  ':' + date.getSeconds() + " " + "req got exception " + e.message);
			}
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
