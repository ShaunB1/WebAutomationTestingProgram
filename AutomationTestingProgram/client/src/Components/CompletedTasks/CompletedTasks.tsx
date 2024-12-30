import {useEffect, useState} from "react";
import {Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow} from "@mui/material";
import {AgGridReact} from "ag-grid-react";
import {ColDef} from "ag-grid-community";

interface Task {
    task: string,
    start_date: string,
    end_date: string,
    days: string,
    worker: string,
}

const CompletedTasks = () => {
    const [colDefs] = useState<ColDef[]>([
        { field: "task", headerName: "Task", sortable: true, filter: true },
        { field: "start_date", headerName: "Start Date", sortable: true, filter: true },
        { field: "end_date", headerName: "End Date", sortable: true, filter: true },
        { field: "days", headerName: "Days Taken", sortable: true, filter: true },
        { field: "worker", headerName: "Worker", sortable: true, filter: true },
    ]);
    const [completedTasks, setCompletedTasks] = useState<Task[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            const completedTasks = await fetch(`https://${import.meta.env.VITE_DB_HOST}/api/completed_tasks`);

            const dbTasks = await completedTasks.json();
            dbTasks.forEach((task: any) => {
                task.days = calculateDays(task.start_date, task.end_date);
            }); 1290

            setCompletedTasks(dbTasks);
        }

        fetchData();
    }, []);

    const calculateDays = (startDate: string, endDate: string) => {
        const start = new Date(startDate);
        const end = new Date(endDate);

        const timeDifference = end.getTime() - start.getTime();
        const daysDifference = timeDifference / (1000 * 60 * 60 *24);

        return Math.round(daysDifference);
    }

    return (
        <>
            <Box className="ag-theme-quartz" sx={{ width: "100%", height: "90vh" }}>
                <AgGridReact
                    rowData={completedTasks}
                    columnDefs={colDefs}
                    domLayout="normal"
                    onGridReady={(params) => {
                        params.api.sizeColumnsToFit();
                    }}
                />
            </Box>
        </>
    )
}

export default CompletedTasks;