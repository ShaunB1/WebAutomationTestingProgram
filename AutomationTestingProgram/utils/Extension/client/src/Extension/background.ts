// const socketUrl = "ws://localhost:5223/ws/recorder"
// let socket: WebSocket | null = null;
//
// function initWebSocket() {
//     socket = new WebSocket(socketUrl);
//
//     socket.onopen = () => {
//         console.log("WebSocket connection established.");
//     }
//
//     socket.onmessage = (event) => {
//         console.log("Message from server: ", event.data);
//     }
//
//     socket.onclose = (event) => {
//         console.log(`WebSocket closed: ${event.code} - ${event.reason}`);
//     }
//
//     socket.onerror = (error) => {
//         console.error(`WebSocket error: `, error);
//     }
// }
//
// initWebSocket();
//
// function sendInteractionData(eventData: any) {
//     if (socket?.readyState === WebSocket.OPEN) {
//         const message = JSON.stringify({ type: "interaction", data: eventData });
//         socket.send(message);
//     } else {
//         console.error("WebSocket is not open.");
//     }
// }

// chrome.runtime.onMessage.addListener(sendInteractionData);
chrome.runtime.onMessage.addListener((message) => {
    if (message.action === "actiononobject") {
        chrome.runtime.sendMessage({ action: "RECORD_TEST_STEP", locator: message.locator, stepValues: message.stepValues });
    }
});