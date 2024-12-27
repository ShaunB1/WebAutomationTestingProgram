import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Box,
    Button, Collapse, Drawer, IconButton,
    List,
    ListItem, Menu, MenuItem,
    TextField,
    Typography
} from "@mui/material";
import React from "react";
import {useEffect, useState} from "react";
import {DragDropContext, Draggable, Droppable} from "react-beautiful-dnd";
import "./Taskboard.css";
import CheckIcon from "@mui/icons-material/Check";
import DeleteIcon from "@mui/icons-material/Delete";
import MoreVertIcon from '@mui/icons-material/MoreVert';

interface Task {
    draggableId: string;
    name: string;
    startDate: string;
    description: string;
}

interface Worker {
    name: string,
    tasks: Task[],
    droppableId: string,
}

const TaskBoard = () => {
    const [tasks, setTasks] = useState<Task[]>([]);
    const [socket, setSocket] = useState<any>(`ws://${import.meta.env.VITE_HOST}/ws/tasks`);
    const [newTask, setNewTask] = useState<string>("");
    const [workerName, setWorkerName] = useState<string>("");
    const [workers, setWorkers] = useState<Worker[]>([]);
    
    useEffect(() => {
        const fetchData = async () => {
            const [tasksResponse, workersResponse] = await Promise.all([
                fetch(`https://${import.meta.env.VITE_DB_HOST}/api/tasks`),
                fetch(`https://${import.meta.env.VITE_DB_HOST}/api/workers`),
            ])

            const dbTasks: any[] = await tasksResponse.json();
            const dbWorkers: any[] = await workersResponse.json();

            const savedWorkers: Worker[] = []
            dbWorkers.forEach(dbWorker => {
                const tasks: Task[] = [];
                const workerObj: Worker = {
                    name: dbWorker.name,
                    tasks: tasks,
                    droppableId: dbWorker.droppable_id,
                };
                dbTasks.forEach(task => {
                    if (task.droppable_id === dbWorker.droppable_id) {
                        const taskObj: Task = {
                            draggableId: task.draggable_id,
                            name: task.name,
                            startDate: task.start_date,
                            description: task.description,
                        }
                        tasks.push(taskObj);
                    }
                })
                savedWorkers.push(workerObj);
            });

            const savedTasks: Task[] = []
            dbTasks.forEach(task => {
                if (task.droppable_id === "taskList") {
                    const taskObj: Task = {
                        draggableId: task.draggable_id,
                        name: task.name,
                        startDate: task.start_date,
                        description: task.description,
                    }
                    savedTasks.push(taskObj);
                }
            })

            setTasks(savedTasks);
            setWorkers(savedWorkers);
        }

        fetchData();
    }, []);

    const handleDragEnd = async (result: any) => {
        const currentDate = new Date().toLocaleDateString("en-US", {
            year: "numeric",
            month: "long",
            day: "numeric",
        });
        const { source, destination } = result;

        if (!result.destination) return;

        if (source?.droppableId === destination?.droppableId && source?.index === destination?.index) {
            return;
        }

        const moveItem = (srcList: any, destList: any, srcIndex: any, destIndex: any) => {
            const [removed] = srcList.splice(srcIndex, 1);
            removed.startDate = currentDate;
            destList.splice(destIndex, 0, removed);
            return removed;
        }

        const updateTaskWorker = async (draggableId: string, srcDroppableId: string | null, destDroppableId: string | null) => {
            try {
                const taskDate = new Date().toLocaleDateString("en-US", {
                    year: "numeric",
                    month: "long",
                    day: "numeric",
                });

                await fetch(`https://${import.meta.env.VITE_DB_HOST}/api/tasks`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        draggable_id: draggableId,
                        source_droppable_id: srcDroppableId,
                        destination_droppable_id: destDroppableId,
                        start_date: taskDate,
                    }),
                });
            } catch (e) {
                console.log("Failed to update task ownership: ", e);
            }
        }

        // moving tasks within a list
        if (source.droppableId === destination.droppableId) {
            if (source.droppableId === "taskList") {
                const updatedTasks = tasks;
                moveItem(updatedTasks, updatedTasks, source.index, destination.index);
                setTasks(updatedTasks);
            } else {
                const workerTasks = workers.find(worker => worker.droppableId === source.droppableId)?.tasks || [];
                moveItem(workerTasks, workerTasks, source.index, destination.index);
                setWorkers((prevWorkers: any) => prevWorkers.map(
                    (worker: any) => worker.droppableId === source.droppableId ? {...worker, tasks: workerTasks} : worker
                ));
            }
        }

        // moving from task board to worker
        else if (source.droppableId === "taskList") {
            const updatedTasks = Array.from(tasks);
            const worker = workers.find(worker => worker.droppableId === destination.droppableId);

            if (worker) {
                const workerTasks = workers.find(worker => worker.droppableId === destination.droppableId)?.tasks || [];
                const movedTask = moveItem(updatedTasks, workerTasks, source.index, destination.index);

                setTasks(updatedTasks);
                setWorkers((prevWorkers: any) => prevWorkers.map(
                    (worker: any) => worker.droppableId === destination.droppableId
                        ? {...worker, tasks: workerTasks}
                        : worker
                ));

                await updateTaskWorker(movedTask.draggableId, source.droppableId, destination.droppableId);
            }
        }

        // moving from worker to task board
        else if (destination.droppableId === "taskList") {
            const updatedTasks = Array.from(tasks);
            const worker = workers.find(worker => worker.droppableId === source.droppableId);

            if (worker) {
                const workerTasks = workers.find(worker => worker.droppableId === source.droppableId)?.tasks || [];
                const movedTask = moveItem(workerTasks, updatedTasks, source.index, destination.index);

                setTasks(updatedTasks);
                setWorkers((prevWorkers: any) => prevWorkers.map(
                    (worker: any) => worker.droppableId === source.droppableId
                        ? {...worker, tasks: workerTasks}
                        : worker
                ))

                await updateTaskWorker(movedTask.draggableId, source.droppableId, destination.droppableId);
            }
        }

        // moving from worker to worker
        else if (source.droppableId !== "taskList" && destination.droppableId !== "taskList") {
            const workerSrc = workers.find(worker => worker.droppableId === source.droppableId)?.tasks || [];
            const workerDest = workers.find(worker => worker.droppableId === destination.droppableId)?.tasks || [];
            const movedTask = moveItem(workerSrc, workerDest, source.index, destination.index);

            setWorkers((prevWorkers: any) => prevWorkers.map(
                (worker: any) => worker.droppableId === source.droppableId
                    ? {...worker, tasks: workerSrc}
                    : worker
            ))
            setWorkers((prevWorkers: any) => prevWorkers.map(
                (worker: any) => worker.droppableId === destination.droppableId
                    ? {...worker, tasks: workerDest}
                    : worker
            ))

            await updateTaskWorker(movedTask.draggableId, source.droppableId, destination.droppableId);
        }
    }

    const handleAddTask = async () => {
        if (newTask.trim() === "") return;

        const task = { draggableId: Date.now().toString(), name: newTask, startDate: "", description: description };
        const updatedTasks = [...tasks, task];
        setTasks(updatedTasks);
        setNewTask("");
        setDescription("");

        await fetch(`https://${import.meta.env.VITE_DB_HOST}/api/tasks`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: task.name, draggable_id: task.draggableId, droppable_id: "taskList", start_date: "", description: description }),
        });
    }

    const handleAddWorker = async () => {
        if (workerName.trim() === "") return;

        const workerObj: Worker = {
            name: workerName,
            tasks: [],
            droppableId: Date.now().toString(),
        }

        try {
            await fetch(`https://${import.meta.env.VITE_DB_HOST}/api/workers`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name: workerObj.name, droppable_id: workerObj.droppableId }),
            });

            setWorkers(prevWorkers => [...prevWorkers, workerObj]);
            setWorkerName("");
        } catch (e) {
            console.log("Could not add worker: ", e);
        }
    }

    const handleCompleteTask = async (task: Task, worker: Worker) => {
        const draggableId = task.draggableId;
        const droppableId = worker.droppableId;
        const tasks = worker.tasks || [];
        const endDate = new Date().toLocaleDateString("en-US", {
            year: "numeric",
            month: "long",
            day: "numeric",
        });
        const newTasks = tasks.filter(task => task.draggableId !== draggableId);

        setWorkers(prevWorkers => prevWorkers.map(prevWorker => prevWorker.droppableId === worker.droppableId
            ? { ...prevWorker, tasks: newTasks }
            : prevWorker
        ));

        await Promise.all([
            fetch(`https://${import.meta.env.VITE_DB_HOST}/api/completed_tasks`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name: worker.name, task: task.name, start_date: task.startDate, end_date: endDate }),
            }),
            fetch(`https://${import.meta.env.VITE_DB_HOST}/api/tasks`, {
                method: "DELETE",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ droppable_id: droppableId, draggable_id: draggableId }),
            }),
        ]);
    }

    const calculateDays = (startDate: string, endDate: string) => {
        const start = new Date(startDate);
        const end = new Date(endDate);

        const timeDifference = end.getTime() - start.getTime();
        const daysDifference = timeDifference / (1000 * 60 * 60 *24);

        return Math.round(daysDifference);
    }

    const [description, setDescription] = useState("");

    const handleDeleteTask = async (draggableId: string) => {
        await fetch(`https://${import.meta.env.VITE_DB_HOST}/api/tasks`, {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ draggable_id: draggableId }),
        });
        setTasks(prevTasks => prevTasks.filter(task => task.draggableId !== draggableId));
    }

    return (
        <>
            <Box sx={{
                width: "100%",
                mb: 2,
                display: "flex",
                justifyContent: "space-between",
            }}>
                <Box sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 2,
                }}>
                    <TextField
                        label="Enter a worker..."
                        value={workerName}
                        onChange={(e) => setWorkerName(e.target.value)}
                    />
                    <Button variant="contained" onClick={handleAddWorker}>Add Worker</Button>
                </Box>
                <Box sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 2,
                }}>
                    <TextField
                        label="Enter a task..."
                        value={newTask}
                        onChange={(e) => setNewTask(e.target.value)}
                    />
                    <TextField
                        label="Enter a description..."
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                    <Button variant="contained" onClick={handleAddTask}>Add Task</Button>
                </Box>
            </Box>
            <Box display="flex" sx={{ width: "100%", gap: 2}}>
                <DragDropContext onDragEnd={handleDragEnd}>
                    <Box display="flex" sx={{ width: "75%", gap: 2, flexWrap: "wrap" }}>
                        {workers.map((worker, index) => (
                            <Box key={index}  sx={{
                                height: "400px",
                                borderRadius: 2,
                                width: "45%",
                                background: "#F5F5F5",
                            }}>
                                <Typography
                                    variant="h6"
                                    color="textSecondary"
                                    sx={{
                                        width: "100%",
                                        display: "flex",
                                        justifyContent: "center",
                                        alignItems: "center",
                                        background: "#313d4f",
                                        borderRadius: 1,
                                        height: "40px",
                                        color: "white",
                                    }}
                                >
                                    {worker.name}
                                </Typography>
                                <Box sx={{ height: "95%", padding: 1 }}>
                                    <Droppable droppableId={worker.droppableId}>
                                        {(provided: any): any => (
                                            <Box display="flex" sx={{ height: "90%", flexDirection: "column", alignItems: "space-between", justifyContent: "space-between" }}>
                                                <List
                                                    {...provided.droppableProps}
                                                    ref={provided.innerRef}
                                                    style={{listStyle: "none", padding: 0, width: "100%", height: "100%", overflow: "auto"}}
                                                >
                                                    {workers[index].tasks?.map((task: any, index: any) => (
                                                        <Draggable key={task.draggableId} draggableId={task.draggableId} index={index}>
                                                            {(provided) => (
                                                                <ListItem
                                                                    ref={provided.innerRef}
                                                                    {...provided.draggableProps}
                                                                    {...provided.dragHandleProps}
                                                                    style={{
                                                                        padding: '10px 0px',
                                                                        margin: '4px 0',
                                                                        backgroundColor: '#C4DAD2',
                                                                        borderRadius: '4px',
                                                                        ...provided.draggableProps.style,
                                                                    }}
                                                                >
                                                                    <IconButton sx={{ cursor: "grab" }}>
                                                                        <MoreVertIcon />
                                                                    </IconButton>
                                                                    <Box display="flex" sx={{
                                                                        width: "100%",
                                                                        justifyContent: "space-between",
                                                                        alignItems: "center",
                                                                    }}>
                                                                        <Accordion sx={{
                                                                            width: "100%",
                                                                            background: "#f0f0f0",
                                                                        }}>
                                                                            <AccordionSummary>
                                                                                <Typography
                                                                                    sx={{ color: "#333333" }}
                                                                                >
                                                                                    {task.name}
                                                                                </Typography>
                                                                            </AccordionSummary>
                                                                            <AccordionDetails>
                                                                                <Typography>{task.description}</Typography>
                                                                            </AccordionDetails>
                                                                        </Accordion>
                                                                        <Box display="flex" sx={{ alignItems: "center", gap: 2, ml: 2 }}>
                                                                            <Typography style={{ width: "50px", display: "flex", justifyContent: "center", color: "#333333"}}>
                                                                                {(() => {
                                                                                    const currentDate = new Date().toLocaleDateString("en-US", {
                                                                                        year: "numeric",
                                                                                        month: "long",
                                                                                        day: "numeric",
                                                                                    });
                                                                                    const days = calculateDays(task.startDate, currentDate);
                                                                                    return `Day ${days}`
                                                                                })()}
                                                                            </Typography>
                                                                            <IconButton color="primary" onClick={() => handleCompleteTask(task, worker)}>
                                                                                <CheckIcon />
                                                                            </IconButton>
                                                                        </Box>
                                                                    </Box>
                                                                </ListItem>
                                                            )}
                                                        </Draggable>
                                                    ))}
                                                    {provided.placeholder}
                                                </List>
                                            </Box>
                                        )}
                                    </Droppable>
                                </Box>
                            </Box>
                        ))}
                    </Box>
                    <Box sx={{
                        width: "25%",
                        borderRadius: 2,
                        display: "flex",
                        flexWrap: "wrap",
                        height: "75vh",
                        position: "fixed",
                        right: 20,
                        background: "#F5F5F5",
                    }}>
                        <Typography variant="h6" color="textSecondary"
                            sx={{
                                display: "flex",
                                justifyContent: "center",
                                alignItems: "center",
                                width: "100%",
                                borderRadius: 2,
                                background: "#313d4f",
                                height: "40px",
                                color: "white",
                            }}
                        >
                            Task Board
                        </Typography>
                        <Box sx={{ height: "100%", width: "100%", padding: 1 }}>
                            <Droppable droppableId={"taskList"}>
                                {(provided: any): any => (
                                    <List
                                        {...provided.droppableProps}
                                        ref={provided.innerRef}
                                        style={{listStyle: "none", padding: 0, width: "100%", height: "92%", overflow: "auto"}}
                                    >
                                        {tasks.map((task: any, index: any) => (
                                            <Draggable key={task.draggableId} draggableId={task.draggableId} index={index}>
                                                {(provided) => (
                                                    <ListItem
                                                        ref={provided.innerRef}
                                                        {...provided.draggableProps}
                                                        {...provided.dragHandleProps}
                                                        sx={{
                                                            padding: '10px 0',
                                                            margin: '4px 0',
                                                            backgroundColor: '#C4DAD2',
                                                            borderRadius: '4px',
                                                            ...provided.draggableProps.style,
                                                        }}
                                                    >
                                                        <IconButton sx={{ cursor: "grab" }}>
                                                            <MoreVertIcon />
                                                        </IconButton>
                                                        <Accordion sx={{
                                                            width: "90%",
                                                            background: "#f0f0f0",
                                                        }}>
                                                            <AccordionSummary>
                                                                <Typography
                                                                    sx={{ color: "#333333" }}
                                                                >
                                                                    {task.name}
                                                                </Typography>
                                                            </AccordionSummary>
                                                            <AccordionDetails>
                                                                <Typography>{task.description}</Typography>
                                                            </AccordionDetails>
                                                        </Accordion>
                                                        <IconButton onClick={() => handleDeleteTask(task.draggableId)}>
                                                            <DeleteIcon />
                                                        </IconButton>
                                                    </ListItem>
                                                )}
                                            </Draggable>
                                        ))}
                                        {provided.placeholder}
                                    </List>
                                )}
                            </Droppable>
                        </Box>
                    </Box>
                </DragDropContext>
            </Box>
        </>
    );
}

export default TaskBoard;