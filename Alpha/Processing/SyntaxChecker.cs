using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Alpha.Processing
{
    // 数据类型枚举
    public enum DataType
    {
        Matrix,
        Group,
        Universe,
        Vector
    }

    // 参数定义
    public class ParameterDefinition
    {
        public string? Name { get; set; }
        public DataType Type { get; set; }
        public bool HasDefault { get; set; }
        public string? DefaultValue { get; set; }
    }

    // 函数定义
    public class FunctionDefinition
    {
        public string? Name { get; set; }
        public List<ParameterDefinition>? Parameters { get; set; }

        // 解析函数定义字符串
        public static FunctionDefinition FromDefinitionString(string definition)
        {
            // 示例： "add(x: Vector, y: Vector, filter: Vector = false)"
            string pattern = @"(\w+)\((.*?)\)";
            Match match = Regex.Match(definition, pattern);
            if (!match.Success)
            {
                throw new Exception($"Invalid definition format: {definition}");
            }

            string funcName = match.Groups[1].Value;
            string paramsString = match.Groups[2].Value;

            List<ParameterDefinition> parameters = [];
            List<string> paramsSplit = SplitParameters(paramsString);

            foreach (string param in paramsSplit)
            {
                string p = param.Trim();
                ParameterDefinition paramDef = new();
                if (p.Contains("="))
                {
                    string[] parts = p.Split('=');
                    paramDef.HasDefault = true;
                    paramDef.DefaultValue = parts[1].Trim();
                    string[] nameType = parts[0].Trim().Split(':');
                    paramDef.Name = nameType[0].Trim();
                    paramDef.Type = ParseDataType(nameType[1].Trim());
                }
                else
                {
                    paramDef.HasDefault = false;
                    string[] nameType = p.Split(':');
                    paramDef.Name = nameType[0].Trim();
                    paramDef.Type = ParseDataType(nameType[1].Trim());
                }
                parameters.Add(paramDef);
            }

            return new FunctionDefinition
            {
                Name = funcName,
                Parameters = parameters
            };
        }

        private static List<string> SplitParameters(string paramsString)
        {
            List<string> result = [];
            string current = "";
            bool inQuotes = false;
            foreach (char c in paramsString)
            {
                if (c is '"' or '\'')
                {
                    inQuotes = !inQuotes;
                }
                if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            if (!string.IsNullOrWhiteSpace(current))
            {
                result.Add(current.Trim());
            }

            return result;
        }

        private static DataType ParseDataType(string typeStr)
        {
            return typeStr.ToLower() switch
            {
                "matrix" => DataType.Matrix,
                "group" => DataType.Group,
                "universe" => DataType.Universe,
                "vector" => DataType.Vector,
                _ => throw new Exception($"Unsupported data type: {typeStr}")
            };
        }
    }

    // 令牌类型枚举
    public enum TokenType
    {
        FunctionName,
        Parameter,
        Symbol,         // 如 '=', ','
        ParenthesisOpen,
        ParenthesisClose,
        Value,          // 值，如数字、布尔值
        VariableName,
        EndOfLine
    }

    // 令牌类
    public class Token
    {
        public TokenType Type { get; set; }
        public string? Value { get; set; }
        public int Position { get; set; } // 位置，用于错误报告

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }

    // 词法分析器
    public class Tokenizer
    {
        private readonly string _input;
        private int _position;
        private readonly int _length;

        public Tokenizer(string input)
        {
            _input = input;
            _position = 0;
            _length = input.Length;
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = [];

            while (_position < _length)
            {
                char current = _input[_position];

                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                if (current == '(')
                {
                    tokens.Add(new Token { Type = TokenType.ParenthesisOpen, Value = "(", Position = _position });
                    _position++;
                    continue;
                }
                if (current == ')')
                {
                    tokens.Add(new Token { Type = TokenType.ParenthesisClose, Value = ")", Position = _position });
                    _position++;
                    continue;
                }
                if (current == ',')
                {
                    tokens.Add(new Token { Type = TokenType.Symbol, Value = ",", Position = _position });
                    _position++;
                    continue;
                }
                if (current == '=')
                {
                    tokens.Add(new Token { Type = TokenType.Symbol, Value = "=", Position = _position });
                    _position++;
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    string identifier = ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
                    // Determine if it's a function name or a variable/parameter
                    tokens.Add(new Token { Type = TokenType.FunctionName, Value = identifier, Position = _position - identifier.Length });
                    continue;
                }

                if (char.IsDigit(current) || current == '.' || current == '-')
                {
                    string number = ReadWhile(c => char.IsDigit(c) || c == '.' || c == '-');
                    tokens.Add(new Token { Type = TokenType.Value, Value = number, Position = _position - number.Length });
                    continue;
                }

                if (current is '"' or '\'')
                {
                    string str = ReadString(current);
                    tokens.Add(new Token { Type = TokenType.Value, Value = str, Position = _position - str.Length - 2 });
                    continue;
                }

                // 未识别的符号
                tokens.Add(new Token { Type = TokenType.Symbol, Value = current.ToString(), Position = _position });
                _position++;
            }

            tokens.Add(new Token { Type = TokenType.EndOfLine, Value = "EOF", Position = _position });
            return tokens;
        }

        private string ReadWhile(Func<char, bool> condition)
        {
            int start = _position;
            while (_position < _length && condition(_input[_position]))
            {
                _position++;
            }
            return _input[start.._position];
        }

        private string ReadString(char quote)
        {
            _position++; // 跳过开始的引号
            int start = _position;
            while (_position < _length && _input[_position] != quote)
            {
                _position++;
            }
            if (_position >= _length)
            {
                throw new Exception("未闭合的字符串字面量");
            }

            string str = _input[start.._position];
            _position++; // 跳过结束的引号
            return str;
        }
    }

    // 语法检查器
    public class SyntaxChecker
    {
        private readonly List<FunctionDefinition> _functionDefinitions;
        private readonly Dictionary<string, DataType> _variables; // 变量名及其类型

        public SyntaxChecker(List<FunctionDefinition> functionDefinitions)
        {
            _functionDefinitions = functionDefinitions;
            _variables = [];
        }

        public List<string> CheckSyntax(List<string> codeLines)
        {
            List<string> errors = [];

            foreach (string line in codeLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    Tokenizer tokenizer = new(line);
                    List<Token> tokens = tokenizer.Tokenize();
                    if (tokens.Count == 0)
                    {
                        continue;
                    }

                    // 简单判断是否是变量赋值
                    int assignIndex = tokens.FindIndex(t => t.Type == TokenType.Symbol && t.Value == "=");
                    if (assignIndex != -1)
                    {
                        // 变量赋值
                        Token varNameToken = tokens[0];
                        if (varNameToken.Type != TokenType.FunctionName)
                        {
                            errors.Add($"错误: 无效的变量名 '{varNameToken.Value}' 在行: {line}");
                            continue;
                        }
                        string? varName = varNameToken.Value;

                        // 表达式部分
                        List<Token> exprTokens = tokens.GetRange(assignIndex + 1, tokens.Count - assignIndex - 2); // 排除EOF
                        DataType? exprType = EvaluateExpression(exprTokens, line, errors);
                        if (!string.IsNullOrEmpty(varName) && exprType != null)
                        {
                            if (_variables.ContainsKey(varName))
                            {
                                // 可选：检查类型一致性
                                if (_variables[varName] != exprType)
                                {
                                    errors.Add($"错误: 变量 '{varName}' 类型不一致。在行: {line}");
                                }
                            }
                            else
                            {

                                _variables[varName] = exprType.Value;
                            }
                        }
                    }
                    else
                    {
                        // 仅函数调用
                        DataType? exprType = EvaluateExpression(tokens, line, errors);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"错误: {ex.Message} 在行: {line}");
                }
            }

            return errors;
        }

        private DataType? EvaluateExpression(List<Token> tokens, string line, List<string> errors)
        {
            if (tokens.Count == 0)
            {
                return null;
            }

            // 假设表达式是一个函数调用
            if (tokens[0].Type != TokenType.FunctionName)
            {
                errors.Add($"错误: 表达式必须以函数调用开始。在行: {line}");
                return null;
            }

            string? funcName = tokens[0]?.Value;
            FunctionDefinition? funcDef = _functionDefinitions.Find(f => f.Name == funcName);
            if (funcDef == null)
            {
                errors.Add($"错误: 未定义的函数 '{funcName}' 在行: {line}");
                return null;
            }

            // 检查括号
            if (tokens.Count < 2 || tokens[1].Type != TokenType.ParenthesisOpen)
            {
                errors.Add($"错误: 函数调用缺少 '(' 在行: {line}");
                return null;
            }

            // 查找关闭括号
            int closeParenIndex = tokens.FindIndex(t => t.Type == TokenType.ParenthesisClose);
            if (closeParenIndex == -1)
            {
                errors.Add($"错误: 函数调用缺少 ')' 在行: {line}");
                return null;
            }

            // 提取参数令牌
            List<Token> argTokens = tokens.GetRange(2, closeParenIndex - 2);
            List<string> args = SplitArguments(argTokens);

            // 验证参数数量
            int minParams = 0;
            foreach (ParameterDefinition p in funcDef.Parameters ?? [])
            {
                if (!p.HasDefault)
                {
                    minParams++;
                }
            }

            if (args.Count < minParams || args.Count > funcDef.Parameters?.Count)
            {
                errors.Add($"错误: 函数 '{funcName}' 参数数量不匹配。期望最少 {minParams} 个参数，最多 {funcDef.Parameters?.Count} 个参数。在行: {line}");
                return null;
            }

            // 验证每个参数
            for (int i = 0; i < args.Count; i++)
            {
                string arg = args[i];
                ParameterDefinition? paramDef = funcDef.Parameters?[i];
                DataType? argType = GetArgumentType(arg, line, errors);
                if (argType == null || paramDef == null)
                {
                    // 错误已记录
                    continue;
                }

                if (argType != paramDef.Type)
                {
                    errors.Add($"错误: 参数 '{paramDef.Name}' 期望类型为 '{paramDef.Type}', 但接收到类型 '{argType}' 在行: {line}");
                }
            }

            // 假设函数的返回类型与最后一个参数类型相关（根据实际需求调整）
            // 这里需要根据具体的函数定义来决定返回类型
            // 为简化，假设所有函数返回 Vector 类型
            return DataType.Vector;
        }

        private List<string> SplitArguments(List<Token> tokens)
        {
            List<string> args = [];
            string current = "";
            int parenDepth = 0;

            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.ParenthesisOpen)
                {
                    parenDepth++;
                    current += token.Value;
                }
                else if (token.Type == TokenType.ParenthesisClose)
                {
                    parenDepth--;
                    current += token.Value;
                }
                else if (token.Type == TokenType.Symbol && token.Value == "," && parenDepth == 0)
                {
                    args.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += token.Value + " ";
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                args.Add(current.Trim());
            }

            return args;
        }

        private DataType? GetArgumentType(string arg, string line, List<string> errors)
        {
            // 简单判断参数类型
            if (double.TryParse(arg, out _))
            {
                return DataType.Matrix; // 假设数值为 Matrix 类型
            }

            if (arg.Equals("true", StringComparison.OrdinalIgnoreCase) || arg.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return DataType.Vector; // 布尔值为 Vector 类型
            }

            if (_variables.ContainsKey(arg))
            {
                return _variables[arg];
            }
            // 可能是一个函数调用
            if (Regex.IsMatch(arg, @"^\w+\(.*\)$"))
            {
                // 递归检查
                Tokenizer tokenizer = new(arg);
                List<Token> tokens = tokenizer.Tokenize();
                return EvaluateExpression(tokens, line, errors);
            }

            errors.Add($"错误: 未知的参数或变量 '{arg}' 在行: {line}");
            return null;
        }
    }

    internal class SyntaxCheckerProgram
    {
        private static void Test()
        {
            // 从提供的JSON加载函数定义
            string json = @"[
                {
                    'name': 'add',
                    'definition': 'add(x: Vector, y: Vector, filter: Vector = false)'
                },
                {
                    'name': 'log',
                    'definition': 'log(x: Vector)'
                },
                {
                    'name': 'subtract',
                    'definition': 'subtract(x: Vector, y: Vector, filter: Vector = false)'
                },
                {
                    'name': 'multiply',
                    'definition': 'multiply(x: Vector, y: Vector, filter: Vector = false)'
                },
                {
                    'name': 'divide',
                    'definition': 'divide(x: Vector, y: Vector)'
                },
                // 添加其他函数定义...
            ]";

            List<FunctionDefinition> funcList = [];
            List<dynamic> funcs = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? [];
            foreach (dynamic func in funcs)
            {
                string name = func.name;
                string definition = func.definition;
                try
                {
                    FunctionDefinition funcDef = FunctionDefinition.FromDefinitionString(definition);
                    funcList.Add(funcDef);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"错误解析函数 '{name}': {ex.Message}");
                }
            }

            // 初始化语法检测器
            SyntaxChecker checker = new(funcList);

            Console.WriteLine("请输入要检查的代码（输入空行结束）：");
            List<string> codeLines = [];
            string? line;
            while (!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                codeLines.Add(line);
            }

            // 检查语法
            List<string> errors = checker.CheckSyntax(codeLines);

            if (errors.Count == 0)
            {
                Console.WriteLine("语法检查通过！");
            }
            else
            {
                Console.WriteLine("语法检查发现以下错误：");
                foreach (string error in errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}
