let verifyMode = "availability";
let previouslySelectedElement: any = null;
let previousOutlineStyle: any = null;
let startState: boolean = false;
let prevMouseOverOutline: any = null;

interface TableValues {
    testdescription: string;
    actiononobject: string;
    object: string;
    value: string;
    comments: string;
}

const START_KEY = "startState";

chrome.storage.onChanged.addListener((changes, namespace) => {
    if (changes[START_KEY] && namespace === "local") {
        startState = changes[START_KEY].newValue;

        if (startState) {
            document.addEventListener("click", clickEventListener);
            document.addEventListener("contextmenu", contextMenuEventListener);
        } else {
            document.removeEventListener("click", clickEventListener);
            document.removeEventListener("contextmenu", contextMenuEventListener);
        }
    }
})

chrome.storage.local.get(START_KEY, (result) => {
    if (result[START_KEY]) {
        startState = result[START_KEY];
        if (startState) {
            document.addEventListener("click", clickEventListener);
            document.addEventListener("contextmenu", contextMenuEventListener);
        }
    }
});

function getElementDetails(e: Event) {
    const element = e.target as HTMLElement;
    const attributes = Array.from(element.attributes).map((attr) => ({
        ATTRIBUTE: attr.name,
        VALUE: attr.value,
    }));

    chrome.runtime.sendMessage({ action: "sendElementDetails", attributes: attributes });
}

document.addEventListener("click", getElementDetails);

