document.addEventListener('DOMContentLoaded', () => {

    const beginLat = 41.705117;
    const beginLon = 44.808169;

    const map = L.map('map').setView([beginLat, beginLon], 17);
    const server = "http://localhost:5245";

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19
    }).addTo(map);

    let addPinMode = false;
    let moveLocMode = false;
    let markers = [];
    let userMarker = null;
    let userCircle = null;
    let currentPosition = null;
    let radius = 500;


    const unboxers = [2, 3];
    const userCircleObj = {
        color: 'green',
        fillColor: '#0afb0aff',
        fillOpacity: 0.15,
        radius: 20
    };

    const radiusSlider = document.getElementById('radiusSlider');
    const radiusValue = document.getElementById('radiusValue');
    const showAllBtn = document.getElementById('showAllBtn');

    const blueIcon = new L.Icon({
        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-blue.png',
        shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    const redIcon = new L.Icon({
        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
        shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    document.getElementById('addPinBtn').addEventListener('click', () => {
        addPinMode = true;
        moveLocMode = false;
        alert("Click on the map to place a new pin.");
    });

    document.getElementById('moveLocBtn').addEventListener('click', () => {
        moveLocMode = true;
        addPinMode = false;
        alert("Click on the map to move your location.");
    });

    let currentPins = [];

    function clearMarkers() {
        markers.forEach(m => map.removeLayer(m));
        markers = [];
        currentPins = [];
    }

    async function loadNearbyPins(lat, lon, radiusMeters) {
        try {
            const response = await fetch(`${server}/api/pin/nearby?latitude=${lat}&longitude=${lon}&radius=${radiusMeters}`);
            const pins = await response.json();

            const pinsChanged = hasPinsChanged(pins, currentPins);

            if (pinsChanged) {
                clearMarkers();

                pins.forEach(p => {
                    const unboxed = p.statistics.unboxCount ?? 0;
                    const created = new Date(p.createdat).toLocaleDateString();

                    const marker = L.marker([p.latitude ?? p.x, p.longitude ?? p.y], { icon: redIcon })
                        .addTo(map)
                        .bindPopup(`<b>${p.ped?.username ?? 'Unknown'}</b><br>Distance: ${p.distance?.toFixed(1) ?? '?'} m<br>🎲Unboxes: ${unboxed}<br>🕒Created: ${created}<br><button class="openBoxBtn" data-pinid="${p.id}">🎁 Open Box</button>`);
                    markers.push(marker);
                });

                currentPins = pins.map(p => ({
                    lat: p.latitude ?? p.x,
                    lon: p.longitude ?? p.y,
                    unboxed: p.statistics.unboxCount ?? 0,
                    id: p.id
                }));
            }
        } catch (err) {
            console.error("Failed to load nearby pins:", err);
        }
    }

    function hasPinsChanged(newPins, oldPins) {
        if (newPins.length !== oldPins.length)
            return true;

        return newPins.some(np => {
            const op = oldPins.find(o => o.id === np.id);
            if (!op) return true;

            const newUnbox = np.statistics?.unboxCount ?? 0;
            const oldUnbox = op.unboxed ?? 0;

            return newUnbox !== oldUnbox;
        });
    }

    map.on('click', async (e) => {
        const lat = e.latlng.lat;
        const lon = e.latlng.lng;

        if (addPinMode) {
            selectedCoords = { lat, lon };
            modal.style.display = 'block';
        } else if (moveLocMode) {
            currentPosition = { latitude: lat, longitude: lon };
            if (userMarker) map.removeLayer(userMarker);
            if (userCircle) map.removeLayer(userCircle);

            userMarker = L.marker([lat, lon], { icon: blueIcon })
                .addTo(map)
                .bindPopup("<b>Your New Location</b>")
                .openPopup();

            userCircle = L.circle([lat, lon], userCircleObj).addTo(map);
            moveLocMode = false;
            alert("Location moved!");
            loadNearbyPins(lat, lon, radius);
        }
    });

    map.on('popupopen', e => {
        const btn = e.popup._contentNode.querySelector('.openBoxBtn');
        if (btn) {
            btn.addEventListener('click', async () => {
                const pinId = btn.getAttribute('data-pinid');
                const founder = unboxers[Math.floor(Math.random() * unboxers.length)];

                try {
                    const response = await fetch(`${server}/api/pin/unpack?pinId=${pinId}&founder=${founder}`);
                    const gifts = await response.json();
                    showUnpackModal(gifts);
                } catch (err) {
                    console.error("Failed to unpack pin:", err);
                    alert("Failed to unpack the box.");
                }
            });
        }
    });


    radiusSlider.addEventListener('input', e => {
        radius = parseInt(e.target.value);
        radiusValue.textContent = radius;
        if (currentPosition)
            loadNearbyPins(currentPosition.latitude, currentPosition.longitude, radius);
    });

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(position => {
            console.log(position);
            // const { latitude, longitude } = position.coords;
            // currentPosition = position.coords;
            const latitude = beginLat;
            const longitude = beginLon;
            currentPosition = {
                latitude, longitude
            }
            map.setView([latitude, longitude], 17);

            userMarker = L.marker([latitude, longitude], { icon: blueIcon })
                .addTo(map)
                .bindPopup("<b>U are here</b>")
                .openPopup();

            userCircle = L.circle([latitude, longitude], userCircleObj).addTo(map);

            // loadNearbyPins(latitude, longitude, radius);

            setInterval(async () => {
                console.log(`querying whispers from ${currentPosition.latitude}:${currentPosition.longitude}`)
                await loadNearbyPins(currentPosition.latitude, currentPosition.longitude, radius)
            }, 1000);

        }, err => {
            console.error("Geolocation error:", err);
            loadNearbyPins(beginLat, beginLon, radius);
        });
    }

    const controls = document.getElementById('controls');
    let offsetX, offsetY, isDragging = false;

    controls.addEventListener('mousedown', e => {
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'BUTTON')
            return;
        isDragging = true;
        offsetX = e.clientX - controls.offsetLeft;
        offsetY = e.clientY - controls.offsetTop;
    });

    document.addEventListener('mousemove', e => {
        if (isDragging) {
            controls.style.left = `${e.clientX - offsetX}px`;
            controls.style.top = `${e.clientY - offsetY}px`;
        }
    });

    document.addEventListener('mouseup', () => {
        isDragging = false;
    });

    showAllBtn.addEventListener('click', () => {
        if (!currentPosition) return alert("Location not determined yet.");
        const hugeRadius = 10000;
        loadNearbyPins(currentPosition.latitude, currentPosition.longitude, hugeRadius);
    });


    const modal = document.getElementById('whisperModal');
    const modalTextInput = document.getElementById('whisperText');
    const recordBtn = document.getElementById('recordBtn');
    const sendBtn = document.getElementById('sendBtn');
    const cancelBtn = document.getElementById('cancelBtn');
    const modeToggle = document.getElementById('modeToggle');
    const recordingIndicator = document.getElementById('recordingIndicator');

    let selectedCoords = null;
    let audioChunks = [];
    let mediaRecorder = null;
    let isRecording = false;
    let whisperMode = "text"; // 'text' or 'audio'

    modeToggle.addEventListener('change', e => {
        whisperMode = e.target.value;
        modalTextInput.style.display = whisperMode === 'text' ? 'block' : 'none';
        recordBtn.style.display = whisperMode === 'audio' ? 'inline-block' : 'none';
    });

    recordBtn.addEventListener('click', async () => {
        if (!isRecording) {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            mediaRecorder = new MediaRecorder(stream);
            audioChunks = [];
            mediaRecorder.ondataavailable = e => audioChunks.push(e.data);
            mediaRecorder.onstop = () => stream.getTracks().forEach(t => t.stop());
            mediaRecorder.start();
            isRecording = true;
            recordBtn.textContent = "Stop Recording";
            recordingIndicator.textContent = "● Recording...";
        } else {
            mediaRecorder.stop();
            isRecording = false;
            recordBtn.textContent = "Record";
            recordingIndicator.textContent = "";
        }
    });

    cancelBtn.addEventListener('click', () => {
        modal.style.display = 'none';
        addPinMode = false;
    });

    sendBtn.addEventListener('click', async () => {
        if (!selectedCoords) return;
        let payload = {
            pedestrianId: 1,
            latitude: selectedCoords.lat,
            longitude: selectedCoords.lon
        };

        if (whisperMode === 'text') {
            const text = modalTextInput.value.trim();
            if (!text) return alert("Enter text first.");
            payload.textBox = { text };
        } else if (whisperMode === 'audio') {
            if (!audioChunks.length) return alert("No audio recorded.");
            const blob = new Blob(audioChunks, { type: 'audio/webm' });
            const base64Audio = await blobToBase64(blob);
            payload.audioBox = { audio: base64Audio };
        }

        await fetch(`${server}/api/pin`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        modal.style.display = 'none';
        addPinMode = false;

        L.marker([selectedCoords.lat, selectedCoords.lon], { icon: redIcon })
            .addTo(map)
            .bindPopup(`<b>New Whisper</b>`);

        alert("Whisper added!");
        if (currentPosition)
            loadNearbyPins(currentPosition.latitude, currentPosition.longitude, radius);
    });

    function blobToBase64(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                const result = reader.result;
                if (typeof result === 'string') {
                    const base64 = result.split(',')[1] || '';
                    resolve(base64);
                } else {
                    resolve('');
                }
            };
            reader.onerror = reject;
            reader.readAsDataURL(blob);
        });
    }

    const unpackModal = document.getElementById('unpackModal');
    const unpackTitle = document.getElementById('unpackTitle');
    const unpackContent = document.getElementById('unpackContent');
    const closeUnpackBtn = document.getElementById('closeUnpackBtn');

    closeUnpackBtn.addEventListener('click', () => {
        unpackModal.style.display = 'none';
        unpackContent.innerHTML = '';
    });

    function showUnpackModal(gifts) {
        clearMarkers();
        unpackContent.innerHTML = '';

        if (gifts.TextBox) {
            unpackTitle.textContent = "📜 Text Whisper";
            const p = document.createElement('p');
            p.textContent = gifts.TextBox;
            unpackContent.appendChild(p);
        } else if (gifts.AudioBox) {
            unpackTitle.textContent = "🔊 Audio Whisper";
            const audio = document.createElement('audio');
            audio.controls = true;
            audio.src = `data:audio/webm;base64,${gifts.AudioBox}`;
            unpackContent.appendChild(audio);
        } else {
            unpackTitle.textContent = "Empty Box";
            unpackContent.textContent = "No content found inside.";
        }

        unpackModal.style.display = 'block';
    }

});
