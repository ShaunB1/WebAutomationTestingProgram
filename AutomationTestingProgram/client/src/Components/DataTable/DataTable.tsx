import './DataTable.css';
import { useState } from "react";
import React from "react";
import { Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField } from "@mui/material";

interface Column {
    name: string;
    editable: boolean;
}

const DataTable: React.FC = () => {
    const [columns, setColumns] = useState<Column[]>([{ name: "Variable 1", editable: false }]);
    const [rows, setRows] = useState([{ id: 1, data: [''] }]);

    const addColumn = () => {
        setColumns([...columns, { name: `Variable ${columns.length + 1}`, editable: false }]);
        setRows(rows.map(row => ({ ...row, data: [...row.data, ''] })));
    };

    const addRow = () => {
        setRows([...rows, { id: rows.length + 1, data: Array(columns.length).fill('') }]);
    }

    const handleCellChange = (rowIndex: number, colIndex: number, value: string) => {
        const newRows = [...rows];
        newRows[rowIndex].data[colIndex] = value;
        setRows(newRows);
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
        setRows([{ id: 1, data: [''] }]);
        setColumns([{ name: "Variable 1", editable: false }]);
    }

    return (
        <>
            <Button variant="contained" className="button-primary" onClick={addColumn}>Add Column</Button>
            <Button variant="contained" className="button-secondary" onClick={addRow}>Add Row</Button>
            <Button variant="contained" className="button-secondary" onClick={handleClearTable}>Clear Table</Button>
            <TableContainer className="table-container">
                <Table aria-label="simple table">
                    <TableHead>
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
                                <TableCell align="center" className="table-cell">{row.id}</TableCell>
                                {row.data.map((cell, colIndex) => (
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
