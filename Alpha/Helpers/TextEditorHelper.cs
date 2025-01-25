using System.Windows;
using ICSharpCode.AvalonEdit;

namespace Alpha.Helpers
{
    public static class TextEditorHelper
    {
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached(
                "BindableText",
                typeof(string),
                typeof(TextEditorHelper),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableTextChanged));

        public static string GetBindableText(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject obj, string value)
        {
            obj.SetValue(BindableTextProperty, value);
        }

        private static void OnBindableTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor textEditor)
            {
                // 保存当前的光标位置
                int caretOffset = textEditor.CaretOffset;

                textEditor.TextChanged -= OnTextEditorTextChanged;

                // 暂停撤销/恢复操作
                textEditor.Document.UndoStack.StartUndoGroup();

                if (e.NewValue is string newText)
                {
                    // 替换文档内容而不重置撤销堆栈
                    textEditor.Document.Replace(0, textEditor.Document.TextLength, newText);
                }

                // 恢复光标位置
                if (caretOffset <= textEditor.Document.TextLength)
                {
                    textEditor.CaretOffset = caretOffset;
                }
                else
                {
                    // 如果新文本比原来的短，将光标设置到文本末尾
                    textEditor.CaretOffset = textEditor.Document.TextLength;
                }

                // 结束撤销/恢复操作
                textEditor.Document.UndoStack.EndUndoGroup();
                textEditor.Document.UndoStack.MarkAsOriginalFile();

                textEditor.TextChanged += OnTextEditorTextChanged;
            }
        }

        private static void OnTextEditorTextChanged(object? sender, EventArgs e)
        {
            if (sender is TextEditor textEditor)
            {
                SetBindableText(textEditor, textEditor.Text);
            }
        }
    }
}
