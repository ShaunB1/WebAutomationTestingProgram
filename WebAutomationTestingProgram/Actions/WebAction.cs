using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using WebAutomationTestingProgram.Core.Helpers;
using WebAutomationTestingProgram.Core.Hubs.Services;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

class ElDict
{
    public int IFrame { get; set; }
    public string Tag { get; set; }
    public string Text { get; set; }
    public Dictionary<string, object> Attributes { get; set; }
}

public abstract class WebAction
{
    public abstract Task<bool> ExecuteAsync(IPage page,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration,
        string cycleGroupName);

    public string GetIterationData(
        TestStep step,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration,
        string cycleGroupName
    )
    {
        if (cycleGroups.TryGetValue(cycleGroupName, out var iterations))
        {
            var iterationData = iterations[currentIteration];
            
            if (step.TestCaseName.Contains("{") && step.TestCaseName.Contains("}"))
            {
                var pattern = @"\{(.*?)\}";
                                
                MatchCollection matches = Regex.Matches(step.TestCaseName, pattern);
                var extractedValues = "";

                foreach (Match match in matches)
                {
                    extractedValues = match.Groups[1].Value;
                }

                var identifier = "";
                var splitValues = extractedValues.Split(", ");
                foreach (var splitValue in splitValues)
                {
                    if (iterationData.TryGetValue(splitValue, out var name))
                    {
                        identifier += $" {name}";
                    }
                }

                step.TestCaseName = Regex.Replace(step.TestCaseName, pattern, $"{identifier}");
            }
            
            string variableName;
            if (step.Value.StartsWith("{") && step.Value.EndsWith("}"))
            {
                variableName = step.Value.Trim('{', '}');
            }
            else
            {
                variableName = step.Value;
            }
            
            
            if (iterationData.TryGetValue(variableName, out var value))
            {
                return value;
            }
        }

        return "";
    }

