import TableRenderers from "react-pivottable/TableRenderers";
import {useState} from "react";
import Papa from "papaparse";
import PivotTableUI from "react-pivottable/PivotTableUI";
import {TableContainer, Paper} from "@mui/material";
import "./PivotTablePage.css";

const PivotTablePage = () => {
    const [state, setState] = useState({
        data: [],
        renderers: TableRenderers,
        aggregatorName: "Count",
        cols: [],
        rows: [],
        vals: [],
        rendererName: "Table"
    });

    const handleFileChange = (event: any) => {
        const file = event.target.files[0];
        Papa.parse(file, {
            download: true,
            header: false,
            skipEmptyLines: true,
            complete: (results) => {
                const data = results.data.slice(1);
                setState((prevState: any) => ({
                    ...prevState,
                    data
                }));
            }
        });
    }

    return (
        <>
            <input type="file" onChange={handleFileChange} />
            <TableContainer component={Paper} sx={{ width: "100%", overflow: "auto" }}>
                <PivotTableUI
                    {...state}
                    onChange={(s: any) => setState(s)}
                    renderers={TableRenderers}
                />
            </TableContainer>
        </>
    );
};

export default PivotTablePage;