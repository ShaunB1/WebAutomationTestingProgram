import { Box, IconButton, ListItem, Paper, Typography } from "@mui/material";
import { ArrowDownward, Delete, PauseCircle, PlayCircle, StopCircle } from "@mui/icons-material";
import { useEffect, useRef, useState } from "react";
import { FixedSizeList as List } from 'react-window';

const LogDisplay = (props: any) => {
    const logRef = useRef<any>(null);
    const [autoScroll, setAutoScroll] = useState(false);

    useEffect(() => {
        if (logRef.current && autoScroll) {
            logRef.current.scrollToItem(props.testRunLog.logs.length - 1, "end");
        }
    }, [props.testRunLog.logs, autoScroll]);

    const handleAutoScroll = (e: any) => {
        setAutoScroll(prevAutoScroll => !prevAutoScroll);
    }

    const Row = ({ index, style }: { index: number; style: React.CSSProperties }) => {
        const log = props.testRunLog.logs[index];
        return (
            <div style={{ ...style }}>
                <Typography
                    component="pre"
                    sx={{
                        whiteSpace: "nowrap",
                        fontFamily: "Courier, monospace",
                        fontSize: "1rem",
                        margin: "0 20px",
                        color: log.includes("TEST CASE COMPLETE") || log.includes("TEST STEP COMPLETE") ? "green" :
                            log.includes("TEST CASE FAILURE") || log.includes("TEST STEP FAILURE") ? "red" : 
                            log.includes("TEST CASE START") || log.includes("TEST STEP START") ? "yellow" : "white",
                    }}
                >
                    {log}
                </Typography>
            </div>
        );
    };
    return (
        <>
            <Box
                sx={{
                    width: "100%",
                    display: "flex",
                    justifyContent: "center",
                    pt: 2,
                }}
            >
                <Box
                    sx={{
                        width: "97%",
                        display: "flex",
                        flexWrap: "wrap",
                        justifyContent: "center",
                    }}
                >
                    <Paper
                        elevation={3}
                        sx={{
                            width: '100%',
                            height: 600,
                            bgcolor: 'black',
                            color: 'white',
                            borderRadius: 2,
                        }}
                    >
                        <Box
                            sx={{
                                bgcolor: "#313d4f",
                                p: 1,
                                textAlign: 'center',
                                top: 0,
                                zIndex: 1,
                                borderRadius: 2,
                            }}
                        >
                            <Box
                                sx={{
                                    width: "100%",
                                    height: "100%",
                                    display: "flex",
                                    justifyContent: "flex-start",
                                    alignItems: "center",
                                    position: "relative",
                                    marginLeft: "1rem"
                                }}
                            >
                                <Typography variant={"h6"} color={"white"}
                                    sx={{
                                        maxWidth: 'calc(100% - 12rem)',
                                        overflow: 'hidden',
                                        textOverflow: 'ellipsis',
                                        whiteSpace: 'nowrap',
                                    }}>
                                    {props.testRunLog.id}
                                </Typography>
                                <Box
                                    sx={{
                                        position: "absolute",
                                        right: "13em",
                                    }}
                                >
                                    <IconButton
                                        sx={{
                                            color: autoScroll ? "green" : "white",
                                        }}
                                        onClick={handleAutoScroll}
                                    >
                                        <ArrowDownward />
                                    </IconButton>
                                </Box>
                                <Box
                                    sx={{
                                        position: "absolute",
                                        right: "10rem",
                                    }}
                                >
                                    <IconButton
                                        sx={{
                                            color: "white",
                                        }}
                                        onClick={e => props.handleUnpauseRun(e, props.testRunLog.id)}
                                    >
                                        <PlayCircle />
                                    </IconButton>
                                </Box>
                                <Box
                                    sx={{
                                        position: "absolute",
                                        right: "7rem",
                                    }}
                                >
                                    <IconButton
                                        sx={{
                                            color: "white",
                                        }}
                                        onClick={e => props.handlePauseRun(e, props.testRunLog.id)}
                                    >
                                        <PauseCircle />
                                    </IconButton>
                                </Box>
                                <Box
                                    sx={{
                                        position: "absolute",
                                        right: "4rem",
                                    }}
                                >
                                    <IconButton
                                        sx={{
                                            color: "white",
                                        }}
                                        onClick={e => props.handleStopRun(e, props.testRunLog.id)}
                                    >
                                        <StopCircle />
                                    </IconButton>
                                </Box>
                                <Box
                                    sx={{
                                        position: "absolute",
                                        right: "1rem",
                                    }}
                                >
                                    <IconButton
                                        sx={{
                                            color: "red",
                                        }}
                                        onClick={e => props.handleCloseRun(e, props.testRunLog.id)}
                                    >
                                        <Delete />
                                    </IconButton>
                                </Box>
                            </Box>
                        </Box>
                        {/* Virtualized List using react-window to reduce lag */}
                        <List
                            height={580}
                            itemCount={props.testRunLog.logs.length}
                            itemSize={24}
                            width={"100%"}
                            ref={logRef}

                        >
                            {Row}
                        </List>
                    </Paper>
                </Box>
            </Box>
        </>
    );
}

export default LogDisplay;