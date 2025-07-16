@extends('layouts.main')

@section('content')
<div class="main-content app-content">
    <div class="container-fluid">

        <div class="d-md-flex d-block align-items-center justify-content-between my-4 page-header-breadcrumb">
            <div>
                <h2>Quiz Dashboard</h2>
                <p class="mb-0 text-muted">Real-time monitoring of your quiz participants and results.</p>
            </div>
            <div class="ms-md-1 ms-0">
                <nav>
                    <ol class="breadcrumb mb-0">
                        <li class="breadcrumb-item"><a href="#">Admin</a></li>
                        <li class="breadcrumb-item active" aria-current="page">Quiz Dashboard</li>
                    </ol>
                </nav>
            </div>
        </div>

        <div class="row">
            {{-- WebSocket Activity Feed --}}
            <div class="col-xl-4">
                <div class="card border-0 shadow-sm h-100">
                    <div class="card-header bg-gradient-primary text-white p-3">
                        <div class="d-flex align-items-center">
                            <i class="bi bi-chat-left-text-fill me-2 fs-5"></i>
                            <h6 class="mb-0">🚀 Participants Activity</h6>
                        </div>
                    </div>
                    <div class="card-body p-0" style="max-height: 500px; overflow-y: auto;">
                        <div id="log" class="p-3"></div>
                    </div>
                    <div class="card-footer bg-light p-3">
                        <small id="log-last-updated" class="text-muted">Connecting to live feed...</small>
                    </div>
                </div>
            </div>

            {{-- Live Results --}}
            <div class="col-xl-8">
                <div class="card custom-card shadow-sm h-100">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">🏆 Live Results</h5>
                        <small id="results-last-updated" class="text-muted">Initialized: {{ now()->format('h:i:s A') }}</small>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <table class="table table-bordered text-nowrap">
                                <thead>
                                    <tr>
                                        <th scope="col">Rank</th>
                                        <th scope="col">Participant</th>
                                        <th scope="col">Correct / Total</th>
                                        <th scope="col">Time (s)</th>
                                        <th scope="col">Score (%)</th>
                                    </tr>
                                </thead>
                                <tbody id="results-tbody">
                                    @if(count($initialResults) > 0)
                                        @foreach($initialResults as $result)
                                        <tr>
                                            <td>{{ $loop->iteration }}</td>
                                            <td>{{ $result['participantName'] ?? 'N/A' }}</td>
                                            <td>{{ $result['correctCount'] ?? '0' }}/{{ $result['progress'] ?? '0' }}</td>
                                            <td>{{ $result['time'] ?? 'N/A' }}</td>
                                            <td>{{ isset($result['accuracy']) ? number_format($result['accuracy'] * 100, 0) : 0 }}%</td>
                                        </tr>
                                        @endforeach
                                    @else
                                        <tr>
                                            <td colspan="5" class="text-center text-muted">No results submitted yet</td>
                                        </tr>
                                    @endif
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<style>
    .avatar {
        display: inline-block;
        width: 30px;
        height: 30px;
        line-height: 30px;
        border-radius: 50%;
        background-color: #007bff;
        color: white;
        text-align: center;
        font-weight: bold;
        margin-right: 10px;
    }

    #log {
        margin-top: 0;
        border: none;
        padding: 0;
        background: transparent;
    }

    .log-message {
        padding: 10px;
        margin-bottom: 8px;
        border-radius: 8px;
        background-color: #f8f9fa;
        border-left: 3px solid #007bff;
        transition: background-color 0.3s ease;
    }

    .log-message.new {
        background-color: #e6f7ff;
        border-left-color: #28a745;
    }

    .log-message .timestamp {
        font-size: 0.75rem;
        color: #6c757d;
        display: block;
        margin-top: 3px;
    }
</style>

