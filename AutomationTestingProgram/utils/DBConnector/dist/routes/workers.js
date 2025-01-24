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
        const result = yield pool_1.default.query("SELECT * FROM workers");
        res.json(result.rows);
    }
    catch (e) {
        console.log(e);
        res.status(500).json("Server error.");
    }
}));
router.post("/", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    try {
        const { name, droppable_id } = req.body;
        const result = yield pool_1.default.query("INSERT INTO workers (name, droppable_id) VALUES ($1, $2) RETURNING *", [name, droppable_id]);
        res.status(201).json(result.rows[0]);
    }
    catch (e) {
        console.log(e);
        res.status(500).send("Server Error");
    }
}));
router.delete("/", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    try {
        const { name } = req.body;
        const result = yield pool_1.default.query("DELETE FROM workers WHERE name = $1 RETURNING *", [name]);
        res.json(result.rows);
    }
    catch (e) {
    }
}));
exports.default = router;
