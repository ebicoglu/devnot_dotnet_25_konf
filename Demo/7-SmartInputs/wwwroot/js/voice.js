$(document).ready(function () {

    function showServerResponse(obj) {
        document.getElementById("serverResponse").textContent = obj;
    }

    function logMp3Library(fmt, args) {
        /* disabled
        var text = this.sprintf.apply(this, arguments);
        var element = document.querySelector('#log');
        element.innerHTML = text + "<br>\n" + element.innerHTML;
        */
    }

    function PrintAudioMetadata(fmt, args) {
        var text = this.sprintf.apply(this, arguments);
        var element = document.getElementById('status');
        element.innerHTML = text;
    }

    function loadScript(name, path, cb) {
        var node = document.createElement('SCRIPT');
        node.type = 'text/javascript';
        node.src = path;
        var head = document.getElementsByTagName('HEAD');
        if (head[0] != null)
            head[0].appendChild(node);
        if (cb != null) {
            node.onreadystagechange = cb;
            node.onload = cb;
        }
    }

    var gAudio = null;       //Audio context
    var gAudioSrc = null;    //Audio source
    var gNode = null;        //The audio processor node
    var gIsLame = true;     //Has lame.min.js been loaded?
    var gLame = null;        //The LAME encoder library
    var gEncoder = null;     //The MP3 encoder object
    var gStrmMp3 = [];       //Collection of MP3 buffers
    var gIsRecording = false;
    var gCfg = {
        chnlCt: 1,
        bufSz: 1024, //default: 4096,
        sampleRate: 48000, //default: 44100,
        bitRate: 128
    };
    var gPcmCt = 0;
    var gMp3Ct = 0;

    function EnableDisable() {
        if (!gAudio) {
            Initialize();
        } else {
            Dispose();
        }
    }

    function Initialize() {
        logMp3Library("Initializing...");
        var caps = { audio: true };

        // Clean up previous instances if any
        if (gAudio) {
            gAudio.close();
            gAudio = null;
        }
        gAudioSrc = null;

        try {
            // Standardize AudioContext
            window.AudioContext = window.AudioContext || window.webkitAudioContext;

            // Create a new AudioContext
            gAudio = new AudioContext();

            // Always request a fresh media stream
            navigator.mediaDevices.getUserMedia(caps).then(onUserMedia).catch(onFail);

        } catch (ex) {
            logMp3Library("ERR: Unable to initialize audio context.");
            gAudio = null;
        }

        function onFail(ex) {
            logMp3Library("ERR: getUserMedia failed: %s", ex);
        }
    } 

    function onUserMedia(stream) {
        if (!(gAudioSrc = gAudio.createMediaStreamSource(stream))) {
            logMp3Library("ERR: Unable to create audio source.");
        } else if (!gIsLame) {
            logMp3Library("Fetching lame library...");
            loadScript("lame", "~/js/lame.min.js", LameCreate);
        } else {
            LameCreate();
        }
    }

    function LameCreate() {
        gIsLame = true;
        if (!(gEncoder = Mp3Create())) {
            logMp3Library("ERR: Unable to create MP3 encoder.");
        } else {
            gStrmMp3 = [];
            gPcmCt = 0;
            gMp3Ct = 0;
            logMp3Library("Initialized");
        }
    }

    function Dispose() {
        logMp3Library("Disposing...");
        if (gIsRecording) {
            logMp3Library("ERR: PowerOff: You need to stop recording first.");
        } else {
            gEncoder = null;
            gLame = null;
            gNode = null;
            gAudioSrc = null;
            gAudio = null;
            log("Disposed");
        }
    }

    function StartRecord() {
        var creator;

        if (!gAudio) {
            logMp3Library("ERR: No Audio source.");
        } else if (!gEncoder) {
            logMp3Library("ERR: No encoder.");
        } else if (gIsRecording) {
            logMp3Library("ERR: Already recording.");
        } else {

            if (gAudio.state === 'suspended') {
                gAudio.resume();
            }

            if (!gNode) {
                if (!(creator = gAudioSrc.context.createScriptProcessor || gAudioSrc.createJavaScriptNode)) {
                    logMp3Library("ERR: No processor creator?");
                } else if (!(gNode = creator.call(gAudioSrc.context, gCfg.bufSz, gCfg.chnlCt, gCfg.chnlCt))) {
                    logMp3Library("ERR: Unable to create processor node.");
                }
            }
            if (!gNode) {
                logMp3Library("ERR: onRecord: No processor node.");
            } else {
                gNode.onaudioprocess = onAudioProcess;
                gAudioSrc.connect(gNode);
                gNode.connect(gAudioSrc.context.destination);
                gIsRecording = true;
                document.getElementsByClassName('download')[0].style.display = "block";
                logMp3Library("Recording...");
            }
        }
    }

    function StopRecord() {

        if (!gAudio) {
            logMp3Library("ERR: onStop: No audio.");
        } else if (!gAudioSrc) {
            logMp3Library("ERR: onStop: No audio source.");
        } else if (!gIsRecording) {
            logMp3Library("ERR: onStop: Not recording.");
        } else {
            gAudioSrc.disconnect(gNode);
            gNode.disconnect();
            gIsRecording = false;

            var mp3 = gEncoder.flush();
            if (mp3.length > 0) {
                gStrmMp3.push(mp3);
            }

            //Consolidate the collection of MP3 buffers into a single data Blob.
            var blob = new Blob(gStrmMp3, { type: 'audio/mp3' });

            DisplayAudio(blob);

            SendAudioToBackend(blob);

            logMp3Library("Recording stopped");
        }
    }

    function onAudioProcess(e) {
        var inBuf = e.inputBuffer;
        var samples = inBuf.getChannelData(0); // 1. Get audio samples from the first channel (mono).
        var sampleCt = samples.length;
        var samples16 = convertFloatToInt16(samples); // 2. Convert from 32-bit float to 16-bit integer PCM.
        if (samples16.length > 0) {
            gPcmCt += samples16.length * 2; // 3. Track total size of raw PCM data in bytes.
            var mp3buf = gEncoder.encodeBuffer(samples16); // 4. Encode PCM to MP3 using the LAME encoder.
            var mp3Ct = mp3buf.length;
            if (mp3Ct > 0) {
                gStrmMp3.push(mp3buf);  // 5. Store the MP3 chunk for later blob assembly.
                gMp3Ct += mp3Ct; // 6. Track total size of MP3 data.
            }

            PrintAudioMetadata("Raw: %dB, MP3: %dB, Compression: %2.2f%%", gPcmCt, gMp3Ct, (gMp3Ct * 100) / gPcmCt);
        }
    }

    function Mp3Create() {
        if (!(gLame = new lamejs())) {
            logMp3Library("ERR: Unable to create LAME object.");
        } else if (!(gEncoder = new gLame.Mp3Encoder(gCfg.chnlCt, gCfg.sampleRate, gCfg.bitRate))) {
            logMp3Library("ERR: Unable to create MP3 encoder.");
        } else {
            logMp3Library("MP3 plugin is ready");
        }
        return (gEncoder);
    }

    function convertFloatToInt16(inFloat) {
        var sampleCt = inFloat.length;
        var outInt16 = new Int16Array(sampleCt);
        for (var n1 = 0; n1 < sampleCt; n1++) {
            //This is where I can apply waveform modifiers.
            var sample16 = 0x8000 * inFloat[n1];
            sample16 = (sample16 < -32767) ? -32767 : (sample16 > 32767) ? 32767 : sample16;
            outInt16[n1] = sample16;
        }
        return (outInt16);
    }

    function DisplayAudio(blob) {
        //Create a URL to the blob.
        var url = window.URL.createObjectURL(blob);
        var audio = document.getElementById('audio');
        var download = document.getElementById('DownloadLink');
        audio.src = url;
        download.href = url;
    }

    function showFormLoading() {
        document.getElementById('loadingOverlay').style.display = "block";
    }

    function hideFormLoading() {
        document.getElementById('loadingOverlay').style.display = "none";
    }

    function SendAudioToBackend(audioBlob) {
        showFormLoading();
        const formData = new FormData();
        formData.append('file', audioBlob, 'file.mp3');

        fetch('/Voice/ProcessAudio', {
            method: 'POST',
            body: formData
        }).then(response => response.json()).then(data => {
            if (data.success) {              
                showServerResponse(JSON.stringify(data.car, null, 2))
                // Update the form with the received data
                Object.keys(data.car).forEach(key => {
                    $("#" + key).val(data.car[key]);
                });
            }
            else {
                showServerResponse(data.error);
            }

            hideFormLoading();
        }).catch(error => {
            showServerResponse(error.message);
            hideFormLoading();
        }) 
    }

    function ResetAudio() {
        logMp3Library("Resetting audio...");

        // Stop recording if still active
        if (gIsRecording) {
            StopRecord();
        }

        // Clear encoder, buffers, audio nodes
        gEncoder = null;
        gLame = null;
        gStrmMp3 = [];
        gPcmCt = 0;
        gMp3Ct = 0;

        if (gNode) {
            try {
                gAudioSrc.disconnect(gNode);
                gNode.disconnect();
            } catch (e) { }
            gNode = null;
        }

        if (gAudioSrc) {
            try {
                gAudioSrc.disconnect();
            } catch (e) { }
            gAudioSrc = null;
        }

        if (gAudio) {
            try {
                gAudio.close();
            } catch (e) { }
            gAudio = null;
        }

        // Reinitialize audio
        Initialize();
    }


    $('#btnVoiceInput').click(function () {
        if (gIsRecording) {
            $('#btnVoiceInput').text('Listen ⏺');
            StopRecord();
            ResetAudio();
        }
        else {
            $('#btnVoiceInput').text('Stop ⛔');
            StartRecord();
        }
    });

    (function (window) {
        setTimeout(Initialize, 1);
    })(window);

});
