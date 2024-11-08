export function createWebSocketConnection() {
    const socket = new WebSocket("ws://localhost:5223/ws/desktop");

    socket.onopen = () => {
        console.log("Connected to ASP.NET server.")
    }

    socket.onmessage = (e: MessageEvent) => {
        const message = JSON.parse(e.data);
        console.log(`Received message from ASP.NET server: ${message}`);
    }

    socket.onclose = () => {
        console.log("WebSocket connection closed.")
    }

    socket.onerror = (e: Event) => {
        console.error("WebSocket error: ", e);
    }

    return socket;
}