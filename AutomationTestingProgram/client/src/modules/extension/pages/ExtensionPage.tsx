
import { useMsal } from "@azure/msal-react";
import { getToken } from "@/auth/authConfig";
import { Button } from "@mui/material";
import DownloadIcon from '@mui/icons-material/Download';

const ExtensionPage = () => {
    const { instance, accounts } = useMsal();

    const handleDownloadExtension = async () => {
        try {
            const token = await getToken(instance, accounts);
            const headers = new Headers();
            headers.append("Authorization", `Bearer ${token}`);
            const response = await fetch("/api/extension/download-zip", {
                method: "GET",
                headers: headers,
            });
            if (!response.ok) {
                const errorDetails = await response.json(); 
                throw new Error(
                    `${response.status} - ${response.statusText}\nMessage: ${errorDetails.Message}\nStackTrace: ${errorDetails.StackTrace}`
                );
            }
            const blob = await response.blob();
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = "TAP_Extension.zip";
            link.click();
            URL.revokeObjectURL(link.href);
        } catch (err) {
            console.error(err);
            alert("Failed to download extension");
        }
    }

    return (
        <>
            <h1>Extension</h1>
            <Button onClick={handleDownloadExtension} color="primary" variant="contained" endIcon={<DownloadIcon />}>Download Extension</Button>
        </>
    );
};

export default ExtensionPage;