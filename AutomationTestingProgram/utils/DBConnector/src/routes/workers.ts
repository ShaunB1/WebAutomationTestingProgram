import express, { Request, Response } from "express";
import pool from "../db/pool";

const router = express.Router();

router.get("/", async (_req: Request, res: Response) => {
    try {
        const result = await pool.query("SELECT * FROM workers");
        res.json(result.rows);
    } catch (e) {
        console.log(e);
        res.status(500).json("Server error.");
    }
});

router.post("/", async (req: Request, res: Response) => {
    try {
        const { name, droppable_id } = req.body;
        const result = await pool.query(
            "INSERT INTO workers (name, droppable_id) VALUES ($1, $2) RETURNING *",
            [name, droppable_id]
        );
        res.status(201).json(result.rows[0]);
    } catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
});

router.delete("/", async (req: Request, res: Response) => {
    try {
        const { name } = req.body;
        const result = await pool.query(
            "DELETE FROM workers WHERE name = $1 RETURNING *",
            [name]
        );
        res.json(result.rows);
    } catch (e) {
        
    }
});

export default router;