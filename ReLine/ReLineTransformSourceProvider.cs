using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Formatting;

namespace ReLine
{
    [Export(typeof(ILineTransformSourceProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.EmbeddedPeekTextView)]
    [TextViewRole(PredefinedTextViewRoles.Printable)]
    internal sealed class ReLineTransformSourceProvider : ILineTransformSourceProvider
    {
        public ILineTransformSource Create(IWpfTextView textView)
        {
            if (textView.Roles.Contains("LEFTDIFF") ||
                textView.Roles.Contains("RIGHTDIFF") ||
                textView.Roles.Contains("VSMERGEDEFAULT"))
            {
                return null;
            }
            return ReLineTransformSource.Create(textView);
        }
    }
}
