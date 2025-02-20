import { useEffect, useState } from "react";
import { FilterModel } from "@interfaces/interfaces";
import { Button } from "@mui/material";
import './InfoContainer.css';

const InfoContainer = (props: any) => {
    const [filterState, setFilterState] = useState<FilterModel>({
        env: "",
        app: "",
        role: "",
        account: "",
        org: ""
    });
    const [selectedCell, setSelectedCell] = useState<string>('');

    useEffect(() => {
        if (!props.gridApi) return;

        props.gridApi.addEventListener('cellClicked', (event: any) => setSelectedCell(event.value));
        props.gridApi.addEventListener('filterChanged', updateFilterState);

        return () => {
            props.gridApi.removeEventListener('cellClicked');
            props.gridApi.removeEventListener('filterChanged');
        }
    }, [props.gridApi]);

    const updateFilterState = () => {
        if (!props.gridApi) return;

        const filterModel = props.gridApi.getFilterModel();
        setFilterState((prevFilterState: FilterModel) => ({
            ...prevFilterState,
            ...filterModel
        }));
    };

    const handleResetFilter = (e: any) => {
        setFilterState({
            env: "",
            app: "",
            role: "",
            account: "",
            org: ""
        });
        props.handleResetFilter();
    }

    return (
        <div className="info-container">
            <div className="info-container-column">
                <span className="info-text">
                    <strong>Env (required):</strong> {filterState.env}
                </span>
                <span className="info-text">
                    <strong>App:</strong> {filterState.app}
                </span>
                <span className="info-text">
                    <strong>Role:</strong> {filterState.role}
                </span>
                <span className="info-text">
                    <strong>Account:</strong> {filterState.account}
                </span>
                <span className="info-text">
                    <strong>Orgs:</strong> {filterState.org}
                </span>
                <Button variant='contained' size='small' sx={{ marginTop: '5px' }} onClick={handleResetFilter}>Reset Filters</Button>
            </div>
            <div className="info-container-column">
                <span className="info-text info-container-row">
                    <strong>Selected cell:</strong> {selectedCell}
                </span>
                {props.errorMessage && (
                    <span className="info-text info-container-row" style={{ color: "red" }}>
                        Error: {props.errorMessage}
                    </span>
                )}

            </div>
        </div>
    );
}

export default InfoContainer;