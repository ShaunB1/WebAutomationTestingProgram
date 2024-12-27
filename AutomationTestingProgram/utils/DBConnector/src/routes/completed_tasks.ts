import express, { Request, Response } from 'express';
import pool from "../db/pool";

const router = express.Router();

router.get("/", async (_req: Request, res: Response) => {
    try {
        const result = await pool.query("SELECT * FROM completed_tasks");
        res.json(result.rows);
    } catch (e) {
        console.log(e);
        res.status(500).json("Server error.");
    }
});

router.post("/", async (req: Request, res: Response) => {
    const { name, task, start_date, end_date } = req.body;

    try {
        const result = await pool.query(
            "INSERT INTO completed_tasks (worker, task, start_date, end_date) VALUES ($1, $2, $3, $4) RETURNING *",
            [name, task, start_date, end_date]
        );
        res.json(result.rows);
    } catch (e) {
        console.log(e);
        res.status(500).json("Server error.");
    }
});

export default router;