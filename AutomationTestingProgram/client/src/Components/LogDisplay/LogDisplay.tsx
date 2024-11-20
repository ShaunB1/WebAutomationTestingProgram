import {useEffect, useRef, useState} from "react";
import {Box, List, ListItem, Paper, Typography} from "@mui/material";

const LogDisplay = () => {
    const [logs, setLogs] = useState<string[]>([]);
    const logContainerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const socket = new WebSocket('ws://localhost:5223/ws/logs');

        socket.onmessage = (event) => {
            setLogs((prevLogs) => [...prevLogs, event.data as string]);
        }

        socket.onerror = (error) => {
            console.error("WebSocket error:", error);
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