"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = __importDefault(require("express"));
const pool_1 = __importDefault(require("../db/pool"));
const router = express_1.default.Router();
router.get("/", (_req, res) => __awaiter(void 0, void 0, void 0, function* () {
    try {
        const result = yield pool_1.default.query("SELECT * FROM tasks");
        res.json(result.rows);
    }
    catch (e) {
        console.log(e);
    }
}));
router.post("/", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { name, draggable_id, droppable_id, start_date, description } = req.body;
    try {
        const result = yield pool_1.default.query("INSERT INTO tasks (name, draggable_id, droppable_id, start_date, description) VALUES ($1, $2, $3, $4, $5) RETURNING *", [name, draggable_id, droppable_id, start_date, description]);
        res.status(200).json(result.rows[0]);
    }
    catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
}));
router.put("/", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { draggable_id, source_droppable_id, destination_droppable_id, start_date } = req.body;
    try {
        const result = yield pool_1.default.query("UPDATE tasks SET droppable_id = $1, start_date = $2 WHERE draggable_id = $3 RETURNING *", [destination_droppable_id, start_date, draggable_id]);
        if (result.rowCount === 0) {
            return res.status(404).json({ message: "Task not found." });
        }
        res.status(200).json({ message: "Task moved successfully.", task: result.rows[0] });
    }
    catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
}));
router.delete("/", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { droppable_id, draggable_id } = req.body;
    try {
        const result = yield pool_1.default.query("DELETE FROM tasks WHERE draggable_id = $1 RETURNING *", [draggable_id]);
        res.json(result.rows);
    }
    catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
}));
exports.default = router;
