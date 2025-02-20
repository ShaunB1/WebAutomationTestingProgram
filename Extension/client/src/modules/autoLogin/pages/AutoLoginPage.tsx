import { useEffect, useState, useMemo } from "react";
import { AgGridReact } from 'ag-grid-react';
import { ColDef, GridApi } from "ag-grid-community";
import { Button, CircularProgress } from "@mui/material";
import { HOST } from "@/constants";
import { getToken } from "@auth/authConfig";
import { useMsal } from "@azure/msal-react";
import CustomFilter from "../components/CustomFilter/CustomFilter";
import env_data from "@assets/environment_list.json";
import InfoContainer from "../components/InfoContainer/InfoContainer";

const AutoLoginPage = (props: any) => {
    // const [rowData, setRowData] = useState([]);
    const [gridLoading, setGridLoading] = useState<boolean>(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [gridApi, setGridApi] = useState<GridApi | null>(null);
    const [resetFilters, setResetFilters] = useState<boolean>(false);
    const { instance, accounts } = useMsal();

    // useEffect(() => {
    //     const fetchRows = async () => {
    //         try {
    //             setLoading(true);
    //             await instance.initialize();
    //             const headers = new Headers();
    //             const token = await getToken(instance, accounts);
    //             headers.append("Authorization", `Bearer ${token}`);
    //             const response = await fetch(`${HOST}/api/environments/keychainAccounts`, {
    //                 method: 'GET',
    //                 headers: headers
    //             });
    //             setLoading(false);
    //             if (!response.ok) {
    //                 const errorData = await response.json();
    //                 throw new Error(`${errorData.error}`);
    //             }
    //             const result = await response.json();
    //             setRowData(result.result);
    //         } catch (err) {
    //             console.error(err);
    //         }
    //     };

    //     fetchRows();
    // }, [instance, accounts]);

    useEffect(() => {
        if (!gridApi) return;
        ['env', 'app', 'role', 'account', 'org'].forEach((key: string) => {
            chrome.storage.local.get(key, (result) => {
                if (result[key]) {
                    gridApi.setFilterModel({
                        [key]: {
                            type: key === 'env' || key === 'org' ? 'contains' : 'equals',
                            filter: result[key]
                        }
                    });
                }
            });
        });
    }, [gridApi]);

    const handleClick = async (account: string, env: string, setLoading: (value: boolean) => void, setError: (value: boolean) => void, setSuccess: (value: boolean) => void) => {
        try {
            setLoading(true);
            setError(false);
            setSuccess(false);
            setErrorMessage(null);


            var url = env_data.find(item => item.ENVIRONMENT === env)?.URL;
            if (!url) {
                if (!gridApi) return;

                const filterModel = gridApi.getFilterModel();
                const env2 = filterModel['env'];
                if (!env2 || env2 === '') {
                    if (env.includes(',')) {
                        throw new Error('Environment is ambiguous, select an environment from the filter');
                    } else {
                        throw new Error('URL for environment was not found');
                    }
                }
                url = env_data.find(item => item.ENVIRONMENT === env2)?.URL;
                if (!url) {
                    throw new Error('URL for environment was not found');
                }
            }

            const headers = new Headers();
            const token = await getToken(instance, accounts);
            headers.append("Authorization", `Bearer ${token}`);
            const response = await fetch(`${HOST}/api/environments/secretKey?email=${encodeURIComponent(account.trim())}`, {
                method: 'GET',
                headers: headers
            });

            if (!response.ok) {
                const errorData = await response.json();
                console.log(errorData);
                throw new Error('Error fetching secret key from Azure Key Vault');
            }
            const result = await response.json();
            chrome.runtime.sendMessage(
                {
                    action: "openTabAndLogin",
                    url: url,
                    username: account,
                    password: result.result
                }
            );

            setLoading(false);
            setError(false);
            setSuccess(true);
        } catch (err: any) {
            setLoading(false);
            setError(true);
            setErrorMessage(err.message);
            console.error(err);
        }
    }

    const LoginCellRenderer = (params: any) => {
        const [loading, setLoading] = useState<boolean>(false);
        const [error, setError] = useState<boolean>(false);
        const [success, setSuccess] = useState<boolean>(false);

        return <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
            {loading ? (
                <CircularProgress size={24} />
            ) : (
                <Button
                    variant="contained"
                    color={success ? 'success' : (error ? 'error' : 'primary')}
                    onClick={() => handleClick(params.data.account, params.data.env, setLoading, setError, setSuccess)}
                >
                    {error ? 'Error' : 'Login'}
                </Button>
            )}
        </div>
    };

    // dummy data for testing
    const rowData = useMemo<Object[]>(() => [
        {
            env: "EDCS-9",
            app: "CPA",
            account: "qacpaminadmin@ontarioemail.ca",
            role: "minadmin",
            org: "999999"
        },
        {
            env: "EDCS-9",
            app: "CPA",
            account: "qacpaorgadmin@ontarioemail.ca",
            role: "orgadmin",
            org: "ALGO, BORE"
        },
        {
            env: "EDCS-9",
            app: "CSER",
            account: "qacserminadmin@ontarioemail.ca",
            role: "minadmin",
            org: "999999"
        },
        {
            env: "PR-9",
            app: "CCE",
            account: "cce_col_min_a@ontarioemail.ca",
            role: "minadmin",
            org: "999999"
        },
        {
            env: "PR-8, PR-9",
            app: "CCE",
            account: "cce_col_min_a@ontarioemail.ca",
            role: "minadmin",
            org: "999999"
        },
        {
            env: "testing",
            app: "fake_app",
            account: "fake_account",
            role: "fake_role",
            org: "123456"
        },
    ], []);

    const columnDefs: ColDef[] = [
        { field: "env", headerName: "Env" },
        { field: "app", headerName: "App" },
        { field: "role", headerName: "Role" },
        { field: "account", headerName: "Account" },
        { field: "org", headerName: "Org" },
        {
            field: "login", headerName: "Login", filter: false, sortable: false,
            cellRenderer: LoginCellRenderer, cellStyle: { display: 'flex', justifyContent: 'center', alignItems: 'center' },
        }
    ];

    const handleResetFilter = () => {
        if (!gridApi) return;
        gridApi.setFilterModel(null);
        setResetFilters(prevResetFilters => !prevResetFilters);
    };

    return (
        <>
            <InfoContainer gridApi={gridApi} errorMessage={errorMessage} handleResetFilter={handleResetFilter} />
            <div className="ag-theme-quartz" style={{ width: "100%", height: 600, marginTop: 10 }}  >
                <AgGridReact
                    loading={gridLoading}
                    rowData={rowData}
                    columnDefs={columnDefs}
                    defaultColDef={{
                        filter: CustomFilter,
                        filterParams: {
                            resetFilters: resetFilters
                        },
                        flex: 1
                    }}
                    rowHeight={50}
                    enableCellTextSelection={true}
                    onGridReady={(params: any) => setGridApi(params.api)}
                />
            </div>
        </>
    );
}

export default AutoLoginPage;