export interface Constants {
    caseName: string,
    release: string,
    collection: string,
    stepType: string,
}

export interface ActionDetails {
    [key: string]: any;
    caseName: string,
    desc: string,
    stepNum: number,
    action: string,
    object: string,
    value: string,
    comments: string,
    release: string,
    attempts: string,
    timeout: string,
    control: string,
    collection: string,
    stepType: string,
    goto: string,
    uniqueLocator: boolean,
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