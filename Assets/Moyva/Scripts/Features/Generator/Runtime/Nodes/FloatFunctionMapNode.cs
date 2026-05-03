using System;
using System.Collections.Generic;
using System.Globalization;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Float Function Map", "Generators", "Генерує карту float[,] з формули f(x,y). Підтримує math-функції та shape-функції для кіл, контурів і прямокутників.")]
    public sealed class FloatFunctionMapNode : NodeBase
    {
        [Header("Formula")]
        [Tooltip("Формула для f(x,y). Доступні змінні: x,y,u,v,nx,ny,w,h,pi,e.")]
        [SerializeField] private string _expression = "circle(u,v,0.5,0.5,0.25,0.01)";
        [SerializeField] private bool _clamp01 = true;

        public override string Title => "Float Function Map";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string>("Expression (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("FloatMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            string expression = _expression;
            if (inputs.Length > 0 && inputs[0] is string inputExpression && !string.IsNullOrWhiteSpace(inputExpression))
                expression = inputExpression;

            if (string.IsNullOrWhiteSpace(expression))
                return NodeOutput.Error("Expression is empty.");

            int width = Mathf.Max(1, context.MapSize.x);
            int height = Mathf.Max(1, context.MapSize.y);

            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Map size is invalid. Set Shared Settings map size.");

            AstNode root;
            try
            {
                root = new ExpressionParser(expression).Parse();
            }
            catch (Exception ex)
            {
                return NodeOutput.Error($"Expression parse error: {ex.Message}");
            }

            var map = new float[width, height];
            var scope = new EvalScope(width, height);

            try
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        scope.SetPixel(x, y);
                        float value = root.Eval(scope);
                        map[x, y] = _clamp01 ? Mathf.Clamp01(value) : value;
                        context.CountIteration();
                    }
                }
            }
            catch (Exception ex)
            {
                return NodeOutput.Error($"Expression eval error: {ex.Message}");
            }

            return NodeOutput.Success(map);
        }

        private sealed class EvalScope
        {
            private readonly int _width;
            private readonly int _height;
            private int _x;
            private int _y;

            public EvalScope(int width, int height)
            {
                _width = width;
                _height = height;
            }

            public void SetPixel(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public float ResolveVariable(string name)
            {
                if (name.Equals("x", StringComparison.OrdinalIgnoreCase))
                    return _x;
                if (name.Equals("y", StringComparison.OrdinalIgnoreCase))
                    return _y;
                if (name.Equals("u", StringComparison.OrdinalIgnoreCase))
                    return _width > 1 ? (float)_x / (_width - 1f) : 0f;
                if (name.Equals("v", StringComparison.OrdinalIgnoreCase))
                    return _height > 1 ? (float)_y / (_height - 1f) : 0f;
                if (name.Equals("nx", StringComparison.OrdinalIgnoreCase))
                    return (_width > 1 ? (float)_x / (_width - 1f) : 0f) * 2f - 1f;
                if (name.Equals("ny", StringComparison.OrdinalIgnoreCase))
                    return (_height > 1 ? (float)_y / (_height - 1f) : 0f) * 2f - 1f;
                if (name.Equals("w", StringComparison.OrdinalIgnoreCase) || name.Equals("width", StringComparison.OrdinalIgnoreCase))
                    return _width;
                if (name.Equals("h", StringComparison.OrdinalIgnoreCase) || name.Equals("height", StringComparison.OrdinalIgnoreCase))
                    return _height;
                if (name.Equals("pi", StringComparison.OrdinalIgnoreCase))
                    return Mathf.PI;
                if (name.Equals("e", StringComparison.OrdinalIgnoreCase))
                    return Mathf.Exp(1f);

                throw new InvalidOperationException($"Unknown variable '{name}'.");
            }
        }

        private abstract class AstNode
        {
            public abstract float Eval(EvalScope scope);
        }

        private sealed class NumberNode : AstNode
        {
            private readonly float _value;

            public NumberNode(float value)
            {
                _value = value;
            }

            public override float Eval(EvalScope scope)
            {
                return _value;
            }
        }

        private sealed class VariableNode : AstNode
        {
            private readonly string _name;

            public VariableNode(string name)
            {
                _name = name;
            }

            public override float Eval(EvalScope scope)
            {
                return scope.ResolveVariable(_name);
            }
        }

        private sealed class UnaryNode : AstNode
        {
            private readonly char _op;
            private readonly AstNode _arg;

            public UnaryNode(char op, AstNode arg)
            {
                _op = op;
                _arg = arg;
            }

            public override float Eval(EvalScope scope)
            {
                float v = _arg.Eval(scope);
                return _op == '-' ? -v : v;
            }
        }

        private sealed class BinaryNode : AstNode
        {
            private readonly char _op;
            private readonly AstNode _left;
            private readonly AstNode _right;

            public BinaryNode(char op, AstNode left, AstNode right)
            {
                _op = op;
                _left = left;
                _right = right;
            }

            public override float Eval(EvalScope scope)
            {
                float a = _left.Eval(scope);
                float b = _right.Eval(scope);

                return _op switch
                {
                    '+' => a + b,
                    '-' => a - b,
                    '*' => a * b,
                    '/' => Mathf.Abs(b) < 1e-7f ? 0f : a / b,
                    '^' => Mathf.Pow(a, b),
                    _ => throw new InvalidOperationException($"Unsupported operator '{_op}'.")
                };
            }
        }

        private sealed class FunctionNode : AstNode
        {
            private readonly string _name;
            private readonly AstNode[] _args;

            public FunctionNode(string name, AstNode[] args)
            {
                _name = name;
                _args = args;
            }

            public override float Eval(EvalScope scope)
            {
                var values = new float[_args.Length];
                for (int i = 0; i < _args.Length; i++)
                    values[i] = _args[i].Eval(scope);

                return EvaluateFunction(_name, values);
            }
        }

        private static float EvaluateFunction(string name, float[] args)
        {
            if (name.Equals("sin", StringComparison.OrdinalIgnoreCase)) return Mathf.Sin(Arg(args, 0));
            if (name.Equals("cos", StringComparison.OrdinalIgnoreCase)) return Mathf.Cos(Arg(args, 0));
            if (name.Equals("tan", StringComparison.OrdinalIgnoreCase)) return Mathf.Tan(Arg(args, 0));
            if (name.Equals("abs", StringComparison.OrdinalIgnoreCase)) return Mathf.Abs(Arg(args, 0));
            if (name.Equals("sqrt", StringComparison.OrdinalIgnoreCase)) return Mathf.Sqrt(Mathf.Max(0f, Arg(args, 0)));
            if (name.Equals("pow", StringComparison.OrdinalIgnoreCase)) return Mathf.Pow(Arg(args, 0), Arg(args, 1));
            if (name.Equals("min", StringComparison.OrdinalIgnoreCase)) return Mathf.Min(Arg(args, 0), Arg(args, 1));
            if (name.Equals("max", StringComparison.OrdinalIgnoreCase)) return Mathf.Max(Arg(args, 0), Arg(args, 1));
            if (name.Equals("clamp", StringComparison.OrdinalIgnoreCase)) return Mathf.Clamp(Arg(args, 0), Arg(args, 1), Arg(args, 2));
            if (name.Equals("saturate", StringComparison.OrdinalIgnoreCase)) return Mathf.Clamp01(Arg(args, 0));
            if (name.Equals("step", StringComparison.OrdinalIgnoreCase)) return Arg(args, 0) < Arg(args, 1) ? 0f : 1f;
            if (name.Equals("smoothstep", StringComparison.OrdinalIgnoreCase))
            {
                float edge0 = Arg(args, 0);
                float edge1 = Arg(args, 1);
                float t = Mathf.InverseLerp(edge0, edge1, Arg(args, 2));
                return t * t * (3f - 2f * t);
            }
            if (name.Equals("floor", StringComparison.OrdinalIgnoreCase)) return Mathf.Floor(Arg(args, 0));
            if (name.Equals("ceil", StringComparison.OrdinalIgnoreCase)) return Mathf.Ceil(Arg(args, 0));
            if (name.Equals("round", StringComparison.OrdinalIgnoreCase)) return Mathf.Round(Arg(args, 0));
            if (name.Equals("frac", StringComparison.OrdinalIgnoreCase))
            {
                float v = Arg(args, 0);
                return v - Mathf.Floor(v);
            }
            if (name.Equals("length", StringComparison.OrdinalIgnoreCase))
                return Mathf.Sqrt(Arg(args, 0) * Arg(args, 0) + Arg(args, 1) * Arg(args, 1));
            if (name.Equals("distance", StringComparison.OrdinalIgnoreCase))
            {
                float dx = Arg(args, 0) - Arg(args, 2);
                float dy = Arg(args, 1) - Arg(args, 3);
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            if (name.Equals("circle", StringComparison.OrdinalIgnoreCase))
            {
                float x = Arg(args, 0);
                float y = Arg(args, 1);
                float cx = Arg(args, 2);
                float cy = Arg(args, 3);
                float r = Mathf.Max(0f, Arg(args, 4));
                float soft = args.Length > 5 ? Mathf.Max(1e-6f, Arg(args, 5)) : 1e-4f;
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                return 1f - Mathf.SmoothStep(r - soft, r + soft, d);
            }

            if (name.Equals("ring", StringComparison.OrdinalIgnoreCase))
            {
                float x = Arg(args, 0);
                float y = Arg(args, 1);
                float cx = Arg(args, 2);
                float cy = Arg(args, 3);
                float r = Mathf.Max(0f, Arg(args, 4));
                float t = Mathf.Max(0f, Arg(args, 5));
                float soft = args.Length > 6 ? Mathf.Max(1e-6f, Arg(args, 6)) : 1e-4f;
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float inner = Mathf.SmoothStep(r - t - soft, r - t + soft, d);
                float outer = 1f - Mathf.SmoothStep(r + t - soft, r + t + soft, d);
                return Mathf.Clamp01(inner * outer);
            }

            if (name.Equals("box", StringComparison.OrdinalIgnoreCase))
            {
                float x = Arg(args, 0);
                float y = Arg(args, 1);
                float cx = Arg(args, 2);
                float cy = Arg(args, 3);
                float hx = Mathf.Max(0f, Arg(args, 4));
                float hy = Mathf.Max(0f, Arg(args, 5));
                float soft = args.Length > 6 ? Mathf.Max(1e-6f, Arg(args, 6)) : 1e-4f;

                float qx = Mathf.Abs(x - cx) - hx;
                float qy = Mathf.Abs(y - cy) - hy;
                float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                float sd = outside + inside;
                return 1f - Mathf.SmoothStep(-soft, soft, sd);
            }

            throw new InvalidOperationException($"Unknown function '{name}'.");
        }

        private static float Arg(float[] args, int index)
        {
            if (index < 0 || index >= args.Length)
                throw new InvalidOperationException("Not enough function arguments.");
            return args[index];
        }

        private sealed class ExpressionParser
        {
            private readonly List<Token> _tokens;
            private int _index;

            public ExpressionParser(string expression)
            {
                _tokens = Tokenize(expression);
            }

            public AstNode Parse()
            {
                var root = ParseExpression();
                Expect(TokenType.End, "Unexpected trailing tokens.");
                return root;
            }

            private AstNode ParseExpression()
            {
                var node = ParseTerm();
                while (MatchOperator('+') || MatchOperator('-'))
                {
                    char op = Previous().Operator;
                    var right = ParseTerm();
                    node = new BinaryNode(op, node, right);
                }
                return node;
            }

            private AstNode ParseTerm()
            {
                var node = ParsePower();
                while (MatchOperator('*') || MatchOperator('/'))
                {
                    char op = Previous().Operator;
                    var right = ParsePower();
                    node = new BinaryNode(op, node, right);
                }
                return node;
            }

            private AstNode ParsePower()
            {
                var node = ParseUnary();
                if (MatchOperator('^'))
                {
                    char op = Previous().Operator;
                    var right = ParsePower();
                    node = new BinaryNode(op, node, right);
                }
                return node;
            }

            private AstNode ParseUnary()
            {
                if (MatchOperator('+') || MatchOperator('-'))
                {
                    char op = Previous().Operator;
                    return new UnaryNode(op, ParseUnary());
                }

                return ParsePrimary();
            }

            private AstNode ParsePrimary()
            {
                if (Match(TokenType.Number))
                    return new NumberNode(Previous().Number);

                if (Match(TokenType.Identifier))
                {
                    string name = Previous().Text;
                    if (Match(TokenType.LeftParen))
                    {
                        var args = new List<AstNode>();
                        if (!Check(TokenType.RightParen))
                        {
                            do
                            {
                                args.Add(ParseExpression());
                            }
                            while (Match(TokenType.Comma));
                        }

                        Expect(TokenType.RightParen, "Expected ')' after function arguments.");
                        return new FunctionNode(name, args.ToArray());
                    }

                    return new VariableNode(name);
                }

                if (Match(TokenType.LeftParen))
                {
                    var node = ParseExpression();
                    Expect(TokenType.RightParen, "Expected ')' after expression.");
                    return node;
                }

                throw new InvalidOperationException($"Unexpected token '{Peek().Text}'.");
            }

            private static List<Token> Tokenize(string expression)
            {
                var tokens = new List<Token>();
                int i = 0;
                while (i < expression.Length)
                {
                    char c = expression[i];

                    if (char.IsWhiteSpace(c))
                    {
                        i++;
                        continue;
                    }

                    if (char.IsDigit(c) || c == '.')
                    {
                        int start = i;
                        while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                            i++;

                        string text = expression.Substring(start, i - start);
                        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                            throw new InvalidOperationException($"Invalid number '{text}'.");

                        tokens.Add(Token.FromNumber(number, text));
                        continue;
                    }

                    if (char.IsLetter(c) || c == '_')
                    {
                        int start = i;
                        while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                            i++;

                        string ident = expression.Substring(start, i - start);
                        tokens.Add(Token.Identifier(ident));
                        continue;
                    }

                    switch (c)
                    {
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case '^':
                            tokens.Add(Token.FromOperator(c));
                            i++;
                            break;
                        case '(':
                            tokens.Add(Token.Simple(TokenType.LeftParen, "("));
                            i++;
                            break;
                        case ')':
                            tokens.Add(Token.Simple(TokenType.RightParen, ")"));
                            i++;
                            break;
                        case ',':
                            tokens.Add(Token.Simple(TokenType.Comma, ","));
                            i++;
                            break;
                        default:
                            throw new InvalidOperationException($"Unexpected character '{c}'.");
                    }
                }

                tokens.Add(Token.Simple(TokenType.End, "<end>"));
                return tokens;
            }

            private bool Match(TokenType type)
            {
                if (!Check(type))
                    return false;
                _index++;
                return true;
            }

            private bool MatchOperator(char op)
            {
                if (Peek().Type != TokenType.Operator || Peek().Operator != op)
                    return false;
                _index++;
                return true;
            }

            private void Expect(TokenType type, string message)
            {
                if (!Match(type))
                    throw new InvalidOperationException(message);
            }

            private bool Check(TokenType type)
            {
                return Peek().Type == type;
            }

            private Token Peek()
            {
                return _tokens[_index];
            }

            private Token Previous()
            {
                return _tokens[_index - 1];
            }
        }

        private enum TokenType
        {
            Number,
            Identifier,
            Operator,
            LeftParen,
            RightParen,
            Comma,
            End
        }

        private readonly struct Token
        {
            public readonly TokenType Type;
            public readonly string Text;
            public readonly float Number;
            public readonly char Operator;

            private Token(TokenType type, string text, float number, char op)
            {
                Type = type;
                Text = text;
                Number = number;
                Operator = op;
            }

            public static Token FromNumber(float number, string text) => new(TokenType.Number, text, number, '\0');
            public static Token Identifier(string text) => new(TokenType.Identifier, text, 0f, '\0');
            public static Token FromOperator(char op) => new(TokenType.Operator, op.ToString(), 0f, op);
            public static Token Simple(TokenType type, string text) => new(type, text, 0f, '\0');
        }
    }
}