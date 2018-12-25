<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>WebSocket Chat</title>
    <script type="text/javascript" src="Scripts/jquery-3.3.1.js"></script>
    <script type="text/javascript" src="Scripts/WebSocket.js?v1.10"></script>
    <link rel="stylesheet" type="text/css" href="content/site.css?v=1.10" media="screen" />
    <%--<meta name=viewport content='initial-scale=1.1' />--%>
    <meta name="viewport" content="width=device-width">
</head>
<body>


    <div class="content">
        <div class="msgContent">
            <div id="LoginPanel">
                <span id="Login">Login</span>
                <select name='user' id='user'></select>
                <span id="debug">????</span>
            </div>
            <div id="splitter">
                <div id='messageList'>
                </div>
                <div id='userState'></div>
            </div>
            <div id="SendPanel">
                <input type="text" id="textInput" value="Hi" />
                <input type="button" value="Send" id="btnSend" /><br />
            </div>
        </div>
        <div id="status">Not Connected</div>

    </div>



</body>
</html>
