import React from "react";
import FileUpload from "../Components/FileUpload/FileUpload.tsx";
import LogDisplay from "../Components/LogDisplay/LogDisplay.tsx";

const Home: React.FC = () => {
    return (
        <>
            <FileUpload />
            {/*<DataTable />*/}
            <LogDisplay />
        </>
    );
}

export default Home;