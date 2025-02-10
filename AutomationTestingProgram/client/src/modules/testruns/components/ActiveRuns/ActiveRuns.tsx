import { Delete, PlayCircle, StopCircle } from "@mui/icons-material";
import { Box, IconButton, Typography, Divider } from "@mui/material";


const ActiveRuns = (props: any) => {
    return (
        <>
            <Box
                sx={{
                    width: "100%",
                    height: "100%",
                    background: "beige",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    p: 2,

                }}
            >
                <Typography>{props.testRun.id}</Typography>
                {
                    props.showButton ?
                        <Box
                            sx={{
                                marginRight: "1rem"
                            }}
                        >
                            <IconButton sx={{
                                marginLeft: "auto"
                            }}
                                onClick={e => props.handleJoinRun(e, props.testRun.id)}
                            >
                                <PlayCircle />
                            </IconButton>
                        </Box>
                        : <></>
                }
            </Box>
            <Divider sx={{}} />
        </>
    );
}

export default ActiveRuns;