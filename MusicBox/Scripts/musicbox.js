function playmedia(name, file, index) {

    var player = $("#jquery_jplayer_1");
    player.jPlayer("destroy");
    player.jPlayer({
        ready: function () {
            $(this).jPlayer("setMedia", {
                title: name,
                mp3: "Player/StreamUploadedSongs/" + index + "?file=" + file
            }).jPlayer("play");
        },
        swfPath: "../../dist/jplayer",
        supplied: "mp3",
        wmode: "window",
        useStateClassSkin: true,
        autoBlur: false,
        smoothPlayBar: true,
        keyEnabled: true,
        remainingDuration: true,
        toggleDuration: true,
        errorAlerts: true,
        warningAlerts: true
    });

}

$(document).ready(function () {
    $('#playlistlink').click();
});

