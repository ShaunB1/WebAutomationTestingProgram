import {useState} from "react";
import {Box, Button, Stack, Typography} from "@mui/material";


const FileUpload = () => {
    const [file, setFile] = useState<File | null>(null);

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (event.target.files && event.target.files.length > 0) {
            setFile(event.target.files[0]);
        }
    }

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        if (!file) {
            alert("Please select a file to upload.");
            return;
        }

        const formData = new FormData();
        formData.append("file", file);

        try {
            const res = await fetch("/api/test/run", {
                method: "POST",
                body: formData,
            });

            if (res.ok) {
                alert("File uploaded successfully!");
            } else {
                alert("Failed to upload file.")
            }

        } catch (e) {
            console.error("Error uploading file: ", e);
            alert("An error occurred while uploading the file.");
        }
    }

    return (
        <>
            <Box>
                <Typography variant="h4" color="textSecondary" gutterBottom>
                    Upload Excel File
                </Typography>
                <Box component="form" onSubmit={handleSubmit} display="flex" flexDirection={"column"} alignItems="center">
                    <Stack direction={"row"} alignItems={"center"} spacing={2}>
                        <Button variant={"contained"} color={"primary"} component={"label"}>
                            Choose File
                            <input
                                type='file'
                                accept={".xlsx, .xls"}
                                hidden
                                onChange={handleFileChange}
                            />
                        </Button>
                        <Typography variant={"body2"} color={"textSecondary"}>
                            {file ? file.name : "No file chosen"}
                        </Typography>
                    </Stack>
                    <Button
                        variant="contained"
                        color={"secondary"}
                        type={"submit"}
                        sx={{ mt: 2 }}
                    >
                        Upload
                    </Button>
                </Box>
            </Box>
        </>
    );
}

export default FileUpload;