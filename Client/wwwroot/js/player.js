var _sound = null; //the current song/sound
var _interval = null;
var _vol = 0.3;

window.onload = (event) => {
    console.log("page is fully loaded");

    window.addEventListener('message', (event) => {
        switch (event.data.type) {

            case "play_file":
                if (event.data.file) {                    
                    playFile(event.data.file, event.data.pos);
                }
                break;
            case "vol":
                if (event.data.vol) {
                    _vol = event.data.vol;
                    _sound.volume(_vol);
                }                
                break;
            case "pause":
            case "stop":
                if (_sound) {
                    _sound.stop();
                }
                break;
            case "seek":
                console.log("seek:" + event.data.time / 1000)
                _sound.seek(Math.round(event.data.time / 1000));
                break;
            default:
                break;
        }
    });
};

///plays file and handles updates in seconds so the client has the current song position
///when sound stops the interval is removed
function playFile(file, pos) {

    Howler.stop();
    console.log("play_file:" + file);
    _sound = new Howl({
        src: [file],
        // format: ['webm'],
        html5: true,
        volume: 0.3,
        html5PoolSize: 0,
        preload: true,
        onend: function () {
            console.log("song finished TODO post back ended");
        },
        onload: function () {
            console.log("song loaded: " + _sound);
            _sound._volume(_vol);
            postBackLoaded();
        },
        onplay: function () {

            console.log("playing song: " + file);

            if (_interval)
                clearInterval(_interval)

            _interval = setInterval(function () {

                // Check if the sound has finished playing
                if (_sound.playing() === false) {
                    clearInterval(_interval); // Stop the interval when the sound is finished
                }
                else {
                    //post back to client position changes
                    postBackPositon();
                }

            }, 1000);
        },
        onplayerror: function (id, code) {
            console.log("song error: " + code);
        }
    });


    if (pos > 0) {
        console.log("JS seeking to..." + pos / 1000);
        _sound.seek(Math.round(pos / 1000));
    }
    _sound.play();
}

function postBackLoaded() {
    if (!_sound) return;

    var duration = Math.round(_sound.duration());

    // browser-side JS
    fetch(`https://custom_radios_fivem/setSongLoaded`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=UTF-8',
        },
        body: JSON.stringify({
            duration: duration
        })
    }).then(resp => resp.json()).then(resp => console.log(resp));
}

function postBackPositon() {

    if (!_sound) return;

    currentPosition = Math.round(_sound.seek()) // Get the current playback position

    // browser-side JS
    fetch(`https://custom_radios_fivem/setCurPos`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=UTF-8',
        },
        body: JSON.stringify({
            pos: currentPosition
        })
    }).then(resp => resp.json());
}
// No arg
//onNet("imReady", () => {
//    console.log("I'm ready to go!");
//});

// browser side

