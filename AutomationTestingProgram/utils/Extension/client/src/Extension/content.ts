let verifyMode = "availability";

interface TableValues {
    testdescription: string;
    actiononobject: string;
    object: string;
    value: string;
    comments: string;
}

function clickEventListener(e: Event) {
    const element = e.target as HTMLElement;
    const tag = element.tagName.toLowerCase();
    const elDict = getAllAttributes(e.target as HTMLElement);
    const values: TableValues = {
        testdescription: "",
        actiononobject: "",
        object: elDict,
        value: "",
        comments: "",
    }

    if ((tag === "input" && (element as HTMLInputElement).type === "text") || tag === "textarea") {
        element.removeEventListener("blur", blurEventListener);
        element.removeEventListener("keydown", triggerBlur);

        element.addEventListener("blur", blurEventListener);
        element.addEventListener("keydown", triggerBlur);
    } else if (tag === "select") {
        values.testdescription = "Select option ";
        values.actiononobject = "SelectDDL";

        element.removeEventListener("change", changeEventListener);
        element.addEventListener("change", changeEventListener);
    } else if (tag === "input" && (element as HTMLInputElement).type === "file") {
        const fileInput = e.target as HTMLInputElement;

        values.actiononobject = "UploadFile";

        fileInput.removeEventListener("change", fileChangeListener);
        fileInput.addEventListener("change", fileChangeListener);

        function fileChangeListener() {
            const files = fileInput.files;
            const fileNum = files ? files.length : 0;

            if (fileNum > 0) {
                const file = files ? files[0] : null;
                values.value = (file as unknown as HTMLInputElement).name;
            } else {
                values.value = "No file selected.";
            }

            values.testdescription = `Upload ${values.value}`;

            fileInput.removeEventListener("change", fileChangeListener);
            chrome.runtime.sendMessage({ action: "actiononobject", locator: elDict, stepValues: values });
        }
    }

    else {
        const text = getTextContent(element);
        values.actiononobject = "ClickWebElement";
        values.testdescription = text !== "" ? `Click ${text}` : `Click ${tag}`;
        chrome.runtime.sendMessage({ action: "actiononobject", locator: elDict, stepValues: values });
    }
}

function changeEventListener(e: Event) {
    const element = e.target as HTMLSelectElement;
    const target = e.target as HTMLSelectElement;
    const elDict = getAllAttributes(target as HTMLElement);
    const selectedOption = element.options[target.selectedIndex];
    const text = selectedOption.text;
    const values: TableValues = {
        testdescription: `Select option ${text}`,
        actiononobject: "SelectDDL",
        object: elDict,
        value: text,
        comments: "",
    }

    chrome.runtime.sendMessage({ action: "actiononobject", locator: elDict, stepValues: values });
}

function triggerBlur(e: KeyboardEvent) {
    const key = e.key;
    const element = e.target as HTMLElement;
    if (key === "Enter") {
        element.blur();
    }
}

function blurEventListener(e: Event) {
    const element = e.target as HTMLElement;
    const tag = element.tagName.toLowerCase();
    const textContent = (element as HTMLInputElement).value;
    const elDict = getAllAttributes(e.target as HTMLElement);
    const values: TableValues = {
        testdescription: `Populate ${tag}`,
        actiononobject: "PopulateWebElement",
        object: elDict,
        value: textContent,
        comments: "",
    }

    chrome.runtime.sendMessage({ action: "actiononobject", locator: elDict, stepValues: values });
}

function contextMenuEventListener(e: Event) {
    e.preventDefault();
    const interactiveElements = ["button", "input", "select", "textarea", "fieldset", "optgroup", "option"];
    const elementTag = (e.target as HTMLElement).tagName.toLowerCase();
    const elDict = getAllAttributes(e.target as HTMLElement);
    const element = e.target as HTMLElement;
    const isDisabled = (element as HTMLInputElement).disabled;
    const isReadOnly = (element as HTMLInputElement).readOnly;
    const values: TableValues = {
        testdescription: "",
        actiononobject: "",
        object: elDict,
        value: "",
        comments: ""
    }

    if (verifyMode === "availability") {
        values.actiononobject = "VerifyWebElementAvailability";
        if (interactiveElements.includes(elementTag)) {
            if (isDisabled || isReadOnly) {
                values.value = "DISABLED";
            } else {
                values.value = "ENABLED";
            }
        } else {
            values.value = "EXISTS";
        }

        values.testdescription = `Verify ${elementTag} ${values.value}`;

        chrome.runtime.sendMessage({ action: "actiononobject", locator: elDict, stepValues: values });
    } else if (verifyMode === "content") {
        values.actiononobject = "VerifyWebElementContent";
        const element = e.target as HTMLElement;
        const text = getTextContent(element);

        values.value = text;
        values.testdescription = `Verify ${elementTag} contains ${text}`;

        chrome.runtime.sendMessage({ action: "actiononobject", locator: elDict, stepValues: values });
    }
}

chrome.runtime.onMessage.addListener((message) => {
    if (message.action === "ALT_VERIFY") {
        if (message.changeMode) {
            verifyMode = "content";
        } else {
            verifyMode = "availability";
        }
    } else if (message.action === "FILL_TEXT_BOXES") {
        const inputElements = document.querySelectorAll("input");
        const textareaElements = document.querySelectorAll("textarea");

        inputElements.forEach((input: HTMLInputElement) => {
            if (input.type.toLowerCase() === "text" && !input.disabled && !input.readOnly) {
                input.value = message.fillerText;

                const inputEvent = new Event("input", { bubbles: true });
                input.dispatchEvent(inputEvent);
            }
        });

        textareaElements.forEach((textarea: HTMLTextAreaElement) => {
            if (!textarea.disabled && !textarea.readOnly) {
                textarea.value = message.fillerText;

                const textareaEvent = new Event("input", { bubbles: true });
                textarea.dispatchEvent(textareaEvent);
            }
        });
    } else if (message.action === "CHANGE_START_STATE") {
        if (!message.start) {
            document.removeEventListener("click", clickEventListener);
            document.removeEventListener("contextmenu", contextMenuEventListener);
        } else {
            document.addEventListener("click", clickEventListener);
            document.addEventListener("contextmenu", contextMenuEventListener);
        }
    }
})

function getTextContent(element: HTMLElement) {
    const interactiveElements = ["button", "input", "select", "textarea", "fieldset", "optgroup", "option"];
    const tag = element.tagName.toLowerCase();
    let text = "";

    if (interactiveElements.includes(tag)) {
        text = (element as HTMLInputElement).value;
    } else {
        element.childNodes.forEach((node) => {
            if (node.nodeType === Node.TEXT_NODE) {
                text += node.textContent?.trim() || "";
            }
        });
    }

    return text;
}

function getAllAttributes(element: HTMLElement) {
    const attributesDict: { [key: string]: string | string[] | { [key: string]: string | string[] } } = {};

    attributesDict["tag"] = element.tagName.toLowerCase();
    attributesDict["text"] = getTextContent(element);
    attributesDict["attributes"] = {};

    for (let i = 0; i < element.attributes.length; i++) {
        const attr = element.attributes[i];
        const attrName = attr.name;
        const attrValue = attr.value;

        if (attrValue.includes(" ")) {
            attributesDict["attributes"][attrName] = attrValue.split(" ").filter(Boolean);
        } else {
            attributesDict["attributes"][attrName] = attrValue;
        }
    }

    return JSON.stringify(attributesDict, null, 0);
}