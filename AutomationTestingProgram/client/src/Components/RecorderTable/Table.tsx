import React from "react";
import "./Table.module.css";
import { ActionDetails } from "../../interfaces";
import Button from "../Button/Button";

interface TableProps {
    rows: ActionDetails[];
    setRows: React.Dispatch<React.SetStateAction<ActionDetails[]>>;
}

const Table: React.FC<TableProps> = ((props: TableProps) => {
    const { rows, setRows } = props;

    React.useEffect(() => {
        const handleClick = (event: Event, actionDetails: ActionDetails) => {
            setRows(prevRows => {
                const newRows = [...prevRows, actionDetails];
                window.scrollTo(0, document.body.scrollHeight);
                return newRows;
            })
        }
        //window.electronAPI.clickWebElement(handleClick);
    }, [setRows]);

    const handleRemoveRow = (index: number) => {
        setRows(prevRows => {
            const deletedRow = prevRows[index];
            //window.electronAPI.verifyStepNum(deletedRow);
            return prevRows.filter((_, i) => i !== index);
        })
    }

    const highlightElement = (e: React.MouseEvent) => {
        const highlightLocator = e.currentTarget.textContent as string;
        let nextSibling = e.currentTarget.nextElementSibling;
        let type = "";

        while (nextSibling) {
            if (nextSibling.className === "comments") {
                type = nextSibling.textContent as string;
            }

            nextSibling = nextSibling.nextElementSibling;
        }
        console.log(type)
        const element = {
            locator: highlightLocator,
            type: type,
        }

        //window.electronAPI.highlightLocator(element);
    }

    const removeHighlight = (e: React.MouseEvent) => {
        const highlightLocator = e.currentTarget.textContent as string;
        let nextSibling = e.currentTarget.nextElementSibling;
        let type = "";

        while (nextSibling) {
            if (nextSibling.className === "comments") {
                type = nextSibling.textContent as string;
            }

            nextSibling = nextSibling.nextElementSibling;
        }
        const element = {
            locator: highlightLocator,
            type: type,
        }

        //window.electronAPI.removeHighlight(element);
    }

    const handleEdit = (e: React.FocusEvent, index: number) => {
        const newRows = [...rows];
        const field: string = e.currentTarget.className;
        newRows[index][field] = (e.currentTarget.textContent as string).trim();
        setRows(newRows);
    }

    const handleAddRowAbove = (index: number) => {
        const row = rows[index];
        const actionDetails: ActionDetails = {
            caseName: row.caseName,
            desc: "",
            stepNum: 1,
            action: "",
            object: "",
            value: "",
            comments: "",
            release: row.release,
            attempts: "",
            timeout: "",
            control: "",
            collection: row.collection,
            stepType: row.stepType,
            goto: "",
            uniqueLocator: false
        };

        setRows(prevRows => {
            const newRows = [...prevRows];
            newRows.splice(index, 0, actionDetails);
            let count = 1;
            for (const row of newRows) {
                if (row.caseName === actionDetails.caseName) {
                    row.stepNum = count;
                    count++;
                }
            }
            return newRows;
        });
    }

    const handleAddRowBelow = (index: number) => {
        const row = rows[index];
        const actionDetails: ActionDetails = {
            caseName: row.caseName,
            desc: "",
            stepNum: 1,
            action: "",
            object: "",
            value: "",
            comments: "",
            release: row.release,
            attempts: "",
            timeout: "",
            control: "",
            collection: row.collection,
            stepType: row.stepType,
            goto: "",
            uniqueLocator: false
        };

        setRows(prevRows => {
            const newRows = [...prevRows];
            newRows.splice(index + 1, 0, actionDetails);
            let count = 1;
            for (const row of newRows) {
                if (row.caseName === actionDetails.caseName) {
                    row.stepNum = count;
                    count++;
                }
            }
            return newRows;
        });
    }

    return (
        <>
            <table id="test-table">
                <thead>
                    <tr id="table-head">
                        <th id="actions-container">ACTIONS</th>
                        <th>TESTCASENAME</th>
                        <th>TESTDESCRIPTION</th>
                        <th>STEPNUM</th>
                        <th>ACTIONONOBJECT</th>
                        <th>OBJECT</th>
                        <th>VALUE</th>
                        <th>COMMENTS</th>
                        <th>RELEASE</th>
                        <th>LOCAL_ATTEMPTS</th>
                        <th>LOCAL_TIMEOUT</th>
                        <th>CONTROL</th>
                        <th>COLLECTION</th>
                        <th>TESTSTEPTYPE</th>
                        <th>GOTOSTEP</th>
                    </tr>
                </thead>
                <tbody id="table-body">
                    {rows.map((row, index) => (
                        <tr key={index} id={`row${index}`}>
                            <td className="actions"><Button content={"X"} onClick={() => handleRemoveRow(index)} /><Button content={"\u25B2"} onClick={() => handleAddRowAbove(index)}></Button><Button content={"\u25BC"} onClick={() => handleAddRowBelow(index)}></Button></td>
                            <td className="caseName" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.caseName}</td>
                            <td className="desc" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.desc}</td>
                            <td className="stepNum" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.stepNum}</td>
                            <td className="action" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.action}</td>
                            <td className="object" onBlur={(e) => handleEdit(e, index)} onMouseOver={highlightElement} onMouseOut={removeHighlight} contentEditable suppressContentEditableWarning={true}>{row.object}</td>
                            <td className="value" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.value}</td>
                            <td className="comments" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.comments}</td>
                            <td className="release" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.release}</td>
                            <td className="attempts" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.attempts}</td>
                            <td className="timeout" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.timeout}</td>
                            <td className="control" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.control}</td>
                            <td className="collection" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.collection}</td>
                            <td className="stepType" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.stepType}</td>
                            <td className="goto" onBlur={(e) => handleEdit(e, index)} contentEditable suppressContentEditableWarning={true}>{row.goto}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </>
    );
})

export default Table;