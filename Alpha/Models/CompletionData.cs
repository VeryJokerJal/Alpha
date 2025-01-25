using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Alpha.Models
{
    public enum CompletionType
    {
        Variable,
        Function,
        Placeholder,
        Other
    }

    public class CompletionData : ICompletionData
    {
        public ImageSource? Image { get; set; }

        public string? Text { get; set; }

        public object? Content => Text;

        public object? Description { get; set; }

        public double Priority { get; set; }

        public string SplitText { get; set; }

        public CompletionType CompletionType { get; set; }

        // 构造函数，用于初始化补全项的文本
        public CompletionData(string text, CompletionType completionType)
        {
            Text = text;
            SplitText = string.Empty;
            CompletionType = completionType;
            Initialize();
        }

        private void Initialize()
        {
            ResourceDictionary resourceDictionary = Application.Current.Resources;
            switch (CompletionType)
            {
                case CompletionType.Variable:
                    Image = resourceDictionary["VariableIcon"] as ImageSource;
                    Description = "变量";
                    break;
                case CompletionType.Function:
                    Image = resourceDictionary["FunctionIcon"] as ImageSource;
                    Description = "函数方法";
                    break;
                case CompletionType.Placeholder:
                    Image = resourceDictionary["PlaceholderIcon"] as ImageSource;
                    Description = "占位符";
                    break;
                case CompletionType.Other:
                    Image = resourceDictionary["KeywordIcon"] as ImageSource;
                    Description = "关键词";
                    break;
            }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment.Offset - SplitText.Length, SplitText.Length, Text);
        }
    }
}
