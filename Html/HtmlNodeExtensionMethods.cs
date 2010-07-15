using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cobalt.Css;
using Cobalt.Interfaces;
using Cobalt.Html;

namespace Cobalt {

    /// <summary>
    /// Common extensions for modifying collections of HtmlNodes
    /// </summary>
    public static class HtmlNodeExtensionMethods {

        /// <summary>
        /// Returns only the html elements from a list of nodes
        /// </summary>
        public static CobaltElement AsElement<T>(this ICobaltElement[] nodes) {
            return new CobaltElement(nodes.Select(node => node.AsElement()));
        }

        /// <summary>
        /// Returns only the html elements from a list of nodes
        /// </summary>
        public static CobaltElement AsElement<T>(this ICollection<ICobaltElement> nodes) {
            return new CobaltElement(nodes.Select(node => node.AsElement()));
        }

        /// <summary>
        /// Returns only the html elements from a list of nodes
        /// </summary>
        public static CobaltElement AsElement<T>(this IEnumerable<ICobaltElement> nodes) {
            return new CobaltElement(nodes.Select(node => node.AsElement()));
        }

        /// <summary>
        /// Returns only the html elements from a list of nodes
        /// </summary>
        public static CobaltElement AsElement<T>(this IEnumerable<T> nodes) where T : ICobaltElement {
            return new CobaltElement(nodes.Select(node => node.AsElement()));
        }

        /// <summary>
        /// Returns only the html elements from a list of nodes
        /// </summary>
        public static IEnumerable<HtmlNode> MatchesSelector(this IEnumerable<HtmlNode> nodes, string selector) {
            CssSelector css = new CssSelector(selector, nodes);
            return css.GetMatches();
        }
        
        /// <summary>
        /// Loops and performs an action to each of the nodes
        /// </summary>
        public static IEnumerable<HtmlNode> Each(this IEnumerable<HtmlNode> nodes, Action<HtmlNode> with) {

            //loop and run the elements in order but make
            //sure that any changes to the collection don't 
            //end up killing the enumeration in the process
            List<HtmlNode> elements = nodes.ToList();
            for (int i = elements.Count; i --> 0;) {
                with(elements.ElementAt(0));
                elements.RemoveAt(0);
            }

            //return the original collection
            return nodes;
        }

        /// <summary>
        /// Returns only the html elements from a list of nodes
        /// </summary>
        public static IEnumerable<HtmlNode> HtmlElements(this IEnumerable<HtmlNode> nodes) {
            return nodes.Where(node => node.IsElement);
        }

        /// <summary>
        /// Returns only the html comments from a list of nodes
        /// </summary>
        public static IEnumerable<HtmlNode> HtmlComments(this IEnumerable<HtmlNode> nodes) {
            return nodes.Where(node => node.IsComment);
        }

        /// <summary>
        /// Returns only the html text items from a list of nodes
        /// </summary>
        public static IEnumerable<HtmlNode> HtmlText(this IEnumerable<HtmlNode> nodes) {
            return nodes.Where(node => node.IsText);
        }

        /// <summary>
        /// Returns only elements that have node information
        /// </summary>
        public static IEnumerable<HtmlNode> HtmlNodes(this IEnumerable<HtmlNode> nodes) {
            return nodes.Where(node => node.IsElement || node.IsDocument);
        }

        /// <summary>
        /// Returns if this is an input element or not
        /// </summary>
        public static bool IsInput(this HtmlNode node) {
            return node.Tag.Equals("input", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a radio button
        /// </summary>
        public static bool IsRadioButton(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("radio", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a checkbox
        /// </summary>
        public static bool IsCheckboxInput(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("checkbox", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a regular text box
        /// </summary>
        public static bool IsTextInput(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("text", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a file upload box
        /// </summary>
        public static bool IsFileInput(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("file", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML is a hidden input
        /// </summary>
        public static bool IsHiddenInput(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("hidden", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a password input
        /// </summary>
        public static bool IsPaswordInput(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("password", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a submit
        /// </summary>
        public static bool IsSubmitButton(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("submit", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a HTML node is a button of some sort
        /// </summary>
        public static bool IsButtonInput(this HtmlNode node) {
            return node.IsInput() && node["type"].ToString().Equals("radio", StringComparison.OrdinalIgnoreCase)
                || node.Tag.Equals("button", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns if a the node is a textarea element
        /// </summary>
        public static bool IsTextArea(this HtmlNode node) {
            return node.Tag.Equals("textarea", StringComparison.OrdinalIgnoreCase);
        }
    
    }

}