<script>
    // Global variables
    const uniqueId = @json($uniqueId);
    const liveResultsUrl = `{{ route('admin.getLiveResults', ['uniqueId' => $uniqueId]) }}`;
    const websocketUrl = `ws://156.67.218.162:7267/ws?sessionId=${uniqueId}`; 
    let ws = null;
    let isWebSocketConnecting = false;
    let resultsData = @json($initialResults);

    // DOM Elements
    const logElement = document.getElementById('log');
    const logLastUpdatedElement = document.getElementById('log-last-updated');
    const resultsTableBody = document.getElementById('results-tbody');
    const resultsLastUpdatedElement = document.getElementById('results-last-updated');

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function() {
        // Render initial results
        renderResultsTable(resultsData);
        
        // Set up polling for live results (every 5 seconds)
        setInterval(fetchLiveResults, 5000);
        

    });

    // Function to render results table
    function renderResultsTable(results) {
        if (!results || results.length === 0) {
            resultsTableBody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-muted">No results submitted yet</td>
                </tr>
            `;
            return;
        }
        
        // Sort results by accuracy and time
        const sortedResults = [...results].sort((a, b) => {
            // First by accuracy (descending)
            if (b.accuracy !== a.accuracy) {
                return (b.accuracy || 0) - (a.accuracy || 0);
            }
            // Then by time (ascending - lower time is better)
            return (a.time || Infinity) - (b.time || Infinity);
        });
        
        let tableHTML = '';
        sortedResults.forEach((result, index) => {
            tableHTML += `
                <tr>
                    <td>${index + 1}</td>
                    <td>${result.participantName || 'N/A'}</td>
                    <td>${result.correctCount || '0'}/${result.progress || '0'}</td>
                    <td>${result.time || 'N/A'}</td>
                    <td>${result.accuracy ? (result.accuracy * 100).toFixed(0) + '%' : '0%'}</td>
                </tr>
            `;
        });
        
        resultsTableBody.innerHTML = tableHTML;
        resultsLastUpdatedElement.textContent = `Updated: ${new Date().toLocaleTimeString()}`;
    }

    // Fetch live results from API
    async function fetchLiveResults() {
        try {
            const response = await fetch(liveResultsUrl);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            
            const data = await response.json();
            if (data.success && JSON.stringify(data.results) !== JSON.stringify(resultsData)) {
                resultsData = data.results;
                renderResultsTable(resultsData);
            }
        } catch (error) {
            console.error('Error fetching live results:', error);
            resultsLastUpdatedElement.textContent = 'Error updating results';
        }
    }
var messages = [];

function logMessage(message) {
    const log = document.getElementById('log');

    // Add the new message to the messages array
    messages.push(message); // Corrected from .add() to .push()

    // Update the log display
    log.innerHTML = messages.map((msg, index) => {
        const participantName = msg.split(' ')[0];
        // Assuming createAvatar function exists and works correctly
        const avatar = createAvatar(participantName);
        const displayMessage = msg.replace(participantName, `${avatar} ${participantName}`);
        // Corrected closing quote for the class attribute
        return `<div class="log-message>${displayMessage}</div>`;
    }).join('');


}

 function createAvatar(participantName) {
            const firstLetter = participantName.charAt(0).toUpperCase();
            return `<span class="avatar">${firstLetter}</span>`;
        }

        function connectWebSocket() {

            const url = `ws://156.67.218.162:5000/ws?uniqueId=${uniqueId}`;

            socket = new WebSocket(url);

            socket.onopen = () => {
                console.log("✅ Connected to WebSocket server");
                // logMessage("✅ Connected to WebSocket server");
            };

            socket.onmessage = (event) => {
                logMessage(event.data);
            };

            socket.onclose = () => {
                console.log("❌ WebSocket connection closed");
                // logMessage("❌ WebSocket connection closed");
            };

            socket.onerror = (error) => {
                console.log("⚠️ WebSocket error: ", error);
                // logMessage("⚠️ WebSocket error occurred");
            };

          
        }

        window.onload = connectWebSocket;
</script>
@endsection