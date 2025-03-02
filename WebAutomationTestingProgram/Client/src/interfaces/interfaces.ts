export interface EnvRowData {
    environment: string,
    ops_bps: string,
    aad: string,
    db_name: string,
}

export interface RowData {
    testcasename: string;
    testdesc: string;
    stepnum: string;
    action: string;
    object: string;
    value: string;
    comments: string;
    release: string;
    attempts: string;
    timeout: string;
    control: string;
    collection: string;
    steptype: string;
    goto: string;
}

export interface TestRun {
    id: string;
    logs: string[];
}

export interface ActiveRun {
    id: string;
}