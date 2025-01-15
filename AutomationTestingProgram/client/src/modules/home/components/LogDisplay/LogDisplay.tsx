import {useEffect, useRef, useState} from "react";
import {Box, List, ListItem, Paper, Typography} from "@mui/material";

interface TestRun {
    logs: string[];
}

const LogDisplay = () => {
    const [logs, setLogs] = useState<string[]>([]);
    const logContainerRef = useRef<HTMLDivElement>(null);
    const host = process.env.NODE_ENV === "production" ? process.env.VITE_HOST : process.env.LOCAL_HOST;

    useEffect(() => {
        const socket = new WebSocket(`wss://${host}/ws/logs`);
        console.log(`WS Host: ${host}`);
        
        socket.onopen = (event) => {
            console.log("Connected to WebSocket server");
        }
        
        socket.onmessage = (event) => {
            setLogs((prevLogs) => [...prevLogs, event.data as string]);
        }

        socket.onerror = (error) => {
            console.error("WebSocket error:", error);
        }

        socket.onclose = (event) => {
            console.log("Disconnected from WebSocket server");
        }

        return () => {
            socket.close();
        }
    }, [])

    useEffect(() => {
        if (logContainerRef.current) {
            logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
        }
    }, [logs]);

    return (
        <>
            <Box>
                <Paper
                    elevation={3}
                    ref={logContainerRef}
                    sx={{
                        width: '50%',
                        height: 600,
                        bgcolor: 'black',
                        color: 'white',
                        borderRadius: 2,
                        overflowY: 'auto',
                    }}
                >
                    <Box
                        sx={{
                            bgcolor: "#313d4f",
                            p: 1,
                            textAlign: 'center',
                            position: 'sticky',
                            top: 0,
                            zIndex: 1
                        }}
                    >
                        <Typography variant={"h6"} color={"white"}>
                            Test Run 1
                        </Typography>
                    </Box>
                    <List>
                        {logs.map((log, logIndex) => (
                            <ListItem key={logIndex}>
                                <Typography>
                                    {log}
                                </Typography>
                            </ListItem>
                        ))}
                    </List>
                </Paper>
            </Box>
        </>
    );
}

export default LogDisplay;