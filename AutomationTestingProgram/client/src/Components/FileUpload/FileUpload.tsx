import {useState} from "react";


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
            <h1>Upload Excel File</h1>
            <form onSubmit={handleSubmit}>
                <input type="file" accept={".xlsx, .xls"} onChange={handleFileChange} />
                <button type="submit">Upload</button>
            </form>
        </>
    );
}

export default FileUpload;