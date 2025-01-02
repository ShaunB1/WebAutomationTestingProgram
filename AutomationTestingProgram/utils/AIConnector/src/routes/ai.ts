import express, { Request, Response } from "express";
import OpenAI from "openai";

const router = express.Router();
const openai = new OpenAI({
    apiKey: "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",
});

router.get("/", async (req: Request, res: Response) => {

});

router.post("/", async (req: Request, res: Response) => {
    try {
        const { prompt } = req.body;
        const completion = openai.chat.completions.create({
            model: "gpt-4o-mini",
            store: true,
            messages: [
                {"role": "user", "content": `${prompt}`},
            ],
        });

        completion.then((result) => {
            console.log(result);
            res.json(result.choices[0].message);
        });
    } catch (e) {
        console.log(e);
    }
})

export default router;