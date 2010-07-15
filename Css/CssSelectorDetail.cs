using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cobalt.Html;

namespace Cobalt.Css {

    /// <summary>
    /// Information on how to handle individual parts of a CSS selector
    /// </summary>
    public class CssSelectorDetail {

        #region Constants

        //reads an css item to determine what is targets
        private static readonly Regex ParseCssSelection = new Regex(
            @"^(?<element>(\w|\-|_)+)?((?<scope>(\.|\#|\$){1})(?<identity>(\w|\-|_)+))?(\:(?<pseudo>\w+)(\((?<pseudoArgument>[^\)]+)\))?)?(?<attributes>.*)?", //(?<attributes>\[.*\])?",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture // | RegexOptions.Singleline | RegexOptions.ExplicitCapture
            );

        //removes leading and trailing brackets
        private static readonly Regex RemoveAttributeBrackets = new Regex(
            "^\\[|\\]$",
            RegexOptions.Compiled
            );

        //drops the extra equals sign off of an attribute selector
        private static readonly Regex RemoveAttributeEqualityModifier = new Regex("=$", RegexOptions.Compiled);

        //locates matches inside of an attribute
        private static readonly Regex ParseAttribute = new Regex(
            @"(?<full>(@(?<name>\w+))?(\s)*(?<param>((?<match>([^a-z0-9=]=)|(=))(\s)*(?<value>.*)?)?))",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

        #endregion

        #region Static Constructors

        //Creates the default selectors for the library
        static CssSelectorDetail() {
            CssSelectorDetail._CreateDefaultPseudoSelectors();
            CssSelectorDetail._CreateDefaultAttributeMatches();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new selection part for the path provided
        /// </summary>
        public CssSelectorDetail(string part) {

            //get all of the sections to use
            this._Result = CssSelectorDetail.ParseCssSelection.Match(part);
            this._ParseAttributes();
            
        }

        #endregion

        #region Static Properties

        //a list of custom Attribute matches
        private static Dictionary<string, Func<string, string, bool>> _AttributeMatches;

        //a list of custom Pseudo selectors
        private static Dictionary<string, Func<IEnumerable<HtmlNode>, string, IEnumerable<HtmlNode>>> _PseudoSelectors;

        #endregion

        #region Properties

        /// <summary>
        /// The element this selector targets
        /// </summary>
        protected string Element {
            get {
                //ignore 'all' selectors since we do that anyways
                string name = (this.GetMatchValue("element") ?? string.Empty).Trim();

                //returns a special name for tags that are going by
                //a different name for processing. HtmlAgilityPack
                //messes up some tags and this helps them be selected
                //and doesn't cause the css selectors to have to use
                //odd names
                return HtmlNode.GetCobaltHelperTagName(name);
            }
        }

        /// <summary>
        /// Returns if this has a match for an element
        /// </summary>
        protected bool HasElementSelector {
            get { return !string.IsNullOrEmpty(this.Element); }
        }

        /// <summary>
        /// Returns the scope for an identity match
        /// </summary>
        protected string Scope {
            get { return (this.GetMatchValue("scope") ?? string.Empty).Trim(); }
        }

        /// <summary>
        /// Returns the pseudo command (if any)
        /// </summary>
        protected string PseudoCommand {
            get { return (this.GetMatchValue("pseudo") ?? string.Empty).Trim(); }
        }

        /// <summary>
        /// Returns the pseudo argument (if any)
        /// </summary>
        protected string PseudoArgument {
            get { return (this.GetMatchValue("pseudoArgument") ?? string.Empty).Trim(); }
        }

        /// <summary>
        /// Returns the identity of a match
        /// </summary>
        protected string Identity {
            get { return (this.GetMatchValue("identity") ?? string.Empty).Trim(); }
        }

        /// <summary>
        /// Is this selecting an ID value
        /// </summary>
        protected bool HasIDSelector {
            get { return this.Scope.Equals("#") && !string.IsNullOrEmpty(this.Identity); }
        }

        /// <summary>
        /// Is this selecting a Name attribute
        /// </summary>
        protected bool HasNameSelector {
            get { return this.Scope.Equals("$") && !string.IsNullOrEmpty(this.Identity); }
        }

        /// <summary>
        /// Is this selecting an ID value
        /// </summary>
        protected bool HasClassSelector {
            get { return this.Scope.Equals(".") && !string.IsNullOrEmpty(this.Identity); }
        }

        /// <summary>
        /// Does this selector have a pseudo selector
        /// </summary>
        protected bool HasPseudoCommand {
            get { return !string.IsNullOrEmpty(this.PseudoCommand); }
        }

        //the result of the match
        private Match _Result;

        //the css attributes attached
        private string[] _Attributes;

        //a list of methods that should be executed to match this selector
        private List<Func<HtmlNode, bool>> _Requirements;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a selection filter for this selector
        /// </summary>
        public Func<HtmlNode, bool> GetRequirements() {
            
            //create the requirements
            if (this._Requirements == null) { this._BuildRequirements(); }

            //if nothing was created, do not allow this to pass
            if (this._Requirements.Count > 0) {
                return node => this._Requirements.All(requirement => requirement(node));
            }
            else {
                return node => false;
            }
        }

        /// <summary>
        /// Returns the pseudo requirements for this selector
        /// </summary>
        public IEnumerable<HtmlNode> ApplyPseudoFilter(IEnumerable<HtmlNode> nodes) {

            //check the arguments
            if (nodes == null) { return new HtmlNode[] { }; }
            if (!this.HasPseudoCommand || !nodes.Any()) { return nodes; }

            //return the correct value
            string command = this.PseudoCommand;
            string argument = this.PseudoArgument;

            //find the command to run
            if (CssSelectorDetail._PseudoSelectors.ContainsKey(command)) {
                return CssSelectorDetail._PseudoSelectors[command](nodes, argument).ToArray();
            }
            //no such selector exists
            else {
                throw new GenericCobaltException(string.Format("{0} is not a valid pseudo selector!", command));
            }

        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the value of the group requested (if any)
        /// </summary>
        protected string GetMatchValue(string name) {
            if (this._Result.Groups[name] == null) { return null; }
            string result = this._Result.Groups[name].Value;
            return string.IsNullOrEmpty((result ?? string.Empty).Trim()) ? null : result;
        }

        /// <summary>
        /// Returns if the group name provided has a value for it
        /// </summary>
        protected bool HasMatchValue(string name) {
            return this.GetMatchValue(name) is string;
        }

        #endregion

        #region Building The Path

        //builds a list of requirements to match this node
        private void _BuildRequirements() {
            this._Requirements = new List<Func<HtmlNode, bool>>();

            //check if the tag is being compared
            if (this.HasElementSelector) {

                //wildcard selectors
                if (this.Element.Equals("*")) {
                    this._Requirements.Add(node => true);
                }
                //or a named element
                else {
                    this._Requirements.Add(node => node.SelectorTag.Equals(this.Element, StringComparison.OrdinalIgnoreCase));
                }
            }

            //check for ID attributes
            if (this.HasIDSelector) {
                this._Requirements.Add(node => node["id"].Equals(this.Identity));
            }
            //check for a name attribute
            else if (this.HasNameSelector) {
                this._Requirements.Add(node => node["name"].Equals(this.Identity));
            }
            //check for a CSS class value
            else if (this.HasClassSelector) {
                this._Requirements.Add(node => node.GetCss().Any(value => value.Equals(this.Identity)));
            }

            //also, build the attributes if needed
            this._BuildAttributeRequirements();

        }

        //builds an attribute string for this element
        private void _BuildAttributeRequirements() {

            //check for any additional attributes
            foreach (string attribute in this._Attributes) {

                //make sure something is even there
                if (string.IsNullOrEmpty((attribute ?? string.Empty).Trim())) {
                    continue;
                }

                //try and extract the match
                Match match = CssSelectorDetail.ParseAttribute.Match(attribute);

                //get the parts of the match
                var content = new {
                    full = match.Groups["full"].Value,
                    name = match.Groups["name"].Value,
                    value = this._ExtractValue(match.Groups["value"].Value),
                    hasParam = !string.IsNullOrEmpty((match.Groups["param"].Value ?? string.Empty).Trim()),
                    type = CssSelectorDetail.RemoveAttributeEqualityModifier
                        .Replace(match.Groups["match"].Value, string.Empty)
                };

                //if this has no value then check the name
                if (!content.hasParam) {
                    this._Requirements.Add(node => node.HasAttribute(content.name));
                }
                //try and find the matching selector
                else if (CssSelectorDetail._AttributeMatches.ContainsKey(content.type)) {
                    Func<string, string, bool> compare = CssSelectorDetail._AttributeMatches[content.type];
                    this._Requirements.Add(node => compare(content.value, (node[content.name] ?? string.Empty).ToString()));
                }
                //unsupported
                else {
                    throw new GenericCobaltException(string.Format("{0}= is not a supported attribute match type.", content.type));
                }

            }

        }

        //gets the value of an attribute by unesaping and replacing values
        private string _ExtractValue(string value) {
            value = Regex.Replace(value, "^('|\")|('|\")$", string.Empty);
            value = Regex.Replace(value, @"\\('|""|\[|\]|\(|\))", match => match.Value.Substring(1));
            return value;
        }

        //helper method to get the pseudo value as an integer and
        //to throw the correct error if it fails
        private static int _GetPseudoArgumentAsInteger(string command, string argument) {
            try {
                return int.Parse(argument);
            }
            catch (Exception ex) {
                throw new GenericCobaltException(
                    string.Format("The argument for :{0} should be an integer", command),
                    ex
                    );
            }
        }

        //parses the selector to find attributes
        private void _ParseAttributes() {

            //make sure this even worked
            this._Attributes = new string[] { };
            if (!this._Result.Groups["attributes"].Success) { return; }

            //get the attribute string
            string detail = this._Result.Groups["attributes"].Value;
            detail = CssSelectorDetail.RemoveAttributeBrackets.Replace((detail ?? string.Empty).Trim(), string.Empty);

            //double check again that something was found
            if (string.IsNullOrEmpty((detail ?? string.Empty).Trim())) {
                return;
            }

            //split out each of the matches
            this._Attributes = Regex.Split(detail, "\\]\\[")
                .Select(attr => (attr ?? string.Empty).Trim())
                .Where(attr => !string.IsNullOrEmpty(attr))
                .Distinct()
                .ToArray();

        }

        #endregion

        #region Registering

        /// <summary>
        /// Registers a custom pseudo selector
        /// </summary>
        public static void RegisterCustomPseudoSelector(string name, Func<IEnumerable<HtmlNode>, string, IEnumerable<HtmlNode>> compare) {
            name = (name ?? string.Empty).Trim();

            //check the argument
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentException("You must provide a name for a custom pseudo selector.");
            }
            else if (!Regex.IsMatch(name, "^[a-z0-9]+$", RegexOptions.IgnoreCase)) {
                throw new ArgumentException("A pseudo selector name can only be letters or numbers");
            }

            //remove an existing one if needed
            CssSelectorDetail._PseudoSelectors.Remove(name);
            CssSelectorDetail._PseudoSelectors.Add(name, compare);

        }

        /// <summary>
        /// Removes a custom pseudo selector if any for the name exists
        /// </summary>
        public static void UnregisterCustomPseudoSelector(string name) {
            name = (name ?? string.Empty).Trim();
            CssSelectorDetail._PseudoSelectors.Remove(name);
        }

        /// <summary>
        /// Registers a custom attribute selector
        /// </summary>
        public static void RegisterCustomAttributeSelector(string type, Func<string, string, bool> compare) {
            type = CssSelectorDetail.RemoveAttributeEqualityModifier.Replace((type ?? string.Empty).Trim(), string.Empty);

            //check the argument
            if (string.IsNullOrEmpty(type)) {
                throw new ArgumentException("You must provide a special character for your attribute selector.");
            }
            //make sure it is only a single character
            else if (type.Length > 1) {
                throw new ArgumentException("An attribute selector modifier cannot be more than one special character long.");
            }
            else if (Regex.IsMatch(type, "[a-z0-9]+", RegexOptions.IgnoreCase)) {
                throw new ArgumentException("An attribute selector modifier cannot be letters or numbers");
            }

            //remove an existing one if needed
            CssSelectorDetail._AttributeMatches.Remove(type);
            CssSelectorDetail._AttributeMatches.Add(type, compare);

        }

        /// <summary>
        /// Removes an existing custom attribute selector
        /// </summary>
        public static void UnregisterCustomAttributeSelector(string type) {
            type = CssSelectorDetail.RemoveAttributeEqualityModifier.Replace((type ?? string.Empty).Trim(), string.Empty);
            CssSelectorDetail._AttributeMatches.Remove(type);
        }

        #endregion

        #region Creating Defaults (Static Methods)

        //registers built-in attribute matching
        private static void _CreateDefaultAttributeMatches() {
            CssSelectorDetail._AttributeMatches = new Dictionary<string, Func<string, string, bool>>();

            //add basic selector
            CssSelectorDetail._AttributeMatches.Add("", (argument, value) => value.Equals(argument, StringComparison.OrdinalIgnoreCase));
            CssSelectorDetail._AttributeMatches.Add("|", (argument, value) => value.StartsWith(argument, StringComparison.OrdinalIgnoreCase));
            CssSelectorDetail._AttributeMatches.Add("$", (argument, value) => value.EndsWith(argument, StringComparison.OrdinalIgnoreCase));
            CssSelectorDetail._AttributeMatches.Add("~", (argument, value) => value.ToUpper().Contains(argument.ToUpper()));

        }

        //prepares the built-in pseudo selectors
        private static void _CreateDefaultPseudoSelectors() {

            //create the list of selectors
            CssSelectorDetail._PseudoSelectors = new Dictionary<string, Func<IEnumerable<HtmlNode>, string, IEnumerable<HtmlNode>>>();

            //position in the list
            CssSelectorDetail._PseudoSelectors.Add("first", (nodes, arg) => nodes.HtmlElements().Take(1));
            CssSelectorDetail._PseudoSelectors.Add("last", (nodes, arg) => nodes.HtmlElements().Skip(nodes.Count() - 1));

            //even numbered 
            CssSelectorDetail._PseudoSelectors.Add("even", (nodes, arg) => {
                int index = 0;
                return nodes.HtmlElements().Where(node => ++index % 2 == 0);
            });

            //odd numbered
            CssSelectorDetail._PseudoSelectors.Add("odd", (nodes, arg) => {
                int index = 0;
                return nodes.HtmlElements().Where(node => ++index % 2 != 0);
            });

            //value at specific index
            CssSelectorDetail._PseudoSelectors.Add("nd", (nodes, arg) => {
                int value = CssSelectorDetail._GetPseudoArgumentAsInteger("nd", arg);
                return nodes.HtmlElements().Skip(value - 1).Take(1);
            });

            //every nth-value
            CssSelectorDetail._PseudoSelectors.Add("nth", (nodes, arg) => {
                int index = 0;
                int value = CssSelectorDetail._GetPseudoArgumentAsInteger("nth", arg);
                return nodes.HtmlElements().Where(node => ++index % value == 0);
            });

            //less or less than/equal to a value 
            CssSelectorDetail._PseudoSelectors.Add("lt", (nodes, arg) => {
                int index = 0;
                int value = CssSelectorDetail._GetPseudoArgumentAsInteger("lt", arg);
                return nodes.Where(node => ++index < value);
            });
            CssSelectorDetail._PseudoSelectors.Add("lte", (nodes, arg) => {
                int index = 0;
                int value = CssSelectorDetail._GetPseudoArgumentAsInteger("lte", arg);
                return nodes.Where(node => ++index <= value);
            });

            //greater or greater than/equal to a value 
            CssSelectorDetail._PseudoSelectors.Add("gt", (nodes, arg) => {
                int index = 0;
                int value = CssSelectorDetail._GetPseudoArgumentAsInteger("gt", arg);
                return nodes.Where(node => {
                    bool low = ++index > value;
                    return low;
                });
            });
            CssSelectorDetail._PseudoSelectors.Add("gte", (nodes, arg) => {
                int index = 0;
                int value = CssSelectorDetail._GetPseudoArgumentAsInteger("gte", arg);
                return nodes.Where(node => ++index >= value);
            });

            //input elements
            CssSelectorDetail._PseudoSelectors.Add("text", (nodes, arg) => nodes.Where(node => node.IsTextInput()));
            CssSelectorDetail._PseudoSelectors.Add("password", (nodes, arg) => nodes.Where(node => node.IsPaswordInput()));
            CssSelectorDetail._PseudoSelectors.Add("button", (nodes, arg) => nodes.Where(node => node.IsButtonInput()));
            CssSelectorDetail._PseudoSelectors.Add("checkbox", (nodes, arg) => nodes.Where(node => node.IsCheckboxInput()));
            CssSelectorDetail._PseudoSelectors.Add("radio", (nodes, arg) => nodes.Where(node => node.IsRadioButton()));
            CssSelectorDetail._PseudoSelectors.Add("hidden", (nodes, arg) => nodes.Where(node => node.IsHiddenInput()));
            CssSelectorDetail._PseudoSelectors.Add("file", (nodes, arg) => nodes.Where(node => node.IsFileInput()));
            CssSelectorDetail._PseudoSelectors.Add("submit", (nodes, arg) => nodes.Where(node => node.IsSubmitButton()));
            CssSelectorDetail._PseudoSelectors.Add("textarea", (nodes, arg) => nodes.Where(node => node.IsTextArea()));

        }

        #endregion

    }


}
