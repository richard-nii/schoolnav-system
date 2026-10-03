// Thin wrapper around Leaflet.js, called from Blazor via JS interop.
window.schoolNavMap = (function () {
    let map = null;
    let markersLayer = null;
    let routeLine = null;
    let dotNetRef = null; // used for click-to-place-node callback (admin mode)
    let markersById = {};

    function init(elementId, centerLat, centerLng, zoom) {
        map = L.map(elementId).setView([centerLat, centerLng], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        markersLayer = L.layerGroup().addTo(map);
    }

    function clearMarkers() {
        if (markersLayer) markersLayer.clearLayers();
        markersById = {};
    }

    function addLocationMarker(id, lat, lng, name, category) {
        const marker = L.marker([lat, lng]).bindPopup(`<b>${name}</b><br/>${category}`);
        markersLayer.addLayer(marker);
        markersById[id] = marker;
    }

    // Centers on a specific location's marker and opens its popup,
    // so clicking a sidebar item visibly pinpoints the exact spot.
    function focusLocation(id, lat, lng) {
        map.setView([lat, lng], 18);
        const marker = markersById[id];
        if (marker) {
            marker.openPopup();
        }
    }

    function drawRoute(pointsJson) {
        const points = JSON.parse(pointsJson);
        clearRoute();

        const latLngs = points.map(p => [p.latitude, p.longitude]);
        routeLine = L.polyline(latLngs, { color: '#2563eb', weight: 5, opacity: 0.85 }).addTo(map);
        map.fitBounds(routeLine.getBounds(), { padding: [40, 40] });
    }

    function clearRoute() {
        if (routeLine) {
            map.removeLayer(routeLine);
            routeLine = null;
        }
    }

    function centerOn(lat, lng, zoom) {
        map.setView([lat, lng], zoom || map.getZoom());
    }

    let graphLayer = null;

    // Draws the walkway graph for the admin view: small circles for nodes,
    // thin lines for edges, so it's clear where the graph currently goes.
    function drawGraph(nodesJson, edgesJson) {
        if (graphLayer) map.removeLayer(graphLayer);
        graphLayer = L.layerGroup().addTo(map);

        const nodes = JSON.parse(nodesJson);
        const edges = JSON.parse(edgesJson);
        const nodeById = {};
        nodes.forEach(n => nodeById[n.id] = n);

        edges.forEach(e => {
            const a = nodeById[e.nodeAId];
            const b = nodeById[e.nodeBId];
            if (a && b) {
                L.polyline([[a.latitude, a.longitude], [b.latitude, b.longitude]], {
                    color: '#f59e0b', weight: 3, opacity: 0.8, dashArray: '6,4'
                }).addTo(graphLayer);
            }
        });

        nodes.forEach(n => {
            L.circleMarker([n.latitude, n.longitude], {
                radius: 6, color: '#dc2626', fillColor: '#dc2626', fillOpacity: 0.9
            }).bindTooltip(n.label || `Node ${n.id}`).addTo(graphLayer);
        });
    }

    // Admin mode: lets the admin click the map to capture coordinates
    // for placing a new Node or Location, reported back to Blazor.
    function enableClickToPlace(dotNetHelper) {
        dotNetRef = dotNetHelper;
        map.on('click', onMapClick);
    }

    function disableClickToPlace() {
        map.off('click', onMapClick);
        dotNetRef = null;
    }

    function onMapClick(e) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnMapClicked', e.latlng.lat, e.latlng.lng);
        }
    }

    function tryGetUserLocation() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject('Geolocation not supported');
                return;
            }
            navigator.geolocation.getCurrentPosition(
                pos => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
                err => reject(err.message)
            );
        });
    }

    return {
        init, clearMarkers, addLocationMarker, focusLocation, drawRoute, clearRoute,
        drawGraph, centerOn, enableClickToPlace, disableClickToPlace, tryGetUserLocation
    };
})();
