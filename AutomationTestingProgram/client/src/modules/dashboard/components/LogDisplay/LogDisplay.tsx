import {Box, IconButton, List, ListItem, Paper, Typography} from "@mui/material";
import React from "react";
import {ArrowDownward, Delete, PauseCircle, PlayCircle, StopCircle} from "@mui/icons-material";

const LogDisplay = () => {
    return (
        <>
            <Box
                sx={{
                    width: "100%",
                    display: "flex",
                    justifyContent: "center",
                    pt: 2,
                    overflowY: "auto",
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
                        // ref={logContainerRef}
                        sx={{
                            width: '100%',
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
                            <Box
                                sx={{
                                    width: "100%",
                                    height: "100%",
                                    display: "flex",
                                    justifyContent: "center",
                                    alignItems: "center",
                                    position: "relative",
                                }}
                            >
                                <Typography variant={"h6"} color={"white"}>
                                    {/*Test Run {index + 1}*/}
                                    Test Run
                                </Typography>
                                <Box
                                    sx={{
                                        position: "absolute",
                                        right: "10em",
                                    }}
                                >
                                    <IconButton
                                        sx={{
                                            color: "white",
                                        }}
                                    >
                                        <ArrowDownward />
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
                                    >
                                        <PlayCircle />
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
                                    >
                                        <PauseCircle />
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
                                    >
                                        <Delete />
                                    </IconButton>
                                </Box>
                            </Box>
                        </Box>
                        <List>
                            {/*{testRun.logs.map((log, logIndex) => (*/}
                            {/*    <ListItem key={logIndex}>*/}
                            {/*        <Typography*/}
                            {/*            component="pre"*/}
                            {/*            sx={{*/}
                            {/*                whiteSpace: "pre-wrap",*/}
                            {/*                fontFamily: "Courier, monospace",*/}
                            {/*                fontSize: "1rem",*/}
                            {/*                color: log.includes("STATUS: True") ? "green" : log.includes("STATUS: False") ? "red" : "white",*/}
                            {/*            }}*/}
                            {/*        >*/}
                            {/*            {log}*/}
                            {/*        </Typography>*/}
                            {/*    </ListItem>*/}
                            {/*))}*/}
                        </List>
                    </Paper>
                </Box>
            </Box>
        </>
    );
}

export default LogDisplay;