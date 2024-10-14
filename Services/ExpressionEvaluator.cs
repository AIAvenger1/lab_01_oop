using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace lab01.Services
{
    public class ExpressionEvaluator
    {
        private readonly MainPage mainPage;
        private readonly string expression;
        private int position;

        public ExpressionEvaluator(MainPage mainPage, string expression)
        {
            this.mainPage = mainPage;
            this.expression = expression.Replace(" ", ""); // Видалення всіх пробілів
            this.position = 0;
        }

        public double Evaluate()
        {
            double value = ParseExpression();
            if (position < expression.Length)
                throw new Exception("Несподівані символи в кінці виразу.");
            return value;
        }

        private double ParseExpression()
        {
            double value = ParseTerm();
            while (position < expression.Length)
            {
                if (expression[position] == '+')
                {
                    position++;
                    value += ParseTerm();
                }
                else if (expression[position] == '-')
                {
                    position++;
                    value -= ParseTerm();
                }
                else
                {
                    break;
                }
            }
            return value;
        }

        private double ParseTerm()
        {
            double value = ParseFactor();
            while (position < expression.Length)
            {
                if (expression[position] == '*')
                {
                    position++;
                    value *= ParseFactor();
                }
                else if (expression[position] == '/')
                {
                    position++;
                    double denominator = ParseFactor();
                    if (denominator == 0)
                        throw new Exception("Ділення на нуль.");
                    value /= denominator;
                }
                else
                {
                    break;
                }
            }
            return value;
        }

        private double ParseFactor()
        {
            if (position >= expression.Length)
                throw new Exception("Несподіваний кінець виразу.");

            if (expression[position] == '+')
            {
                position++;
                return ParseFactor();
            }
            else if (expression[position] == '-')
            {
                position++;
                return -ParseFactor();
            }
            else if (expression[position] == '(')
            {
                position++;
                double value = ParseExpression();
                if (position >= expression.Length || expression[position] != ')')
                    throw new Exception("Очікується ')'.");
                position++;
                return value;
            }
            else if (char.IsDigit(expression[position]) || expression[position] == '.')
            {
                return ParseNumber();
            }
            else if (char.IsLetter(expression[position]))
            {
                string identifier = ParseIdentifier();
                if (IsFunction(identifier))
                {
                    return ParseFunction(identifier);
                }
                else
                {
                    return ParseCellReference(identifier);
                }
            }
            else
            {
                throw new Exception($"Несподіваний символ '{expression[position]}' у виразі.");
            }
        }

        private string ParseIdentifier()
        {
            int start = position;
            while (position < expression.Length && char.IsLetter(expression[position]))
            {
                position++;
            }
            return expression.Substring(start, position - start).ToLower();
        }

        private bool IsFunction(string identifier)
        {
            return identifier == "div" || identifier == "mod" || identifier == "mmax" || identifier == "mmin";
        }

        private double ParseFunction(string functionName)
        {
            switch (functionName)
            {
                case "div":
                    return ParseDivFunction();
                case "mod":
                    return ParseModFunction();
                case "mmax":
                    return ParseMMaxFunction();
                case "mmin":
                    return ParseMMinFunction();
                default:
                    throw new Exception($"Невідома функція '{functionName}'.");
            }
        }

        private double ParseDivFunction()
        {
            if (position >= expression.Length || expression[position] != '(')
                throw new Exception("Очікується '(' після 'div'.");

            position++; // Пропуск '('
            List<double> args = ParseFunctionArguments();

            if (args.Count < 2)
                throw new Exception("Функція 'div' вимагає принаймні два аргументи.");

            double result = args[0];
            for (int i = 1; i < args.Count; i++)
            {
                if (args[i] == 0)
                    throw new Exception("Ділення на нуль у 'div'.");
                result /= args[i];
            }

            return result;
        }

        private double ParseModFunction()
        {
            if (position >= expression.Length || expression[position] != '(')
                throw new Exception("Очікується '(' після 'mod'.");

            position++; // Пропуск '('
            List<double> args = ParseFunctionArguments();

            if (args.Count != 2)
                throw new Exception("Функція 'mod' вимагає рівно два аргументи.");

            double a = args[0];
            double b = args[1];

            if (b == 0)
                throw new Exception("Модуль ділення на нуль у 'mod'.");

            return a % b;
        }

        private double ParseMMaxFunction()
        {
            if (position >= expression.Length || expression[position] != '(')
                throw new Exception("Очікується '(' після 'mmax'.");

            position++; // Пропуск '('
            List<double> args = ParseFunctionArguments();

            if (args.Count == 0)
                throw new Exception("Функція 'mmax' вимагає хоча б один аргумент.");

            double max = args[0];
            foreach (var arg in args)
            {
                if (arg > max)
                    max = arg;
            }

            return max;
        }

        private double ParseMMinFunction()
        {
            if (position >= expression.Length || expression[position] != '(')
                throw new Exception("Очікується '(' після 'mmin'.");

            position++; // Пропуск '('
            List<double> args = ParseFunctionArguments();

            if (args.Count == 0)
                throw new Exception("Функція 'mmin' вимагає хоча б один аргумент.");

            double min = args[0];
            foreach (var arg in args)
            {
                if (arg < min)
                    min = arg;
            }

            return min;
        }

        private List<double> ParseFunctionArguments()
        {
            List<double> args = new List<double>();

            while (position < expression.Length && expression[position] != ')')
            {
                args.Add(ParseExpression());

                if (position < expression.Length && expression[position] == ',')
                {
                    position++; // Пропуск ','
                }
                else
                {
                    break;
                }
            }

            if (position >= expression.Length || expression[position] != ')')
                throw new Exception("Очікується ')' після аргументів функції.");

            position++; // Пропуск ')'

            return args;
        }

        private double ParseNumber()
        {
            int start = position;
            bool hasDecimalPoint = false;

            while (position < expression.Length && (char.IsDigit(expression[position]) || expression[position] == '.'))
            {
                if (expression[position] == '.')
                {
                    if (hasDecimalPoint)
                        throw new Exception("Недопустиме використання десяткового роздільника.");
                    hasDecimalPoint = true;
                }
                position++;
            }

            string numberStr = expression.Substring(start, position - start);
            if (!double.TryParse(numberStr, out double number))
                throw new Exception($"Невірне число '{numberStr}'.");

            return number;
        }

        private double ParseCellReference(string identifier)
        {
            // Зчитування цифр після літер
            int start = position;
            while (position < expression.Length && char.IsDigit(expression[position]))
            {
                position++;
            }
            string numberStr = expression.Substring(start, position - start);
            if (string.IsNullOrEmpty(numberStr))
                throw new Exception($"Невірний формат посилання на клітинку після '{identifier}'.");

            int rowNumber = int.Parse(numberStr);
            string columnLetters = identifier.ToUpper();

            int col = ColumnNameToNumber(columnLetters);
            int row = rowNumber - 1; // Індексування від 0

            if (col < 0 || col >= mainPage.columns || row < 0 || row >= mainPage.rows)
                throw new Exception($"Посилання на клітинку '{columnLetters}{rowNumber}' виходить за межі таблиці.");

            string cellExpression = mainPage.GetCellExpression(row, col);
            if (string.IsNullOrWhiteSpace(cellExpression))
                return 0; // Порожні клітинки розглядаються як нуль

            // Запобігання циклічним посиланням
            if (mainPage.IsEvaluating(row, col))
                throw new Exception($"Виявлено циклічне посилання у клітинці '{columnLetters}{rowNumber}'.");

            mainPage.StartEvaluating(row, col);
            var evaluator = new ExpressionEvaluator(mainPage, cellExpression);
            double value = evaluator.Evaluate();
            mainPage.StopEvaluating(row, col);

            return value;
        }

        private int ColumnNameToNumber(string columnName)
        {
            int sum = 0;
            foreach (char c in columnName)
            {
                if (c < 'A' || c > 'Z')
                    throw new Exception($"Невірний символ '{c}' у назві стовпця.");

                sum *= 26;
                sum += (c - 'A' + 1);
            }
            return sum - 1; // Індексування від 0
        }
    }
}
