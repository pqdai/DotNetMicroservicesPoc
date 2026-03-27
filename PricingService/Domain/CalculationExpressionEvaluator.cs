using System;
using System.Collections.Generic;
using System.Globalization;

namespace PricingService.Domain;

internal static class CalculationExpressionEvaluator
{
    public static bool EvaluateBoolean(string expression, Calculation calculation)
    {
        var value = new ExpressionParser(calculation).Evaluate(expression);
        return value switch
        {
            bool boolean => boolean,
            _ => throw new InvalidOperationException($"Cannot convert '{value}' to bool.")
        };
    }

    public static decimal EvaluateDecimal(string expression, Calculation calculation)
    {
        var value = new ExpressionParser(calculation).Evaluate(expression);
        return value switch
        {
            decimal decimalValue => decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            _ => decimal.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture)
        };
    }

    private sealed class ExpressionParser
    {
        private readonly Calculation calculation;
        private string expression = string.Empty;
        private int position;

        public ExpressionParser(Calculation calculation)
        {
            this.calculation = calculation;
        }

        public object Evaluate(string source)
        {
            expression = source;
            position = 0;

            var value = ParseConditional();
            SkipWhitespace();

            if (position != expression.Length)
            {
                throw new InvalidOperationException($"Unexpected token at position {position} in '{expression}'.");
            }

            return value;
        }

        private object ParseConditional()
        {
            var condition = ParseComparison();
            SkipWhitespace();

            if (!Match("?"))
            {
                return condition;
            }

            var whenTrue = ParseConditional();
            Expect(":");
            var whenFalse = ParseConditional();
            return EvaluateBooleanValue(condition) ? whenTrue : whenFalse;
        }

        private object ParseComparison()
        {
            var left = ParseAdditive();

            while (true)
            {
                SkipWhitespace();

                if (Match("=="))
                {
                    left = Equals(left, ParseAdditive());
                }
                else if (Match("!="))
                {
                    left = !Equals(left, ParseAdditive());
                }
                else if (Match(">="))
                {
                    left = EvaluateDecimalValue(left) >= EvaluateDecimalValue(ParseAdditive());
                }
                else if (Match("<="))
                {
                    left = EvaluateDecimalValue(left) <= EvaluateDecimalValue(ParseAdditive());
                }
                else if (Match(">"))
                {
                    left = EvaluateDecimalValue(left) > EvaluateDecimalValue(ParseAdditive());
                }
                else if (Match("<"))
                {
                    left = EvaluateDecimalValue(left) < EvaluateDecimalValue(ParseAdditive());
                }
                else
                {
                    return left;
                }
            }
        }

        private object ParseAdditive()
        {
            var left = ParseMultiplicative();

            while (true)
            {
                SkipWhitespace();

                if (Match("+"))
                {
                    left = EvaluateDecimalValue(left) + EvaluateDecimalValue(ParseMultiplicative());
                }
                else if (Match("-"))
                {
                    left = EvaluateDecimalValue(left) - EvaluateDecimalValue(ParseMultiplicative());
                }
                else
                {
                    return left;
                }
            }
        }

        private object ParseMultiplicative()
        {
            var left = ParseUnary();

            while (true)
            {
                SkipWhitespace();

                if (Match("*"))
                {
                    left = EvaluateDecimalValue(left) * EvaluateDecimalValue(ParseUnary());
                }
                else if (Match("/"))
                {
                    left = EvaluateDecimalValue(left) / EvaluateDecimalValue(ParseUnary());
                }
                else
                {
                    return left;
                }
            }
        }

        private object ParseUnary()
        {
            SkipWhitespace();

            if (Match("-"))
            {
                return -EvaluateDecimalValue(ParseUnary());
            }

            return ParsePrimary();
        }

        private object ParsePrimary()
        {
            SkipWhitespace();

            if (Match("("))
            {
                var nested = ParseConditional();
                Expect(")");
                return nested;
            }

            if (Peek() == '"')
            {
                return ParseString();
            }

            if (char.IsDigit(Peek()))
            {
                return ParseNumber();
            }

            return ResolveIdentifier(ParseIdentifier());
        }

        private object ResolveIdentifier(string identifier)
        {
            if (string.Equals(identifier, "policyFrom", StringComparison.OrdinalIgnoreCase))
            {
                return calculation.PolicyFrom;
            }

            if (string.Equals(identifier, "policyTo", StringComparison.OrdinalIgnoreCase))
            {
                return calculation.PolicyTo;
            }

            if (calculation.Covers.TryGetValue(identifier, out var cover))
            {
                return cover;
            }

            if (calculation.Subject.TryGetValue(identifier, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Calculation value '{identifier}' was not provided.");
        }

        private string ParseIdentifier()
        {
            SkipWhitespace();
            var start = position;

            while (position < expression.Length &&
                   (char.IsLetterOrDigit(expression[position]) || expression[position] == '_'))
            {
                position++;
            }

            if (start == position)
            {
                throw new InvalidOperationException($"Expected identifier at position {position} in '{expression}'.");
            }

            return expression[start..position];
        }

        private string ParseString()
        {
            Expect("\"");
            var start = position;

            while (position < expression.Length && expression[position] != '"')
            {
                position++;
            }

            var value = expression[start..position];
            Expect("\"");
            return value;
        }

        private decimal ParseNumber()
        {
            var start = position;

            while (position < expression.Length &&
                   (char.IsDigit(expression[position]) || expression[position] == '.'))
            {
                position++;
            }

            if (position < expression.Length && (expression[position] == 'M' || expression[position] == 'm'))
            {
                position++;
            }

            return decimal.Parse(expression[start..position].TrimEnd('M', 'm'), CultureInfo.InvariantCulture);
        }

        private void SkipWhitespace()
        {
            while (position < expression.Length && char.IsWhiteSpace(expression[position]))
            {
                position++;
            }
        }

        private char Peek()
        {
            return position < expression.Length ? expression[position] : '\0';
        }

        private bool Match(string token)
        {
            SkipWhitespace();

            if (!expression.AsSpan(position).StartsWith(token, StringComparison.Ordinal))
            {
                return false;
            }

            position += token.Length;
            return true;
        }

        private void Expect(string token)
        {
            if (!Match(token))
            {
                throw new InvalidOperationException($"Expected '{token}' at position {position} in '{expression}'.");
            }
        }

        private static bool EvaluateBooleanValue(object value)
        {
            return value switch
            {
                bool boolean => boolean,
                _ => throw new InvalidOperationException($"Cannot convert '{value}' to bool.")
            };
        }

        private static decimal EvaluateDecimalValue(object value)
        {
            return value switch
            {
                decimal decimalValue => decimalValue,
                int intValue => intValue,
                long longValue => longValue,
                _ => decimal.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture)
            };
        }
    }
}
