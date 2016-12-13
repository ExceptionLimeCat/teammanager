function selectAvailElm(elm) {
    if (!$(elm).hasClass('active')) {
        $p = $(elm).parent();

        $p.children('li').each(function () {
            $(this).removeClass('active');
        });

        $(elm).addClass('active');

        var playerId = $(elm).data('player-id');
        
        $('#Player_Game_PlayerId').val(playerId);
    }
}

function selectConfirmedElm(elm) {
    if (!$(elm).hasClass('active')) {
        $p = $(elm).parent();

        $p.children('li').each(function () {
            $(this).removeClass('active');
        });

        $(elm).addClass('active');

        var playerId = $(elm).data('player-game-id');

        $('#Player_Game_PlayerId').val(playerId);
    }
}

$('#confirmPlayer').on('click', function () {    
    //var playerId = $('#Player_Game_PlayerId').val();
    var playerId = $('#availablePlayers li.active').data('player-id');

    if (playerId == 0)
        return false;

    var gameId = $('#Player_Game_GameId').val();
    var url = $('#confirmUrl').val();

    $.ajax({
        url: url,
        type: 'POST',
        data: { GameId: gameId, PlayerId: playerId },
        cache: false,
        success: function (data) {
            $('#confirmedPlayers').append(data);
            $('#availablePlayers li[data-player-id="' + playerId + '"]').remove();
            $('#Player_Game_PlayerId').val("0");
        },
        error: function (err) {
            console.error(err.msg, err);
        }
    });
});

$('#unconfirmPlayer').on('click', function () {
    var playerGameId = $('#confirmedPlayers li.active').data('player-game-id');
    var playerId = $('#confirmedPlayers li.active').data('player-id');
    var gameId = $('#Player_Game_GameId').val();    
    var url = $('#unconfirmUrl').val();

    $.ajax({
        url: url,
        type: 'POST',
        data: { Player_Game_Id: playerGameId, GameId: gameId, PlayerId: playerId },
        cache: false,
        success: function (data) {
            $('#availablePlayers').append(data);
            $('#confirmedPlayers li[data-player-game-id="' + playerGameId + '"]').remove();
            $('#Player_Game_PlayerId').val("0");
        },
        error: function (err) {
            console.error(err.msg, err);
        }
    });
});