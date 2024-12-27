import express, { Request, Response } from "express";
import pool from "../db/pool";

const router = express.Router();

router.get("/", async (_req: Request, res: Response) => {
    try {
        const result = await pool.query("SELECT * FROM tasks");
        res.json(result.rows);
    } catch (e) {
        console.log(e);
    }
});

router.post("/", async (req: Request, res: Response) => {
    const { name, draggable_id, droppable_id, start_date, description } = req.body;

    try {
        const result = await pool.query(
            "INSERT INTO tasks (name, draggable_id, droppable_id, start_date, description) VALUES ($1, $2, $3, $4, $5) RETURNING *",
            [name, draggable_id, droppable_id, start_date, description]
        );
        res.status(200).json(result.rows[0]);
    } catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
})

router.put("/", async (req: Request, res: Response): Promise<any> => {
    const { draggable_id, source_droppable_id, destination_droppable_id, start_date } = req.body;

    try {
        const result = await pool.query(
            "UPDATE tasks SET droppable_id = $1, start_date = $2 WHERE draggable_id = $3 RETURNING *",
            [destination_droppable_id, start_date, draggable_id]
        );

        if (result.rowCount === 0) {
            return res.status(404).json({ message: "Task not found." });
        }

        res.status(200).json({ message: "Task moved successfully.", task: result.rows[0] });
    } catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
});

router.delete("/", async (req: Request, res: Response) => {
    const { droppable_id, draggable_id } = req.body;

    try {
        const result = await pool.query(
            "DELETE FROM tasks WHERE draggable_id = $1 RETURNING *",
            [draggable_id]
        );
        res.json(result.rows);
    } catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
});

export default router;