import React, { useRef } from "react";
import Button from "../Button/Button";
import "./ToolBar.css";
import { Constants } from "../../interfaces";

interface ToolBarProps {
    constants: Constants;
    setConstants: React.Dispatch<React.SetStateAction<Constants>>;
    onClearTable: () => void;
    onExport: () => void;
    onSave: () => void;
    onLoad: () => void;
}

function ToolBar(props: ToolBarProps) {
    const hasMounted = useRef(false);
    const [testToggle, setTestToggle] = React.useState(false);
    const [xpathToggle, setXpathToggle] = React.useState(false);
    const [caseName, setCaseName] = React.useState("");
    const [release, setRelease] = React.useState("");
    const [collection, setCollection] = React.useState("");
    const [stepType, setStepType] = React.useState("");

    function handleTestToggle() {
        setTestToggle(!testToggle);
    }

    function handleXpathToggle() {
        setXpathToggle(!xpathToggle);
    }

    function handleSetConstants() {
        props.setConstants({
            caseName,
            release,
            collection,
            stepType,
        });
    }

    // React.useEffect(() => {
    //     if (hasMounted.current) {
    //         window.electronAPI.setConstants(props.constants);
    //     }
    // }, [props.constants]);

    // React.useEffect((): void => {
    //     if (hasMounted.current) {
    //         window.electronAPI.recorderToggle(testToggle);
    //     }
    // }, [testToggle]);

    // React.useEffect(() => {
    //     if (hasMounted.current) {
    //         window.electronAPI.xpathToggle(xpathToggle);
    //     }
    // }, [xpathToggle]);

    React.useEffect(() => {
        hasMounted.current = true;
    }, []);

    return (
        <>
            <div className="set-values">
                <div className="value-inputs">
                    <div className="constant-container">
                        <label htmlFor="test-case-name">TESTCASENAME:</label>
                        <input type="text" id={"input-name"} value={caseName} onChange={(e) => setCaseName(e.target.value)} />
                    </div>
                    <div className="constant-container">
                        <label htmlFor="input-release">RELEASE:</label>
                        <input type="text" id={"input-release"} value={release} onChange={(e) => setRelease(e.target.value)} />
                    </div>
                    <div className="constant-container">
                        <label htmlFor="input-collection">COLLECTION:</label>
                        <input type="text" id={"input-collection"} value={collection} onChange={(e) => setCollection(e.target.value)} />
                    </div>
                    <div className="constant-container">
                        <label htmlFor="input-type">TESTSTEPTYPE:</label>
                        <input type="text" id={"input-type"} value={stepType} onChange={(e) => setStepType(e.target.value)} />
                    </div>
                </div>
                <div className="value-buttons">
                    <Button content={"SET"} onClick={handleSetConstants} />
                    <Button content={xpathToggle ? "XPATH: ON" : "XPATH: OFF"} onClick={handleXpathToggle} />
                    <Button content={"EXPORT"} onClick={props.onExport} />
                    <Button content={"SAVE"} onClick={props.onSave} />
                    <Button content={"LOAD"} onClick={props.onLoad} />
                    <Button content={testToggle ? "STOP" : "START"} onClick={handleTestToggle} />
                    <Button content={"CLEAR"} onClick={props.onClearTable} />
                </div>
            </div>
        </>
    );
}

export default ToolBar;