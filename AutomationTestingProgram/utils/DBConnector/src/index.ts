import express from "express";
import cors from 'cors';
import bodyParser from "body-parser";
import fs from "fs";

import taskRoutes from "./routes/tasks";
import workerRoutes from "./routes/workers";
import completedTaskRoutes from "./routes/completed_tasks";
import * as https from "node:https";

const app = express();

app.use(cors());
app.use(bodyParser.json());

app.use("/api/tasks", taskRoutes);
app.use("/api/workers", workerRoutes);
app.use("/api/completed_tasks", completedTaskRoutes);

const PORT = parseInt(process.env.PORT || "5000", 10);
const options = {
    key: fs.readFileSync("../private.key"),
    cert: fs.readFileSync("../certificate.crt"),
}

https.createServer(options, app).listen(PORT, "0.0.0.0", () => {
    console.log(`Server running on port ${PORT}`);
})
