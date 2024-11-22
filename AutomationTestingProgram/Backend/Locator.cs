namespace AutomationTestingProgram.Backend
{
    public enum Locator
    {
        /// <summary>
        /// Locator type of 'html id'. Should be unique to all elements.
        /// </summary>
        ID,

        /// <summary>
        /// Locator type of 'name'. Used to track inputs to database.
        /// </summary>
        NAME,

        /// <summary>
        /// Locator type of 'class'.
        /// </summary>
        CLASS,

        /// <summary>
        /// Locator type of 'css' is used for selecting elements based on their css selectors.
        /// This includes classes, tags, IDs, custom attributes, pseudo-classes, child elements, etc.
        /// Does not support searching for elements based on their text or HTML content.
        /// </summary>
        CSS,

        /// <summary>
        /// Locator type of 'tag'.
        /// </summary>
        TAG,

        /// <summary>
        /// Locator type of 'linktext'. Searches for equal text in '&lt;a&gt;' elements.
        /// </summary>
        LINKTEXT,

        /// <summary>
        /// Locator type of 'linktext'. Searches for partial equal text in '&lt;a&gt;' elements.
        /// </summary>
        LINKTEXT_PARTIAL,

        /// <summary>
        /// Locator type of 'js' or javascript.
        /// </summary>
        JS,

        /// <summary>
        /// Locator type of 'xpath'.
        /// </summary>
        XPATH,

        // *************************************************************

        /// <summary>
        /// Whether the current locater type is By innertext. Will be transformed into "XPATH".
        /// </summary>
        INNERTEXT,

        /// <summary>
        /// Whether the current locater type is By innertext partial. Will be transformed into "XPATH".
        /// </summary>
        INNERTEXT_PARTIAL,

        /// <summary>
        /// Whether the current locater type is By outertext. Will be transformed into "JS".
        /// </summary>
        OUTERTEXT,

        /// <summary>
        /// Whether the current locater type is By innerhtml. Will be transformed into "JS".
        /// </summary>
        INNERHTML,

        /// <summary>
        /// Whether the current locater type is By outerhtml. Will be transformed into "JS".
        /// </summary>
        OUTERHTML,

        /// <summary>
        /// Whether the current locater type is By custom. Will be transformed into "css". Note: requires string input for custom attribute name.
        /// </summary>
        CUSTOM,

        /// <summary>
        /// Whether the current locater type is By custom partial. Will be transformed into "css". Note: require string input for custom attribute name.
        /// </summary>
        CUSTOM_PARTIAL,

        // CUSTOM: Includes 'extra'/custom attributes like code, role, aria-text, alt etc. Note: Does not work for the following common attributes: innerText, outerText, innerHTML, outerHTML, etc.
        // Then Translated to CSS. Ex: [alt*="Ontario"] or span[style*='width: 50%; float: right'] or [code='OSYC.Y2025.Contacts']
        // Note: Must be attributes the element itself. Style for example is inline-style. Ex: span[style*='width: 50%; float: right; text-align: right; padding-right: 2em'] -> all defined in the html element itself
    }

    public static class LocatorMappings
    {
        private static readonly Dictionary<string, Locator> GetAttributeType = new Dictionary<string, Locator>()
        {
            { "xpath", Locator.XPATH },
            { "html id", Locator.ID },
            { "id", Locator.ID },
            { "name", Locator.NAME },
            { "innertext", Locator.INNERTEXT },
            { "partial innertext", Locator.INNERTEXT_PARTIAL },
            { "innertext partial", Locator.INNERTEXT_PARTIAL },
            { "outertext", Locator.OUTERTEXT },
            { "linktext", Locator.LINKTEXT },
            { "partial linktext", Locator.LINKTEXT_PARTIAL },
            { "linktext partial", Locator.LINKTEXT_PARTIAL },
            { "css", Locator.CSS },
            { "css selector", Locator.CSS },
            { "class name", Locator.CLASS },
            { "class", Locator.CLASS },
            { "tag name", Locator.TAG },
            { "html tag", Locator.TAG },
            { "tag", Locator.TAG },
            { "javascript", Locator.JS },
            { "js", Locator.JS },
            { "js script", Locator.JS },
            { "innerhtml", Locator.INNERHTML },
            { "outerhtml", Locator.OUTERHTML },
            { "custom", Locator.CUSTOM },
            { "custom partial", Locator.CUSTOM_PARTIAL },
        };

        public static Locator? GetLocatorType(string locatorString)
        {
            if (GetAttributeType.TryGetValue(locatorString.ToLower(), out var locatorType))
            {
                return locatorType;
            }
            return null;
        }
    }
}
