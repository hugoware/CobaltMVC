using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace Cobalt.Css {

    /// <summary>
    /// Reads each part from a CSS selector path
    /// </summary>
    public class CssSelectorReader {

        #region Constants

        private static readonly char[] Separators = { ' ', '>', '+' };
        private static readonly char[] Escapes = { '\\' };
        private static readonly Regex ClearExcessiveSpaces = new Regex(@"\s{,999}|^\s*|\s*$", RegexOptions.Compiled);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new CssSelectorReader for a new css selector
        /// </summary>
        public CssSelectorReader(string path) {
            this.Selector = (path ?? string.Empty).Trim();

            //the currently tracked items
            this._CurrentSelector = string.Empty;
            this._CurrentCombinator = string.Empty;

            //checing for opened and closed items
            this._IsEscaping = false;
            this._OpenElements = new List<int>(new int[] { 0, 0, 0, 0 });

            //read the content
            this._Read();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selector for path
        /// </summary>
        public string Selector { get; private set; }

        //the list of found items
        private List<CssSelectorDetail> _Selectors;
        private List<CssSelectorScope> _Combinators;

        //the currently tracked items
        private string _CurrentSelector;
        private string _CurrentCombinator;

        //tracking open elements and escapes
        private bool _IsEscaping;
        private List<int> _OpenElements;

        #endregion

        #region Helper Methods

        //checks if any of the segments are currently open or not
        private bool _IsEscapeCharacter(char letter) {
            this._IsEscaping = CssSelectorReader.Escapes.Any(item => item.Equals(letter));
            return this._IsEscaping;
        }

        //checks if any of the segments are currently open or not
        private bool _IsSeparator(char letter) {
            return CssSelectorReader.Separators.Any(item => item.Equals(letter));
        }

        //checks if any of the segments are currently open or not
        private void _SaveCurrentSelector() {
            if (string.IsNullOrEmpty(this._CurrentSelector)) { return; }

            //save the word to use
            this._CurrentSelector = this._CurrentSelector.Trim();
            CssSelectorDetail part = new CssSelectorDetail(this._CurrentSelector);
            this._Selectors.Add(part);
            this._CurrentSelector = string.Empty;
        }

        //checks if any of the segments are currently open or not
        private void _SaveCurrentCombinator() {
            if (string.IsNullOrEmpty(this._CurrentCombinator)) { return; }

            //format the value
            this._CurrentCombinator = this._CurrentCombinator.Trim();
            this._CurrentCombinator = !string.IsNullOrEmpty(this._CurrentCombinator)
                ? this._CurrentCombinator
                : " ";

            //determine the actual value
            CssSelectorScope scope = this._CurrentCombinator.Equals(">") ? CssSelectorScope.DirectChildren
                : this._CurrentCombinator.Equals(" ") ? CssSelectorScope.AnyChild
                : this._CurrentCombinator.Equals("+") ? CssSelectorScope.Siblings
                : CssSelectorScope.Any;

            //save for later
            this._Combinators.Add(scope);
            this._CurrentCombinator = string.Empty;
        }

        //checks if any of the segments are currently open or not
        private bool _HasOpenSegments() {
            return this._OpenElements.Sum(item => item) > 0;
        }

        //check if the value is an open or close
        private void _CheckOpenCloseElement(char letter) {

            //don't worry if we're in the middle of escaping characters
            if (this._IsEscaping) { return; }

            //check how to track this letter
            if ('['.Equals(letter)) {
                this._OpenElements[0]++;
            }
            else if (']'.Equals(letter)) {
                this._OpenElements[0]--;
            }

        }

        //reads the path and prepares it to be used
        private void _Read() {
            this._Selectors = new List<CssSelectorDetail>();
            this._Combinators = new List<CssSelectorScope>();

            //clear any excessive spaces
            string path = CssSelectorReader.ClearExcessiveSpaces.Replace(this.Selector, string.Empty);

            //check each letter to convert the sections
            int index = 0;
            foreach (char letter in path) {
                this._CheckOpenCloseElement(letter);
                this._IsEscapeCharacter(letter);

                //assign this letter to the correct type
                if (this._IsSeparator(letter) && !this._HasOpenSegments()) {
                    this._CurrentCombinator = string.Concat(this._CurrentCombinator, letter);
                    this._SaveCurrentSelector();
                }
                else {
                    this._CurrentSelector = string.Concat(this._CurrentSelector, letter);
                    this._SaveCurrentCombinator();
                }

                //check if this is the end of the request
                index++;
                if (index == path.Length && !this._HasOpenSegments()) {
                    this._SaveCurrentSelector();
                }

            }

            //get the last word in
            if (!string.IsNullOrEmpty(string.Concat(this._CurrentCombinator, this._CurrentSelector).Trim())) {
                throw new ApplicationException("Invalid CSS selector!");
            }

        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns the next combinator for this string
        /// </summary>
        public CssSelectorScope? GetNextCombinator() {

            //get the next value
            if (this._Combinators.Count == 0) { return null; }
            CssSelectorScope? combinator = this._Combinators.First();

            //move forward in the list
            this._Combinators.RemoveAt(0);
            return combinator;

        }

        /// <summary>
        /// Returns the next selector for this string
        /// </summary>
        public CssSelectorDetail GetNextSelector() {

            //get the next value
            if (this._Selectors.Count == 0) { return null; }
            CssSelectorDetail selector = this._Selectors.First();

            //move forward in the list
            this._Selectors.RemoveAt(0);
            return selector;

        }

        /// <summary>
        /// Returns if this has reached the end of the selection
        /// </summary>
        public bool EndOfCssSelector() {
            return (this._Selectors.Count + this._Combinators.Count) == 0;
        }

        #endregion

    }

}
