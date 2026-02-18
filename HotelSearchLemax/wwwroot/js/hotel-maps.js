/*
Hotel Search Google Maps Implementation
Based on hzzweb MapsSurfaceController pattern
------------------------------------------------ */
function initHotelMaps() {
    /*
    1.1 Cache DOM elements
    ------------------------------------------------ */
    var geolocateBtn = $('#Geolocate');
    var searchForm = $('#searchForm');
    var radiusForm = $('#radiusForm');
    var searchRadius = $('#searchRadius');
    var searchKeywords = $('#searchKeywords');
    var sortBy = $('#sortBy');
    var sortDirection = $('#sortDirection');
    var hotelList = $('.hotel-list');
    var positionOut = null;

    var map = new google.maps.Map(document.getElementById("hotel-map"), {
        zoom: 12,
        center: { lat: 45.8150, lng: 15.9819 }, // Zagreb default
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    var bounds = new google.maps.LatLngBounds();
    var userInfoWindow = new google.maps.InfoWindow();
    var locationInfoWindow = new google.maps.InfoWindow({ content: "Loading..." });

    var markers = [];
    var searchArea = null;
    var userMarker = null;

    var searchFilters = {
        userLatitude: 45.8150,
        userLongitude: 15.9819,
        searchKeywords: '',
        radiusKm: null,
        sortBy: 'Score',
        sortDirection: 'Asc',
        pageNumber: 1,
        pageSize: 100
    };

    /*
    1.2 Event handlers
    ------------------------------------------------ */
    geolocateBtn.click(function (e) {
        e.preventDefault();
        initGeolocator();
    });

    searchKeywords.on('input', function () {
        searchFilters.searchKeywords = $(this).val();
    });

    searchForm.submit(function (e) {
        e.preventDefault();
        searchFilters.searchKeywords = searchKeywords.val();
        loadHotels();
    });

    radiusForm.submit(function (e) {
        e.preventDefault();
        var newRadius = parseFloat(searchRadius.val());
        if (newRadius && newRadius > 0) {
            searchFilters.radiusKm = newRadius;
            // Update the circle on map
            if (positionOut) {
                updateSearchArea(positionOut, searchFilters.radiusKm);
            }
            loadHotels();
        }
    });

    sortBy.change(function () {
        searchFilters.sortBy = $(this).val();
        loadHotels();
    });

    sortDirection.change(function () {
        searchFilters.sortDirection = $(this).val();
        loadHotels();
    });

    // Initial load
    loadHotels();

    /*
    1.3 Geolocator - Get user's current position
    ------------------------------------------------ */
    function initGeolocator() {
        if (navigator.geolocation) {
            geolocateBtn.prop('disabled', true).text('Locating...');

            navigator.geolocation.getCurrentPosition(
                function (position) {
                    positionOut = position;

                    // Update search filters with user's position
                    searchFilters.userLatitude = position.coords.latitude;
                    searchFilters.userLongitude = position.coords.longitude;

                    // Set default radius (10km) when geolocating
                    searchFilters.radiusKm = parseFloat(searchRadius.val()) || 10;

                    // Show user position with circle on map
                    showUserPosition(position);

                    // Load hotels within radius
                    loadHotels();

                    // Update UI
                    geolocateBtn.addClass('d-none');
                    $('.radius-form').removeClass('d-none');
                },
                function (error) {
                    handleLocationError(error);
                    geolocateBtn.prop('disabled', false).text('Geolocate Me');
                }
            );
        } else {
            alert('Geolocation is not supported by your browser.');
        }
    }

    /*
    1.4 Show user position on map with radius circle
    ------------------------------------------------ */
    function showUserPosition(position) {
        var lat = position.coords.latitude;
        var lng = position.coords.longitude;
        var point = new google.maps.LatLng(lat, lng);

        // Add user marker
        if (userMarker) {
            userMarker.setMap(null);
        }
        userMarker = new google.maps.Marker({
            position: point,
            map: map,
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                scale: 10,
                fillColor: '#4285F4',
                fillOpacity: 1,
                strokeColor: '#ffffff',
                strokeWeight: 2
            },
            title: 'Your Location'
        });

        // Show info window
        userInfoWindow.setPosition(point);
        userInfoWindow.setContent('<div style="padding: 5px;"><strong>You are here!</strong></div>');
        userInfoWindow.open(map);

        // Draw search radius circle
        updateSearchArea(position, searchFilters.radiusKm);
    }

    /*
    1.5 Update search area circle
    ------------------------------------------------ */
    function updateSearchArea(position, radiusKm) {
        var lat = position.coords.latitude;
        var lng = position.coords.longitude;
        var point = new google.maps.LatLng(lat, lng);
        var radiusMeters = (radiusKm || 10) * 1000;

        // Remove existing circle
        if (searchArea) {
            searchArea.setMap(null);
        }

        // Create new circle
        searchArea = new google.maps.Circle({
            strokeColor: '#FF0000',
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: '#FF0000',
            fillOpacity: 0.15,
            map: map,
            center: point,
            radius: radiusMeters
        });

        // Fit map to circle bounds
        map.fitBounds(searchArea.getBounds());
    }

    /*
    1.6 Load hotels from API
    ------------------------------------------------ */
    function loadHotels() {
        clearOverlays();
        bounds = new google.maps.LatLngBounds();
        $('#loading').show();

        var params = new URLSearchParams({
            userLatitude: searchFilters.userLatitude,
            userLongitude: searchFilters.userLongitude,
            sortBy: searchFilters.sortBy,
            sortDirection: searchFilters.sortDirection,
            pageNumber: searchFilters.pageNumber,
            pageSize: searchFilters.pageSize
        });

        if (searchFilters.searchKeywords) {
            params.append('searchKeywords', searchFilters.searchKeywords);
        }
        if (searchFilters.radiusKm) {
            params.append('radiusKm', searchFilters.radiusKm);
        }

        fetch('/api/hotels/search?' + params.toString())
            .then(response => response.json())
            .then(data => {
                hotelList.empty();

                if (data.items.length === 0) {
                    hotelList.html('<div class="alert alert-info">No hotels found within ' +
                        (searchFilters.radiusKm ? searchFilters.radiusKm + ' km radius.' : 'your criteria.') + '</div>');
                    $('#loading').hide();
                    $('#resultsCount').text('0 hotels found');
                    return;
                }

                data.items.forEach(function (hotel, index) {
                    // Add marker
                    var point = new google.maps.LatLng(hotel.latitude, hotel.longitude);
                    bounds.extend(point);

                    var marker = new google.maps.Marker({
                        position: point,
                        map: map,
                        title: hotel.name,
                        label: (index + 1).toString(),
                        optimized: false
                    });

                    google.maps.event.addListener(marker, 'click', function () {
                        locationInfoWindow.setContent(buildHotelInfo(hotel));
                        locationInfoWindow.open(map, this);
                    });

                    markers.push(marker);

                    // Add hotel card
                    hotelList.append(buildHotelCard(hotel, index + 1));
                });

                // Update results count
                $('#resultsCount').text(data.totalCount + ' hotels found' +
                    (searchFilters.radiusKm ? ' within ' + searchFilters.radiusKm + ' km' : ''));

                $('#loading').hide();

                // Fit map bounds
                if (searchArea !== null) {
                    // If we have a search area, fit to circle and add some zoom
                    map.fitBounds(searchArea.getBounds());
                } else if (markers.length > 0) {
                    // Otherwise fit to markers
                    map.fitBounds(bounds);
                }
            })
            .catch(error => {
                console.error('Error loading hotels:', error);
                hotelList.html('<div class="alert alert-danger">Error loading hotels. Please try again.</div>');
                $('#loading').hide();
            });
    }

    /*
    1.7 Build hotel info window content
    ------------------------------------------------ */
    function buildHotelInfo(hotel) {
        return `
            <div class="hotel-info" style="width: 280px;">
                <h5 style="margin: 0 0 10px 0;">${hotel.name}</h5>
                <p style="margin: 5px 0;"><strong>Price:</strong> €${hotel.price.toFixed(2)}/night</p>
                <p style="margin: 5px 0;"><strong>Distance:</strong> ${hotel.distanceKm.toFixed(2)} km</p>
                <p style="margin: 5px 0;"><strong>Score:</strong> ${hotel.score.toFixed(4)}</p>
                <p style="margin: 5px 0; font-size: 11px; color: #666;">
                    <strong>Coordinates:</strong> ${hotel.latitude.toFixed(6)}, ${hotel.longitude.toFixed(6)}
                </p>
                <div style="margin-top: 10px;">
                    <a href="/HotelsView/Details/${hotel.id}" class="btn btn-sm btn-primary">View Details</a>
                    <a href="/HotelsView/Edit/${hotel.id}" class="btn btn-sm btn-outline-secondary">Edit</a>
                </div>
            </div>
        `;
    }

    /*
    1.8 Build hotel card HTML
    ------------------------------------------------ */
    function buildHotelCard(hotel, rank) {
        return `
            <div class="card hotel-card mb-3" data-hotel-id="${hotel.id}">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <span class="badge bg-primary me-2">#${rank}</span>
                            <h5 class="card-title d-inline">${hotel.name}</h5>
                        </div>
                        <span class="badge bg-success">€${hotel.price.toFixed(2)}</span>
                    </div>
                    <div class="mt-2">
                        <small class="text-muted">
                            <i class="bi bi-geo-alt"></i> ${hotel.distanceKm.toFixed(2)} km away
                            &nbsp;|&nbsp;
                            <i class="bi bi-star"></i> Score: ${hotel.score.toFixed(4)}
                        </small>
                    </div>
                    <div class="mt-2">
                        <a href="/HotelsView/Details/${hotel.id}" class="btn btn-sm btn-outline-primary">Details</a>
                        <button class="btn btn-sm btn-outline-info show-on-map"
                                data-lat="${hotel.latitude}"
                                data-lng="${hotel.longitude}"
                                data-index="${rank - 1}">
                            Show on Map
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    // Show on map button click
    $(document).on('click', '.show-on-map', function () {
        var lat = parseFloat($(this).data('lat'));
        var lng = parseFloat($(this).data('lng'));
        var index = $(this).data('index');

        map.setCenter({ lat: lat, lng: lng });
        map.setZoom(16);

        if (markers[index]) {
            google.maps.event.trigger(markers[index], 'click');
        }
    });

    /*
    1.9 Handle location errors
    ------------------------------------------------ */
    function handleLocationError(error) {
        var message = 'Error getting location: ';
        switch (error.code) {
            case error.PERMISSION_DENIED:
                message += 'Permission denied. Please enable location access.';
                break;
            case error.POSITION_UNAVAILABLE:
                message += 'Location unavailable.';
                break;
            case error.TIMEOUT:
                message += 'Request timed out.';
                break;
            default:
                message += 'Unknown error.';
        }
        alert(message);
    }

    /*
    1.10 Clear map overlays
    ------------------------------------------------ */
    function clearOverlays() {
        for (var i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
        markers = [];
        bounds = new google.maps.LatLngBounds();
    }
}
