import { memo, useCallback, useEffect, useState } from "react";
import { CustomFilterProps, useGridFilter } from "ag-grid-react";
import { Autocomplete, TextField } from '@mui/material';

interface CustomFilterWithProps extends CustomFilterProps {
    resetFilters: boolean,
    forceUpdate: number
}

const CustomFilter = ({ model, onModelChange, api, column, resetFilters }: CustomFilterWithProps) => {
    const [options, setOptions] = useState<string[]>([]);
    const [selectedValue, setSelectedValue] = useState<string | null>(null);
    const [inputValue, setInputValue] = useState<string>('');

    useEffect(() => {
        const key = column.getColId();
        chrome.storage.local.get(key, (result) => {
            if (result[key]) {
                setSelectedValue(result[key]);
                onModelChange(result[key]);
            }
        });
    }, [column]);

    useEffect(() => {
        if (!api) return;

        const updateFilterOptions = () => {
            const uniqueValues: Set<string> = new Set();
            const key = column.getColId();

            api.forEachNodeAfterFilter((node) => {
                const value = node.data[key];
                if (value) {
                    value.split(',').forEach((item: string) => uniqueValues.add(item.trim()));
                }
            });

            setOptions(Array.from(uniqueValues).sort());
        };

        updateFilterOptions();

        api.addEventListener('filterChanged', updateFilterOptions);

        return () => {
            api.removeEventListener('filterChanged', updateFilterOptions);
        };
    }, [api, column]);

    useEffect(() => {
        if (resetFilters) {
            chrome.storage.local.set({ [column.getColId()]: null });
            setSelectedValue(null);
        }
    }, [resetFilters]);

    const handleValueChange = (_e: any, newValue: string | null) => {
        setSelectedValue(newValue);
        const key = column.getColId();
        chrome.storage.local.set({ [key]: newValue });
        onModelChange(newValue);
    };

    const doesFilterPass = useCallback((params: any) => {
        const { data } = params;
        const key = column.getColId();
        if (key === 'env' || key === 'org') {
            return data[key].includes(model);
        }
        return data[key] === model;
    }, [model]);

    useGridFilter({
        doesFilterPass,
    });

    return (
        <div style={{ padding: 10 }}>
            <Autocomplete
                sx={{
                    width: 200,
                    maxLines: 1,
                }}
                value={selectedValue}
                onChange={(e: any, newValue: string | null) => handleValueChange(e, newValue)}
                inputValue={inputValue}
                onInputChange={(_e, newInputValue: string) => {
                    setInputValue(newInputValue);
                }}
                slotProps={{
                    popper: {
                        className: 'ag-custom-component-popup',
                    },
                }}
                options={options}
                renderInput={(params) => <TextField {...params} label="Filter" />}
            />
        </div>
    );
}

export default CustomFilter;