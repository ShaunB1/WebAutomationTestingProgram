import { useState } from "react";
import DataTable from "../DataTable/DataTable.tsx";

interface ScriptTable {
    name: string;
}

const ExecuteScripts: React.FC = () => {
    const [tables] = useState<ScriptTable[]>([]);

    return (
        <>
            {tables.map((table, index) => (
                <div key={index}>
                    <DataTable testCaseName={table.name} />
                </div>
            ))}
        </>
    );
}

export default ExecuteScripts;