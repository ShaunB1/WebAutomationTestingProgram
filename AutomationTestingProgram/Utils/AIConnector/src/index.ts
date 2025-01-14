import express from "express";
import cors from "cors";
import * as fs from "node:fs";

import aiRoutes from "./routes/ai";

const app = express();

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use("/api/ai", aiRoutes);

const PORT = parseInt(process.env.PORT as string, 10) || 5100;
const options = {
    key: fs.readFileSync("../private.key"),
    cert: fs.readFileSync("../certificate.crt"),
}

app.listen(PORT, "0.0.0.0", () => console.log(`Server running on port ${PORT}`));