    public async Task<IElementHandle> LocateElementAsync(IPage page, string locator, string locatorType)
    {
        var types = new List<string> { "htmlid", "xpath", "innertext" };
        locatorType = locatorType.ToLower().Replace(" ", "");
        
        for (int j = 0; j < 2; j++)
        {
            if (types.Contains(locatorType))
            {
                if (locatorType == "innertext")
                {
                    var elements = page.Locator($"//*[contains(text(), '{locator}')]");
                    var count = await elements.CountAsync();

                    for (int i = 0; i < count; i++)
                    {
                        var element = elements.Nth(i);
                        var isVisible = await element.IsVisibleAsync();
                        if (isVisible)
                        {
                            var elementHandle = await element.ElementHandleAsync();
                            return elementHandle;
                        }
                    }
                }
                else if (locatorType == "htmlid")
                {
                    var element = page.Locator($"#{locator}");
                    try
                    {
                        var elementHandle = await element.ElementHandleAsync();
                        return elementHandle;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unable to locate element '{locator}'", ex);
                    }
                }
                else if (locatorType == "xpath")
                {
                    var elementHandle = await page.QuerySelectorAsync(locator);

                    if (elementHandle == null)
                    {
                        await Task.Delay(30000);
                        continue;
                    }
                    
                    var isVisible = await elementHandle.IsVisibleAsync();
                    if (isVisible)
                    {
                        return elementHandle;
                    }
                }
            } 
            else if (locatorType.ToLower() == "eldict")
            {
                JObject elDictObj = JObject.Parse(locator);
                ElDict obj = new ElDict
                {
                    IFrame = int.TryParse(elDictObj["iframe"]?.ToString(), out var iframe) ? iframe : 0,
                    Tag = elDictObj["tag"]?.ToString() ?? string.Empty,
                    Text = elDictObj["text"]?.ToString() ?? string.Empty,
                    Attributes = elDictObj["attributes"]?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>(),
                };
                
                var elements = await page.QuerySelectorAllAsync(obj.Tag);
                var iframes = await GetAllIFramesFromTopAsync(page); // takes forever
                IElementHandle bestMatch = null;
                var highestScore = -1;
                var maxScore = 0;

                if (obj.Text != "")
                {
                    maxScore += 2;
                }

                if (obj.IFrame != -1)
                {
                    maxScore += 2;
                }

                maxScore += obj.Attributes.Count;

                try
                {
                    foreach (var el in elements)
                    {
                        var score = 0;
                        var iframeIndex = await GetIFrameIndexAsync(iframes, el);

                        if (iframeIndex == obj.IFrame || iframeIndex == -1)
                        {
                            foreach (var kvp in obj.Attributes)
                            {
                                var key = kvp.Key;
                                var value =  kvp.Value is JArray ? string.Join(" ", (kvp.Value as JArray).Select(token => token.ToString())) : kvp.Value.ToString();
                                var attrValue = await el.GetAttributeAsync(key);
                                if (attrValue != null)
                                {
                                    attrValue = Regex.Replace(attrValue, @"\s+", " ").Trim();
                                    if (attrValue == value)
                                    {
                                        score += 2;
                                    }
                                    else if (attrValue.Contains(value))
                                    {
                                        score += 1;
                                    }
                                }
                            }

                            var elText = await GetTextContent(el);

                            if (elText == obj.Text)
                            {
                                score += 2;
                            }
                            else if (elText.Contains(obj.Text))
                            {
                                score += 1;
                            }

                            if (score > highestScore)
                            {
                                var isVisible = await el.IsVisibleAsync();
                                if (isVisible)
                                {
                                    highestScore = score;
                                    bestMatch = el;                            
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"NOT IN FRAME. EXPECTED: {iframe} ACTUAL: {iframeIndex}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                return maxScore != 0 && highestScore / maxScore >= 0.66 ? bestMatch : null;
            }

            await Task.Delay(30000);
        }

        return null;

        async Task<string> GetTextContent(IElementHandle element)
        {
            try
            {
                var interactiveElements = new string[] { "input", "select", "textarea", "fieldset", "optgroup", "option" };
                var tag = await element.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
                var text = "";

                var text2 = await element.EvaluateAsync<string>(@"node => node.textContent?.trim() || ''");
                if (text2.Contains("Algoma University (ALGM)"))
                {
                    Console.WriteLine("Mental Health Services Grant 2023 - 2024");
                }
                
                if (interactiveElements.Contains(tag))
                {
                    if (tag == "select")
                    {
                        var selectedOptions = await element.EvaluateAsync<string[]>(
                            @"select => Array.from(select.selectedOptions).map(option => option.text)"
                        );
                        text = string.Join(", ", selectedOptions);
                    }
                    else
                    {
                        text = await element.EvaluateAsync<string>(@"el => el.value");
                    }
                }
                else
                {
                    var childNodes = await element.EvaluateHandleAsync("el => el?.childNodes || []");
                    var childCount = await childNodes.EvaluateAsync<int>("nodes => nodes?.length || 0");

                    for (int i = 0; i < childCount; i++)
                    {
                        var childNode = await childNodes.EvaluateHandleAsync($"nodes => nodes[{i}]");
                        var nodeType = await childNode.EvaluateAsync<int>("node => node.nodeType");

                        if (nodeType == 3)
                        {
                            var textContent = await childNode.EvaluateAsync<string>(@"node => node.textContent?.trim() || ''");
                            text += textContent;
                        }
                        else if (nodeType == 1)
                        {
                            var childElement = childNode.AsElement();
                            if (childElement != null)
                            {
                                text += await GetTextContent(childElement);
                            }
                        }
                    }
                }

                return text;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        async Task<int> GetIFrameIndexAsync(List<IElementHandle> iframes, IElementHandle element)
        {
            var ownerDocument = await element.EvaluateHandleAsync("el => el.ownerDocument");

            for (int i = 0; i < iframes.Count; i++)
            {
                var iframeDocument = await iframes[i].EvaluateHandleAsync("iframe => iframe.contentDocument");

                if (await iframeDocument.JsonValueAsync<object>() == await ownerDocument.JsonValueAsync<object>())
                {
                    return i;
                }
            }

            return -1;
        }
        
        async Task<List<IElementHandle>> GetAllIFramesFromTopAsync(IPage page)
        {
            var root = await page.QuerySelectorAsync("body");

            if (root == null)
            {
                throw new Exception("Root element not found in the top-level document.");
            }
            
            return await GetAllIFramesAsync(root);
        }
        
        async Task<List<IElementHandle>> GetAllIFramesAsync(IElementHandle root)
        {
            var queue = new Queue<IElementHandle>();
            var iframes = new List<IElementHandle>();
            
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                var tagName = await currentNode.EvaluateAsync<string>("node => node.tagName?.toLowerCase()");

                if (tagName == "iframe")
                {
                    iframes.Add(currentNode);
                }
                
                var childNodes = await currentNode.QuerySelectorAllAsync(":scope > *");

                var childTasks = childNodes.Select(async child =>
                {
                    var tagName = await child.EvaluateAsync<string>("node => node.tagName?.toLowerCase()");
                    if (tagName == "iframe")
                    {
                        lock (iframes)
                        {
                            iframes.Add(child);
                        }
                    }
                    else
                    {
                        lock (queue)
                        {
                            queue.Enqueue(child);
                        }
                    }
                });
                
                await Task.WhenAll(childTasks);
            }
            
            return iframes;
        }
    }
}