import {Box, IconButton, TextField, Typography} from "@mui/material";
import "./ChatBot.css";
import SendIcon from '@mui/icons-material/Send';
import {useState} from "react";

const ChatBot = () => {
    const [prompt, setPrompt] = useState<string>("");
    const [response, setResponse] = useState<string>("");

    const handleSubmitPrompt = async () => {
        try {
            const res = await fetch("http://localhost:5100/api/ai", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ prompt: prompt }),
            })

            const data = await res.json();
            setResponse(data.content || JSON.stringify(data));
        } catch (e) {
            console.log(e);
            setResponse("An error occurred while fetching the response.");
        }
    }

    return (
        <>
            <Box
                sx={{
                    height: "80vh",
                    background: "lightgray",
                    padding: 4,
                }}
            >
                <Box
                    sx={{
                        background: "white",
                        height: "90%",
                        width: "100%",
                        borderRadius: 2,
                        mb: 4,
                    }}
                >
                    <Typography>{response}</Typography>
                </Box>
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        background: "white",
                    }}
                >
                    <TextField
                        variant="outlined"
                        value={prompt}
                        onChange={(e) => setPrompt(e.target.value)}
                        sx={{
                            width: "99%",
                            "& .MuiOutlinedInput-root": {
                                "& fieldset": {
                                    border: "none", // Removes the outline
                                },
                            },
                        }}
                    >
                    </TextField>
                    <IconButton onClick={handleSubmitPrompt}>
                        <SendIcon />
                    </IconButton>
                </Box>
            </Box>
        </>
    );
}

export default ChatBot;