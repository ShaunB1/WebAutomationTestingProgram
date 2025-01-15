import React, {useCallback, useState} from "react";
import {Button, TextField} from "@mui/material";

const ToolsPage: React.FC = () => {
    const [fillerValue, setFillerValue] = useState("");
    const handleFillerTextChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFillerValue(e.target.value);
    }

    const handleSubmitFillerText = () => {
        sendFillerTextToContentScript(fillerValue);
    }

    const sendFillerTextToContentScript = useCallback((text: string) => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            if (tabs[0].id) {
                chrome.tabs.sendMessage(
                    tabs[0].id,
                    { action: "FILL_TEXT_BOXES", fillerText: text },
                )
            }
        })
    }, []);

    const handleCheckAllBoxes = () => {

    }

    return (
        <>
            <TextField label="Enter Filler Text..." variant="outlined" onChange={handleFillerTextChange} value={fillerValue} />
            <Button variant="contained" color="primary" onClick={handleSubmitFillerText}>
                Fill
            </Button>
            <Button variant="contained" color="primary" onClick={handleCheckAllBoxes}>
                Check All Boxes
            </Button>
        </>
    )
}

export default ToolsPage;