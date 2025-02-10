import React, {useCallback, useEffect, useState} from "react";
import {
    Box,
    Button,
    Checkbox,
    FormControl,
    FormControlLabel, FormGroup,
    FormLabel,
    Radio,
    RadioGroup,
    TextField, Typography
} from "@mui/material";

const ToolsPage: React.FC = () => {
    const [fillerValue, setFillerValue] = useState("");
    const [fillerArray, setFillerArray] = useState<string[]>([]);
    const [fillType, setFillType] = useState("single");
    const [start, setStart] = useState<boolean>(false);
    const [selectionType, setSelectionType] = useState<string>("all");
    const [elements, setElements] = useState<string[]>([]);


    const handleFillerTextChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setFillerValue(value);
        setFillerArray(value.split("\n"));
    }

    const handleSubmitFillerText = () => {
        sendFillerTextToContentScript(fillerArray);
    }

    // const handleSubmitFillPage = () => {
    //     sendFillPageAction(fillerArray);
    // }

    const sendFillerTextToContentScript = useCallback((text: string[]) => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            if (tabs[0].id) {
                chrome.tabs.sendMessage(
                    tabs[0].id,
                    { action: "FILL_TEXT_BOXES", fillerTexts: text, fillType: fillType, locators: elements, selectionType: selectionType }
                )
            }
        })
    }, [fillType, selectionType, elements]);

    // const handleCheckAllBoxes = () => {
    //     sendCheckBoxFill();
    // }

    const sendCheckBoxFill = useCallback(() => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            if (tabs[0].id) {
                chrome.tabs.sendMessage(
                    tabs[0].id,
                    { action: "CHECK_BOXES" }
                )
            }
        })
    }, []);

    // const sendFillPageAction = useCallback((text: string[]) => {
    //     chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
    //         if (tabs[0].id) {
    //             chrome.tabs.sendMessage(
    //                 tabs[0].id,
    //                 { action: "FILL_PAGE", fillerTexts: text, fillType: fillType, locators: elements, selectionType: selectionType }
    //             )
    //         }
    //     });
    // }, [fillType, selectionType, elements]);

    const handleTypeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setFillType(event.target.value);
    }

    const handleElementSelection = (event: React.ChangeEvent<HTMLInputElement>) => {
        setSelectionType(event.target.value);
    }

    const handleClearElements = () => {
        setElements([]);
    }

    const [checkInputs, setCheckInputs] = useState<boolean>(true);
    const [checkTextAreas, setCheckTextAreas] = useState<boolean>(true);
    const [checkOnlyNum, setCheckOnlyNum] = useState<boolean>(false);
    const [checkOnlyText, setCheckOnlyText] = useState<boolean>(false);
    const [checkOnlySpecial, setCheckOnlySpecial] = useState<boolean>(false);

    const TOOLS_KEY = "toolsState";

    const sendStartState = useCallback(() => {
        chrome.storage.local.set({ [TOOLS_KEY]: start });
    }, [start]);

    useEffect(() => {
        sendStartState();
    }, [sendStartState]);

    const handleStartSwitch = () => {
        setStart(!start);
    }

    useEffect(() => {
        const handleMessage = (message: any) => {
            if (message.action === "TOOLS_SEND_ELEMENT") {
                const element: string = message.locator;
                setElements(prevElements => [...prevElements, element]);
            }
        }

        chrome.runtime.onMessage.addListener(handleMessage);

        return () => {
            chrome.runtime.onMessage.removeListener(handleMessage);
        };
    }, [elements]);

    return (
        <>
            <FormControl>
                <FormLabel>Select an Option</FormLabel>
                <RadioGroup value={selectionType} onChange={handleElementSelection}>
                    <FormControlLabel value="all" control={<Radio />} label="All Page Elements" />
                    <FormControlLabel value="selected" control={<Radio />} label="Selected Page Elements" />
                </RadioGroup>
            </FormControl>
            <Box
                sx={{
                    width: "100%",
                    height: "15vh",
                    display: "flex",
                    flexDirection: "column",
                    gap: 1,
                    // outline: "2px solid red",
                    mb: 2,
                }}
            >
                <Box
                    sx={{
                        width: "100%",
                        height: "10vh",
                        overflowY: "auto",
                        overflowX: "hidden",
                        whiteSpace: "nowrap",
                        borderRadius: 2,
                        outline: "1px solid lightgray",
                    }}
                >
                    {elements.map((element, index) => (
                        <Typography key={index} >{element}</Typography>
                    ))}
                </Box>
                <Box
                    sx={{
                        width: "100%",
                        display: "flex",
                        justifyContent: "flex-end",
                        gap: 2,
                    }}
                >
                    <Button variant="contained" color="primary" onClick={handleStartSwitch} sx={{ width: 4 }}>
                        {start ? "STOP" : "START"}
                    </Button>
                    <Button variant="contained" color="primary" onClick={handleClearElements} sx={{ width: 4 }}>
                        CLEAR
                    </Button>
                </Box>
            </Box>
            <Box
                sx={{
                    width: "100%",
                    height: "60vh",
                    display: "flex",
                    flexDirection: "column",
                }}
            >
                <Box
                    sx={{
                        width: "100%",
                        mt: 2,
                    }}
                >
                    <TextField
                        label="Enter Filler Texts..."
                        variant="outlined"
                        onChange={handleFillerTextChange}
                        minRows={3}
                        maxRows={10}
                        multiline
                        fullWidth
                        value={fillerValue}
                        sx={{
                            whiteSpace: "nowrap",
                            overflow: "auto",
                            fontFamily: "monospace",
                        }}
                        slotProps={{
                            htmlInput: {
                                style: {
                                    whiteSpace: "nowrap",
                                    overflowX: "auto",
                                },
                            },
                        }}
                    />
                </Box>
                <Box
                    sx={{
                        width: "100%",
                        display: "flex",
                        gap: 2,
                    }}
                >
                    <Box
                        sx={{
                            width: "20%",
                        }}
                    >
                        <FormControl>
                            <FormLabel>Fill Method</FormLabel>
                            <RadioGroup value={fillType} onChange={handleTypeChange}>
                                <FormControlLabel value="single" control={<Radio />} label="Single" />
                                <FormControlLabel value="random" control={<Radio />} label="Random" />
                                <FormControlLabel value="unique" control={<Radio />} label="Unique" />
                            </RadioGroup>
                        </FormControl>
                    </Box>
                    <Box
                        sx={{
                            width: "80%",
                            display: "flex",
                            flexDirection: "column",
                            flexWrap: "wrap",
                            justifyContent: "center",
                        }}
                    >
                        <FormControl component="fieldset">
                            <FormLabel component="legend">Filters</FormLabel>
                            <FormGroup>
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={checkInputs}
                                            onChange={(event) => setCheckInputs(event.target.checked)}
                                        />
                                    }
                                    label="Input Elements"
                                />
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={checkTextAreas}
                                            onChange={(event) => setCheckTextAreas(event.target.checked)}
                                        />
                                    }
                                    label="Text Area Elements"
                                />
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={checkOnlyNum}
                                            onChange={(event) => setCheckOnlyNum(event.target.checked)}
                                        />
                                    }
                                    label="Only Numbers"
                                />
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={checkOnlyText}
                                            onChange={(event) => setCheckOnlyText(event.target.checked)}
                                        />
                                    }
                                    label="Only Text"
                                />
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={checkOnlySpecial}
                                            onChange={(event) => setCheckOnlySpecial(event.target.checked)}
                                        />
                                    }
                                    label="Only Special Characters"
                                />
                            </FormGroup>
                        </FormControl>
                    </Box>
                </Box>
                <Box
                    sx={{
                        width: "100%",
                        display: "flex",
                        justifyContent: "flex-end",
                        gap: 1,
                    }}
                >
                    <Button variant="contained" color="primary" onClick={handleSubmitFillerText}>
                        Fill
                    </Button>
                    {/*<Button variant="contained" color="primary" onClick={handleSubmitFillPage}>*/}
                    {/*    Fill Page*/}
                    {/*</Button>*/}
                    <Button variant="contained" color="primary" onClick={sendCheckBoxFill}>
                        Check All Boxes
                    </Button>
                </Box>
            </Box>
        </>
    )
}

export default ToolsPage;