/* =============================================================
  Driver Delivery View — DriverDeliveryJS.js
  Place this file in: MyScripts/DriverDeliveryJS.js
  ============================================================= */

(function () {

    /* ── Read hidden fields ─────────────────────────────────── */
    var orderId = document.getElementById("js-order-id").value;
    var driverId = document.getElementById("js-driver-id").value;
    var orderStatus = document.getElementById("js-order-status").value;
    var deliveryAddress = document.getElementById("js-delivery-address").value;
    var token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    /* ── Map init ───────────────────────────────────────────── */
    var map = L.map("map").setView([-29.8587, 31.0218], 13);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap contributors"
    }).addTo(map);

    var routeLayer = null;
    var startMarker = null;
    var endMarker = null;
    var liveMarker = null;
    var watchId = null;

    /* ── Geocode an address ─────────────────────────────────── */
    function geocode(address) {
        var url = "https://nominatim.openstreetmap.org/search?format=json&countrycodes=za&q=" + encodeURIComponent(address);
        return fetch(url)
            .then(function (res) { return res.json(); })
            .then(function (arr) {
                if (!arr || arr.length === 0) return null;
                return { lat: parseFloat(arr[0].lat), lon: parseFloat(arr[0].lon), display_name: arr[0].display_name };
            });
    }

    /* ── Get driving route from OSRM ───────────────────────── */
    function getRoute(start, end) {
        var url = "https://router.project-osrm.org/route/v1/driving/"
            + start.lon + "," + start.lat + ";"
            + end.lon + "," + end.lat
            + "?overview=full&geometries=geojson&steps=false";
        return fetch(url)
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data || data.code !== "Ok" || !data.routes || !data.routes.length)
                    throw new Error("No route found");
                return {
                    route: data.routes[0].geometry,
                    distance: data.routes[0].distance,
                    duration: data.routes[0].duration
                };
            });
    }

    /* ── Draw route on map ──────────────────────────────────── */
    function drawRoute(startCoord, endCoord, endDisplayName, routeColor) {
        return getRoute(startCoord, endCoord)
            .then(function (summary) {
                if (routeLayer) { map.removeLayer(routeLayer); routeLayer = null; }
                if (startMarker) { map.removeLayer(startMarker); startMarker = null; }
                if (endMarker) { map.removeLayer(endMarker); endMarker = null; }

                routeLayer = L.geoJSON(summary.route, {
                    style: { color: routeColor || "#007bff", weight: 5 }
                }).addTo(map);

                startMarker = L.marker([startCoord.lat, startCoord.lon])
                    .bindPopup("<b>Your location</b>").addTo(map).openPopup();

                endMarker = L.marker([endCoord.lat, endCoord.lon])
                    .bindPopup("<b>Delivery</b><br/>" + (endDisplayName || ""))
                    .addTo(map);

                var coords = summary.route.coordinates.map(function (c) { return [c[1], c[0]]; });
                if (coords.length) map.fitBounds(coords, { padding: [40, 40] });

                var km = (summary.distance / 1000).toFixed(2);
                var min = Math.ceil(summary.duration / 60);
                document.getElementById("routeInfoText").textContent =
                    "Distance: " + km + " km  |  Duration: " + min + " min";
            });
    }

    /* ── Start live GPS tracking ────────────────────────────── */
    function startLiveTracking() {
        if (watchId || !navigator.geolocation) return;
        watchId = navigator.geolocation.watchPosition(
            function (pos) {
                var lat = pos.coords.latitude, lon = pos.coords.longitude;
                if (!liveMarker) {
                    liveMarker = L.marker([lat, lon], {
                        icon: L.icon({
                            iconUrl: "https://maps.google.com/mapfiles/ms/icons/blue-dot.png",
                            iconSize: [32, 32]
                        })
                    }).addTo(map).bindPopup("Your location");
                } else {
                    liveMarker.setLatLng([lat, lon]);
                }
            },
            function (err) { console.warn("Geolocation error:", err.message); },
            { enableHighAccuracy: true, maximumAge: 2000, timeout: 10000 }
        );
    }

    /* ── Update status badge ────────────────────────────────── */
    function setStatusBadge(statusText) {
        var badge = document.getElementById("statusBadge");
        if (!badge) return;
        badge.textContent = statusText;
        badge.className = "dd-status-badge";
        if (statusText === "OutForDelivery") badge.classList.add("out");
        if (statusText === "Delivered") badge.classList.add("delivered");
    }

    /* ── POST helper ────────────────────────────────────────── */
    function postAction(url) {
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: "id=" + encodeURIComponent(orderId)
                + "&driverId=" + encodeURIComponent(driverId)
                + "&__RequestVerificationToken=" + encodeURIComponent(token)
        }).then(function (res) { return res.json(); });
    }

    /* ── Start Delivery ─────────────────────────────────────── */
    window.startDelivery = function () {
        var btn = document.getElementById("startBtn");
        btn.disabled = true;
        btn.textContent = "Starting...";

        if (!navigator.geolocation) {
            alert("Geolocation is not supported by your browser.");
            btn.disabled = false;
            btn.textContent = "Start Delivery";
            return;
        }

        navigator.geolocation.getCurrentPosition(
            function (pos) {
                var driverCoord = { lat: pos.coords.latitude, lon: pos.coords.longitude };

                geocode(deliveryAddress).then(function (dest) {
                    if (!dest) {
                        alert("Could not find the delivery address on the map.");
                        return;
                    }
                    drawRoute(driverCoord, dest, dest.display_name, "#28a745");
                    startLiveTracking();
                });

                postAction("/DeliveryDriver/StartDelivery").then(function (data) {
                    if (data.success) {
                        setStatusBadge("OutForDelivery");
                        document.getElementById("startBtn").disabled = true;
                        document.getElementById("completeBtn").disabled = false;
                    } else {
                        alert("Could not start delivery: " + (data.reason || "Unknown error"));
                        btn.disabled = false;
                        btn.textContent = "Start Delivery";
                    }
                });
            },
            function () {
                alert("Unable to get your location. Please allow location access.");
                btn.disabled = false;
                btn.textContent = "Start Delivery";
            }
        );
    };

    /* ── Complete Delivery ──────────────────────────────────── */
    window.completeDelivery = function () {
        var btn = document.getElementById("completeBtn");
        btn.disabled = true;
        btn.textContent = "Completing...";

        postAction("/DeliveryDriver/CompleteDelivery").then(function (data) {
            if (data.success) {
                setStatusBadge("Delivered");
                document.getElementById("startBtn").disabled = true;
                document.getElementById("completeBtn").disabled = true;

                /* Show delivered banner */
                var banner = document.getElementById("deliveredBanner");
                if (banner) {
                    banner.style.display = "flex";
                }

                /* Stop GPS tracking */
                if (watchId) {
                    navigator.geolocation.clearWatch(watchId);
                    watchId = null;
                }
            } else {
                alert("Could not complete delivery: " + (data.reason || "Unknown error"));
                btn.disabled = false;
                btn.textContent = "Mark as Delivered";
            }
        });
    };

    /* ── Boot: load initial route from store to customer ────── */
    document.addEventListener("DOMContentLoaded", function () {
        var storeAddress = "15 Dalton Road, Durban";

        /* If order is already OutForDelivery, try to use live GPS for start */
        if (orderStatus === "OutForDelivery" && navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    var driverCoord = { lat: pos.coords.latitude, lon: pos.coords.longitude };
                    geocode(deliveryAddress).then(function (dest) {
                        if (dest) drawRoute(driverCoord, dest, dest.display_name, "#28a745");
                    });
                    startLiveTracking();
                },
                function () {
                    /* Fall back to store → customer */
                    geocode(storeAddress).then(function (s) {
                        geocode(deliveryAddress).then(function (e) {
                            if (s && e) drawRoute(s, e, e.display_name, "#007bff");
                        });
                    });
                }
            );
        } else if (orderStatus !== "Delivered") {
            /* Default: show store → customer route */
            geocode(storeAddress).then(function (s) {
                geocode(deliveryAddress).then(function (e) {
                    if (s && e) drawRoute(s, e, e.display_name, "#007bff");
                });
            });
        } else {
            document.getElementById("routeInfoText").textContent = "Order has been delivered.";
        }
    });

})();