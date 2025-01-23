import './DataTable.css';
import { useState } from "react";
import React from "react";
import {
    Button,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
} from "@mui/material";

interface DataTableProps {
    testCaseName: string;
}

interface Column {
    name: string;
    editable: boolean;
}

const DataTable: React.FC<DataTableProps> = ({ testCaseName }) => {
    const [columns, setColumns] = useState<Column[]>([{ name: "Variable 1", editable: false }]);
    const [rows, setRows] = useState([[""]]);

    const addColumn = () => {
        const newColName = `Variable ${columns.length + 1}`;
        setColumns([...columns, { name: newColName, editable: false }]);
        setRows(rows.map(row => [...row, ""]));
    }

    const addRow = () => {
        setRows([...rows, Array(columns.length).fill('')]);
    }

    const handleCellChange = (rowIndex: number, colIndex: number, value: string) => {
        setRows(prevRows => (
            prevRows.map((row, index) => (
                index === rowIndex
                    ? row.map((cell, i) => (i === colIndex ? value : cell))
                    : row
            ))
        ));
    }

    const handleHeaderDoubleClick = (index: number) => {
        if (index !== null) {
            setColumns(columns.map((col, colIndex) => colIndex === index ? { ...col, editable: true } : col ))
        }
    }

    const handleHeaderChange = (event: React.ChangeEvent<HTMLInputElement>, index: number) => {
        const newColumns = [...columns];
        newColumns[index].name = event.target.value;
        setColumns(newColumns);
    }

    const handleHeaderBlur = (index: number) => {
        const newColumns = [...columns];
        newColumns[index].editable = false;
        setColumns(newColumns);
    }

    const handleClearTable = () => {
        setRows([[""]]);
        setColumns([{ name: "Variable 1", editable: false }]);
    }

    const handleExportToCSV = () => {
        const csvRows: string[] = [];

        csvRows.push(["Cycle Iteration", ...columns.map((col) => col.name)].join(","));
        rows.forEach((row, rowIndex) => {
            csvRows.push([rowIndex+1, ...row].join(","));
        });

        const csvContent = csvRows.join("\n");

        const blob = new Blob([csvContent], { type: "text/csv" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = "data.csv";
        a.click();
        URL.revokeObjectURL(url);
    }

    return (
        <>
            <Button variant="contained" className="button-primary" onClick={addColumn}>Add Column</Button>
            <Button variant="contained" className="button-secondary" onClick={addRow}>Add Row</Button>
            <Button variant="contained" className="button-secondary" onClick={handleClearTable}>Clear Table</Button>
            <Button variant="contained" onClick={handleExportToCSV}>Export Data</Button>
            <TableContainer className="table-container">
                <Table aria-label="simple table">
                    <TableHead>
                        <TableRow>
                            <TableCell
                                colSpan={columns.length + 1} // +1 for "Cycle Iteration" column
                                align="center"
                                style={{
                                    backgroundColor: "#3f51b5", // Header background color
                                    color: "white",             // Header text color
                                    fontSize: "18px",           // Header font size
                                    fontWeight: "bold",         // Header font weight
                                }}
                            >
                                {testCaseName}
                            </TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell align="center" className="table-head-cell">Cycle Iteration</TableCell>
                            {columns.map((col, colIndex) => (
                                <TableCell
                                    key={colIndex}
                                    align="center"
                                    className="table-head-cell"
                                    onDoubleClick={() => handleHeaderDoubleClick(colIndex)}
                                >
                                    {col.editable ? (
                                        <TextField
                                            value={col.name}
                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleHeaderChange(e, colIndex)}
                                            onBlur={() => handleHeaderBlur(colIndex)}
                                            onKeyDown={(e) => {
                                                if (e.key === 'Enter') handleHeaderBlur(colIndex);
                                            }}
                                            size="small"
                                            className="textField-white"
                                        />
                                    ) : (
                                        col.name
                                    )}
                                </TableCell>
                            ))}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {rows.map((row, rowIndex) => (
                            <TableRow key={rowIndex} className={rowIndex % 2 === 0 ? "table-row-even" : "table-row-odd"}>
                                <TableCell align="center" className="table-cell">{rowIndex+1}</TableCell>
                                {row.map((cell, colIndex) => (
                                    <TableCell key={colIndex} align="center" className="table-cell">
                                        <TextField
                                            value={cell}
                                            onChange={(e) => handleCellChange(rowIndex, colIndex, e.target.value)}
                                            variant="outlined"
                                            size="small"
                                        />
                                    </TableCell>
                                ))}
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </>
    );
}

export default DataTable;
