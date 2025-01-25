using System.Text.RegularExpressions;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Alpha.Helpers
{
    internal class HighlightingDefinition : IHighlightingDefinition
    {
        // 高亮的名称
        public string Name => "AlphaHighlighting";

        // 主规则集，定义文本的基本高亮规则
        public HighlightingRuleSet MainRuleSet => CreateMainRuleSet();

        // 命名高亮颜色
        public IEnumerable<HighlightingColor> NamedHighlightingColors =>
 [
     new HighlightingColor
    {
        Name = "function",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(222, 222, 95))
    },
    new HighlightingColor
    {
        Name = "bracket",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(220, 220, 220))  // 亮灰色，让括号更柔和
    },
    new HighlightingColor
    {
        Name = "keyword",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(197, 134, 192))  // 优雅的紫色，关键字更突出
    },
    new HighlightingColor
    {
        Name = "number",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(181, 206, 168))  // 柔和的青绿色，数字更易读
    },
    new HighlightingColor
    {
        Name = "otherSymbols",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(128, 128, 128))  // 中性灰色，符号不太显眼
    },
    new HighlightingColor
    {
        Name = "variable",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(156, 220, 254))  // 天蓝色，变量更醒目
    },
    new HighlightingColor
    {
        Name = "string",
        Foreground = new SimpleHighlightingBrush(Color.FromRgb(206, 145, 120))  // 暖棕色，字符串更温和
    },
            new HighlightingColor
                {
                    Name = "comment",
                    Foreground = new SimpleHighlightingBrush(Color.FromRgb(64, 128, 64))  // 注释的颜色（橄榄色）
                },
            new HighlightingColor
            {
                Name = "placeholder",
                Foreground = new SimpleHighlightingBrush(Color.FromRgb(255, 183, 130))
            },
            new HighlightingColor
            {
                Name = "link",
                Foreground = new SimpleHighlightingBrush(Color.FromRgb(255, 183, 130))
            }
 ];

        // 属性（如果需要，通常会有语法相关的属性）
        public IDictionary<string, string> Properties => new Dictionary<string, string>
    {
        { "Name", Name }
    };

        // 根据名称获取特定的颜色
        public HighlightingColor? GetNamedColor(string name)
        {
            foreach (HighlightingColor color in NamedHighlightingColors)
            {
                if (color.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return color;
                }
            }
            return null; // 未找到对应名称的颜色
        }

        // 根据名称获取特定的规则集
        public HighlightingRuleSet? GetNamedRuleSet(string name)
        {
            // 这里可以根据不同语言或者规则返回不同的规则集
            return name == "default" ? MainRuleSet : null;
        }

        // 创建主要的规则集
        private HighlightingRuleSet CreateMainRuleSet()
        {
            HighlightingRuleSet ruleSet = new();

            // 改进的函数规则，匹配定义和调用的函数名
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"([a-zA-Z_][a-zA-Z0-9_]*)\s*(?=\()"),
                Color = GetNamedColor("function")
            });

            // 括号规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"[\(\)\[\]\{\}]"),
                Color = GetNamedColor("bracket")
            });

            // 关键字规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"\b(if|else|for|while|return|break|continue|import|class|if_else|is_nan|equal|and|or|not_equal|not)\b"),
                Color = GetNamedColor("keyword")
            });

            // 数字规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"\b\d+(\.\d+)?\b"),
                Color = GetNamedColor("number")
            });

            // 注释规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"(#.*|//.*)$"),
                Color = GetNamedColor("comment")
            });

            // 其他符号规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"[=+\-*/<>!&|,;:._]"),
                Color = GetNamedColor("otherSymbols")
            });

            // 变量规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*(?=\s*=)"),
                Color = GetNamedColor("variable")
            });

            // 变量规则
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"\$(\d+)\b"),
                Color = GetNamedColor("placeholder")
            });

            // 字符串规则：匹配单引号和双引号的字符串
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"(['""].*?['""])"),
                Color = GetNamedColor("string")
            });

            // 链接规则http(s):
            ruleSet.Rules.Add(new HighlightingRule
            {
                Regex = new Regex(@"(http|https)://[^\s]+"),
                Color = GetNamedColor("link")
            });

            return ruleSet;
        }
    }
}