using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cobalt.Html;

namespace Cobalt.Css {

    /// <summary>
    /// Simple CSS selector for HtmlAgilityPack
    /// </summary>
    public class CssSelector {

        #region Constructors

        /// <summary>
        /// Starts a new selection for the provided set of nodes
        /// </summary>
        public CssSelector(string selector, IEnumerable<HtmlNode> start) {
            this._SearchNodes = start;
            this._Selector = selector;
            this.StartingScope = CssSelectorScope.Any;
        }

        /// <summary>
        /// Starts a new selection for the provided set of nodes
        /// </summary>
        public CssSelector(string selector, HtmlNode start)
            : this(selector, new HtmlNode[] { start }) {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The starting scope for the CssSelector
        /// </summary>
        public CssSelectorScope StartingScope { get; set; }

        //the string used to select with
        private string _Selector;

        //the nodes matched in the selection
        private IEnumerable<HtmlNode> _SelectedNodes;

        //the nodes that should be searched against
        private IEnumerable<HtmlNode> _SearchNodes;

        //the last combination used - determines filter reactions
        private CssSelectorScope _PreviousCombinator;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets matching values for the CSS string provided
        /// </summary>
        public IEnumerable<HtmlNode> GetMatches() {

            //make sure something was even selected
            if (string.IsNullOrEmpty(this._Selector)) {
                return new HtmlNode[] { };
            }

            //default to selecting anything
            this._SetScope(this.StartingScope);

            //parse and build each item in the selection
            int attempt = 0;
            CssSelectorReader reader = new CssSelectorReader(this._Selector);
            while (!reader.EndOfCssSelector() || attempt > 100) {

                //mark the attempts made
                attempt++;

                //update the filter
                CssSelectorDetail part = reader.GetNextSelector();
                this._Filter(part.GetRequirements());

                //apply any pseudo information
                this._SelectedNodes = part.ApplyPseudoFilter(this._SelectedNodes);

                //update the scope
                CssSelectorScope? scope = reader.GetNextCombinator();
                if (scope == null) { break; }

                //update the places to search
                this._SearchNodes = this._SelectedNodes;
                this._SetScope((CssSelectorScope)scope);

            }

            //return the final string to use
            return attempt == 0
                ? new HtmlNode[] { }
                : this._SelectedNodes;

        }

        #endregion

        #region Private Methods

        //changes the nodes to search based on the selector
        private void _SetScope(CssSelectorScope scope) {

            //update the scope of the search
            switch (scope) {

                //matchs any nodes including the currently selected ones
                case CssSelectorScope.Any:
                    this._SearchNodes = this._SearchNodes.SelectMany(node => node.DescendantsAndSelf());
                    break;

                //selects any node beneath the current nodes
                case CssSelectorScope.AnyChild:
                    this._SearchNodes = this._SearchNodes.SelectMany(node => node.Descendants());
                    break;

                //selects only the children directly beneath the selected element
                case CssSelectorScope.DirectChildren:
                    this._SearchNodes = this._SearchNodes.SelectMany(node => node.Children);
                    break;

                //changes the cope to match the sibling nodes around it
                case CssSelectorScope.Siblings:
                    this._SearchNodes = this._SearchNodes.SelectMany(node => node.Siblings);
                    break;

                //limits the scope to only the currently selected item
                case CssSelectorScope.Selected:
                    this._SearchNodes = this._SearchNodes.Select(node => node);
                    break;
            }

            //always filter to distinct values
            this._PreviousCombinator = scope;
            this._SearchNodes = this._SearchNodes.Distinct();

        }

        //filters the currently selected nodes
        private void _Filter(Func<HtmlNode, bool> requirements) {

            //checking for sibling nodes
            if (this._PreviousCombinator == CssSelectorScope.Siblings) {
                this._SelectedNodes = this._SelectedNodes.Union(this._SearchNodes.Where(requirements));
            }
            //or all other matches
            else {
                this._SelectedNodes = this._SearchNodes.Where(requirements);
            }

            //update to select only actual nodes
            this._SelectedNodes = this._SelectedNodes.HtmlElements();
        }

        #endregion

    }

}