function clickEventListener(e: Event) {
    const element = e.target as HTMLElement;
    const tag = element.tagName.toLowerCase();
    const elDict = getAllAttributes(e.target as HTMLElement);

    const values: TableValues = {
        testdescription: "",
        actiononobject: "",
        object: elDict,
        value: "",
        comments: "eldict",
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
        comments: "eldict",
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
    const elDict = getAllAttributes(e.target as HTMLElement, true);
    const values: TableValues = {
        testdescription: `Populate ${tag}`,
        actiononobject: "PopulateWebElement",
        object: elDict,
        value: textContent,
        comments: "eldict",
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
        comments: "eldict"
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

document.addEventListener("mouseover", (e: any) => {
    const element = e.target as HTMLElement;
    if (startState) {
        prevMouseOverOutline = element.style.outline;
        element.style.outline = "2px solid red";
    }
})

document.addEventListener("mouseout", (e: any) => {
    const element = e.target as HTMLElement;
    if (startState) {
        element.style.outline = prevMouseOverOutline || "";
    }
})

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
    } else if (message.action === "ROW_SELECTED") {
        if (previouslySelectedElement instanceof HTMLElement || message.selectedRow.COMMENTS === "") {
            previouslySelectedElement.style.outline = previousOutlineStyle || "";
        }

        if (message.selectedRow.COMMENTS === "eldict") {
            const elDictString = message.selectedRow.OBJECT;
            const closestElement: any = findClosestElement(elDictString);
            console.log("CLOSEST ELEMENT: ");
            console.log(closestElement);

            if (closestElement instanceof HTMLElement) {
                previousOutlineStyle = closestElement.style.outline;
                closestElement.scrollIntoView({ behavior: "smooth", block: "center", inline: "center" });
                closestElement.style.outline = "2px solid red";
                previouslySelectedElement = closestElement;
            }
        } else if (message.selectedRow.COMMENTS !== "") {
            const type = message.selectedRow.COMMENTS.replace(" ", "").trim();
            const locator: string = message.selectedRow.OBJECT;
            let element: any = null;
            let xpath: string;
            switch (type) {
                case "xpath":
                    element = document.evaluate(locator, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    break;
                case "htmlid":
                    xpath = `//*[@id="${locator}"]`
                    element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    break;
                case "innertext":
                    xpath = `//*[contains(text(), "${locator}")]`
                    element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    break;
                default:
                    console.log("Invalid locator type:", message.selectedRow.COMMENTS);
            }
            if (element) {
                previousOutlineStyle = element?.style.outline;
                element?.scrollIntoView({ behavior: "smooth", block: "center", inline: "center" });
                element.style.outline = "2px solid red";
                previouslySelectedElement = element;
            } else {
                previouslySelectedElement.style.outline = previousOutlineStyle || "";
            }
        }
    } else if (message.action === "ROW_UNSELECTED") {
        previouslySelectedElement.style.outline = previousOutlineStyle || "";
    } else if (message.action === "EXECUTE_TEST_STEP") {
        const action = message.stepData.ACTIONONOBJECT.toLowerCase().replace(" ", "");
        const object = message.stepData.OBJECT;
        const value = message.stepData.VALUE;
        const type = message.stepData.COMMENTS.replace(" ", "").trim().toLowerCase();
        let element;
        const inputEvent = new Event("input", { bubbles: true });

        if (type === "eldict") {
            element = findClosestElement(object);
        } else if (type !== "") {
            const locator: string = message.stepData.OBJECT;
            let xpath: string;
            switch (type) {
                case "xpath":
                    element = document.evaluate(locator, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    break;
                case "htmlid":
                    xpath = `//*[@id="${locator}"]`
                    element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    break;
                case "innertext":
                    xpath = `//*[contains(text(), "${locator}")]`
                    element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    break;
                default:
                    console.log("Invalid locator type:", message.selectedRow.COMMENTS);
            }
        }

        if (element) {
            if (action.includes("click")) {
                (element as HTMLElement).click();
            } else if (action === "populatewebelement") {
                (element as HTMLInputElement).value = value;
                (element as HTMLInputElement).dispatchEvent(inputEvent);
            } else {
                console.error("Unsupported action on object: ", action);
            }
        } else {
            console.log(`Element of locator ${object} does not exist.`)
        }
    }
})

function getAllIFrames(root: any) {
    const queue = [root];
    const iframes = [];

    while (queue.length > 0) {
        const currentNode = queue.shift();
        if (currentNode.tagName?.toLowerCase() === "iframe") {
            iframes.push(currentNode);
        }

        for (let i = 0; i < currentNode.childNodes.length; i++) {
            const child = currentNode.childNodes[i];
            if (child.nodeType === Node.ELEMENT_NODE) {
                queue.push(child);
            }
        }
    }

    return iframes;
}

function findClosestElement(elDict: string) {
    const elDictObj = JSON.parse(elDict);
    const { iframe, tag, text, attributes } = elDictObj;
    const elements = document.getElementsByTagName(tag);
    const iframes = getAllIFrames(window.top?.document);
    let bestMatch: HTMLElement | null = null;
    let highestScore = -1;
    let maxScore = 0;

    if (text !== "") {
        maxScore += 2;
    }

    if (iframe !== -1) {
        maxScore += 2;
    }

    maxScore += Object.keys(attributes).length;

    try {
        Array.from(elements).forEach((el) => {
            let score = 0;

            const iframeIndex = iframes.findIndex(iframe => iframe.contentDocument === el.ownerDocument) === -1 ? 0 : iframes.findIndex(iframe => iframe.contentDocument === el.ownerDocument);
            if (iframeIndex.toString() === iframe) {
                try {
                    if (el.tagName.toLowerCase() === tag.toLowerCase()) {
                        score += 2;
                    } else {
                        score += 0.5;
                    }

                    for (const [key, value] of Object.entries(attributes)) {
                        let attrValue = el.getAttribute(key)
                        if (attrValue) {
                            attrValue = attrValue.replace(/\s+/g, " ").trim();
                            if (attrValue === value) {
                                score += 1
                            } else if (attrValue.includes(value)) {
                                score += 0.5
                            }
                        }
                    }

                    const elText = getTextContent(el);

                    if (elText === text) {
                        score += 2;
                    } else if (elText.includes(text)) {
                        score += 1;
                    }

                    if (score > highestScore) {
                        highestScore = score;
                        bestMatch = el;
                    }
                } catch (e) {
                    console.error(e);
                }
            } else {
                console.error(`NOT IN FRAME. EXPECTED: ${iframe} ACTUAL: ${iframeIndex}`);
            }
        });

        console.log(highestScore, maxScore, highestScore / maxScore);
        return maxScore !== 0 && highestScore / maxScore >= 0.66 ? bestMatch : null;
    } catch (e) {
        console.error(e);
    }
}

function getTextContent(element: HTMLElement) {
    const interactiveElements = ["input", "select", "textarea", "fieldset", "optgroup", "option"];
    const tag = element.tagName.toLowerCase();
    let text = "";

    if (interactiveElements.includes(tag)) {
        if (tag === "select") {
            const select = element as HTMLSelectElement;
            text = Array.from(select.selectedOptions).map(option => option.text).join(", ");
        } else {
            text = (element as HTMLInputElement).value;
        }
    } else {
        element.childNodes.forEach((node) => {
            if (node.nodeType === Node.TEXT_NODE) {
                text += node.textContent?.trim() || "";
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                text += getTextContent(node as HTMLElement);
            }
        });
    }

    return text.trim();
}

function getAllAttributes(element: HTMLElement, interactable=false) {
    const attributesDict: { [key: string]: string | string[] | { [key: string]: string | string[] } } = {};
    const iframes = getAllIFrames(window.top?.document);
    const iframeIndex = iframes.findIndex(iframe => iframe.contentDocument === element.ownerDocument);
    const index = iframeIndex === -1 ? 0 : iframeIndex;

    attributesDict["iframe"] = index.toString();
    attributesDict["tag"] = element.tagName.toLowerCase();
    attributesDict["text"] = interactable ? "" : getTextContent(element);
    attributesDict["attributes"] = {};

    for (let i = 0; i < element.attributes.length; i++) {
        const attr = element.attributes[i];
        const attrName = attr.name;
        const attrValue = attr.value;

        if (attrName !== "style") {
            if (attrValue.includes(" ")) {
                attributesDict["attributes"][attrName] = attrValue.split(" ").filter(Boolean);
            } else {
                attributesDict["attributes"][attrName] = attrValue;
            }
        }
    }

    return JSON.stringify(attributesDict, null, 0);
}