using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace ReLine
{
    internal sealed class ReLineTransformSource : ILineTransformSource
    {
        private readonly static LineTransform _transformDefault;
        private readonly static LineTransform _transformToSmall;
        private readonly static Dictionary<string, Dictionary<double, double>> _replaceOriginalVars;

        static ReLineTransformSource()
        {
            _transformDefault = new LineTransform(0, 0, 1);
            _transformToSmall = new LineTransform(0, 0, 0.75);

            //Dictionary<fieldname, Dictionary<ReplaceThisValue, WithThatValue>>
            //The replace Function will be executed more than once...
            //Prevent the double execution with small differences like 0.001
            _replaceOriginalVars = new Dictionary<string, Dictionary<double, double>>() {
                { "_height", new Dictionary<double, double>() { //Line height
                    { 13, 11 }, //small transformed lines
                    { 16, 13.001 }, //normal lines
                    //{ 29, 26 }, //lines with codelens //Does bounce while writing code
                }},
                { "_topSpace", new Dictionary<double, double>() {
                    //{ 13, 12 }, //space after line with codelens //Does bounce while writing code
                }},
                { "_textHeight", new Dictionary<double, double>() { 
                    //{ 12, 11 }, //small transformed lines //Does bounce while writing code
                    //{ 15, 14 }, //normal lines //Does bounce while writing code
                }},
            };
        }

        public static ReLineTransformSource Create(IWpfTextView view)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new ReLineTransformSource(view));
        }

        private ReLineTransformSource(IWpfTextView view)
        {
        }

        public LineTransform GetLineTransform(ITextViewLine line, double yPosition, ViewRelativePosition placement)
        {
            TryChangeField(line, "_textHeight"); //Height of Cursor and the rectangle of the current line
            TryChangeField(line, "_height"); //Line height
            TryChangeField(line, "_topSpace"); //space after line with codelens

            //TryChangeField(line, "_unscaledBaseline"); // Text Baseline, has only 12
            //TryChangeField(line, "_unscaledBottomSpace"); //no Data
            //TryChangeField(line, "_unscaledTextHeigh"); //no Data
            //TryChangeField(line, "_unscaledTopSpace"); //only 0
            //TryChangeField(line, "_top"); //Different unimportant things like the last '}'
            //TryChangeField(line, "_deltaY"); //only 0

            for (int i = line.Start; i < line.End; i++)
            {
                char item = line.Snapshot[i];
                if (char.IsLetterOrDigit(item))
                {
                    return _transformDefault;
                }
            }
            return _transformToSmall;
        }

        private void TryChangeField(object target, string name)
        {
            var v = TryGetField(target, name);
            if (v != null)
            {
                var d = Convert.ToDouble(v);
                if (_replaceOriginalVars.TryGetValue(name, out Dictionary<double, double> valdic))
                {
                    if (valdic.TryGetValue(d, out double valdbl))
                    {
                        d = valdbl;
                        TrySetField(target, name, d);
                    }
                }
#if DEBUG
                Debug.WriteLine($"Field {name}: {v} -> {d}");
#endif
            }
        }

        private object TryGetField(object target, string name)
        {
            var fld = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (fld != null)
            {
                return fld.GetValue(target);
            }
            return null;
        }

        private void TrySetField(object target, string name, object value)
        {
            if (value != null)
            {
                var fld = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (fld != null)
                {
                    fld.SetValue(target, value);
                }
            }
        }
    }
}
