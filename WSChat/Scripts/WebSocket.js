
$().ready(function () {

	var $user = $('#user');


	var userId = 0;
	var users = [];
	var initialised = false;

	var $userState = $('#userState')

	$("#status").text("Connecting...");

	var webSocket;


	function getUser(id) {

		var result = users.filter(function (item) { return item.id === id; });

		if (result.length > 0) {
			return result[0];
		}

		return null;

	}

	function setUserState(message) {



		var changes = [];

		users.forEach(function (user) {

			var match = message.users.filter(function (item) { return item.id === user.id; });

			if (match.length == 0) {

				changes.push({ user: user, remove: true });

			} else {

				user.isOnline = match[0].isOnline;
				user.isSelected = match[0].isSelected;

				$('#messageList').empty();

				message.history.forEach(function (message) {

					var fromUser = getUser(message.userId);

					var $messageRow = getMessageRow(fromUser, message.text);

					$messageBuble = $messageRow.find('.messageBuble');

					$messageBuble.hide();

					$('#messageList').append($messageRow);

					$messageBuble.fadeIn();

					$messageRow[0].focus();

				});

			}

		});

		message.users.forEach(function (user) {

			var match = users.filter(function (item) { return item.id === user.id; });

			if (match.length == 0) {

				changes.push({ user: user, add: true });

			}

		});

		changes.forEach(function (item) {

			if (item.add) {

				users.push(item);

				return;

			}

			if (item.remove) {

				var index = users.findIndex(function (subItem) { return subItem.id === item.user.id; });

				users.splice(index, 1);
				user.$state.remove();

				return;

			}


		});

		users.forEach(function (user) {

			if (!user.$state) {

				user.$state = $('<div class="userStateRow">' + user.name + '</div>').data('user', user);

				$userState.append(user.$state);

			}

			user.$state.text(user.name);

			if (user.isOnline) {

				user.$state.css("background-color", "lawngreen")
				user.$state.attr('data-online', 'y')

			} else {

				user.$state.css("background-color", "lightgray")
				user.$state.attr('data-online', 'n')

			}

			if (user.isSelected) {

				user.$state.css("border-color", "red")

			} else {

				user.$state.css("border-color", "transparent")

			}

		});

		if (users.length > 0) {

			$userState.fadeIn();

		}




	}

	function getMessageRow(fromUser, message) {

		var $messageRow = $('<div class="messageRow" tabindex="0"></div>')

		var $messageBuble = $('<div class="messageBuble"></div>').append(
			$('<div class="messageHeader">' + getUser(fromUser.id).name + '</div>'),
			$('<div class="messageText">' + message + '</div>')
		).appendTo($messageRow);

		if (fromUser.id === userId) {
			$messageRow.css('justify-content', 'flex-end');
			$messageBuble.addClass('messageBubleRight')
		} else {
			$messageRow.css('justify-content', 'flex-start');
			$messageBuble.addClass('messageBubleLeft')
		}

		return $messageRow;

	}

	function Init(src) {

		//alert(src);

		$("#debug").text(src);

		webSocket = new WebSocket("ws://" + window.location.hostname + "/WebSocket/WSHandler.ashx");

		webSocket.onopen = function () {

			$("#status").text("Connected (" + webSocket.readyState + ')');

			var data = {
				userId: userId,
				messageId: userId ? 'login' : 'init'
			};

			webSocket.send(JSON.stringify(data));

		};

		webSocket.onmessage = function (evt) {

			var message = JSON.parse(evt.data);

			switch (message.messageId) {
				case "init":

					if (initialised) {
						break;
					}

					var $option = $('<option>Select User</option>');
					$user.append($option);

					users = message.users;

					users.forEach(function (user) {

						var $option = $('<option value=' + user.id + ' data-online=' + (user.isOnline ? 'y' : 'n') + '>' + user.name + '</option>');
						user.$option = $option;
						$user.append($option);

					});

					$('#LoginPanel').fadeIn();

					$user.change(function () {


						userId = parseInt($user.val());

						var user = getUser(userId);

						$user.fadeOut();
						$('#Login').text(user.name)



						var data = {
							userId: userId,
							MessageId: 'login'
						};

						webSocket.send(JSON.stringify(data));

						$('#messageList').fadeIn();
						$('#SendPanel').fadeIn();



					})

					initialised = true;

					setUserState(message);


					// Select the first offline user.

					var result = users.filter(function (item) { return !item.isOnline; });

					if (result.length) {

						$user.prop('selectedIndex', result[0].$option.index());

						$user.trigger('change');

					}



				case 'send': // Receive

					var fromUser = getUser(message.fromUserId);

					if (fromUser) {

						var $messageRow = getMessageRow(fromUser, message.message);

						$messageBuble = $messageRow.find('.messageBuble');

						$messageBuble.hide();

						$('#messageList').append($messageRow);

						$messageBuble.fadeIn();

						$messageRow[0].focus();
					}

					break;

				case 'userState':

					setUserState(message);

					break;


			}


		};

		webSocket.onerror = function (evt) {

			$("#status").text(evt.message);

		};

		webSocket.onclose = function () {

			$("#status").text("Disconnected (" + webSocket.readyState + ')');

		};


	}

	$userState.click(function (e) {

		var $user = $(e.target);
		var user = $user.data('user');

		if (!user.isOnline) {
			return;
		}

		user.isSelected = !user.isSelected;


		var data = {
			userId: userId,
			selectedUserId: user.id,
			iSselected: user.isSelected,
			messageId: 'select',
			message: $("#textInput").val()
		};

		webSocket.send(JSON.stringify(data));

	})

	$("#btnSend").click(function () {


		var data = {
			userId: userId,
			messageId: 'send',
			message: $("#textInput").val()
		};

		webSocket.send(JSON.stringify(data));


	});


	Init('load');

	$(window).on('pageshow', function (event) {

		if (!webSocket || webSocket.readyState != WebSocket.OPEN) {

			Init('pageshow');

		}

	});


	// First we get the viewport height and we multiple it by 1% to get a value for a vh unit
	var vh = window.innerHeight * 0.01;
	// Then we set the value in the --vh custom property to the root of the document
	document.documentElement.style.setProperty('--vh', `${vh}px`);

	// We listen to the resize event
	$(window).on('resize', () => {
		// We execute the same script as before
		var vh = window.innerHeight * 0.01;
		document.documentElement.style.setProperty('--vh', `${vh}px`);
	});

});