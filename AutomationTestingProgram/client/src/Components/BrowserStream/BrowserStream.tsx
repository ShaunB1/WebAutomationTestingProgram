import React, { useEffect, useRef } from "react";

const BrowserStream: React.FC = () => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const wsRef = useRef<WebSocket | null>(null);

    useEffect(() => {
        console.log("HIT");

        if (wsRef.current) {
            // WebSocket is already initialized
            console.log("WebSocket already initialized.");
            return;
        }

        const ws = new WebSocket("ws://localhost:5223/ws/desktop");
        wsRef.current = ws;

        ws.onopen = () => {
            console.log("WebSocket connection established.");
        };

        ws.onmessage = event => {
            console.log("Message from server:", event.data);
            // Handle incoming messages
        };

        ws.onerror = error => {
            console.error("WebSocket error:", error);
        };

        ws.onclose = event => {
            console.log("WebSocket connection closed:", event);
        };

        return () => {
            console.log("Closing WebSocket connection...");
            if (wsRef.current && wsRef.current.readyState === WebSocket.OPEN) {
                wsRef.current.close();
            }
            wsRef.current = null;
        };
    }, []);

    return <canvas ref={canvasRef} width={800} height={600} />;
};

export default BrowserStream;
