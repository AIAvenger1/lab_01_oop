using System;
using Microsoft.Maui.Controls;
using lab01.Services; // Простір імен для ExpressionEvaluator
using System.Collections.Generic;

namespace lab01
{
    public partial class MainPage : ContentPage
    {
        public int rows = 10;     // Початкова кількість рядків
        public int columns = 5;   // Початкова кількість стовпців
        public Entry[,] cells;    // Масив для зберігання клітинок

        private bool showExpressions = true; // Режим відображення виразів

        // Для відстеження клітинок, які зараз обчислюються (для запобігання циклічним посиланням)
        private HashSet<(int, int)> evaluatingCells = new HashSet<(int, int)>();

        public MainPage()
        {
            InitializeComponent();
            BuildSpreadsheet();
        }

        private void BuildSpreadsheet()
        {
            SpreadsheetGrid.Children.Clear();
            SpreadsheetGrid.RowDefinitions.Clear();
            SpreadsheetGrid.ColumnDefinitions.Clear();

            cells = new Entry[rows, columns];

            // Додавання додаткової колонки для заголовків рядків
            SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            for (int i = 0; i < columns; i++)
            {
                SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            }

            // Додавання додаткового рядка для заголовків стовпців
            SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            for (int i = 0; i < rows; i++)
            {
                SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            }

            // Додавання заголовків стовпців (A, B, C, ...)
            for (int col = 0; col < columns; col++)
            {
                var label = new Label
                {
                    Text = ColumnNumberToName(col),
                    BackgroundColor = Colors.LightGray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 14
                };
                SpreadsheetGrid.Children.Add(label);
                Grid.SetColumn(label, col + 1);
                Grid.SetRow(label, 0);
            }

            // Додавання заголовків рядків (1, 2, 3, ...)
            for (int row = 0; row < rows; row++)
            {
                var label = new Label
                {
                    Text = (row + 1).ToString(),
                    BackgroundColor = Colors.LightGray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 14
                };
                SpreadsheetGrid.Children.Add(label);
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, row + 1);
            }

            // Заповнення Grid клітинками
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var entry = new Entry
                    {
                        BackgroundColor = Colors.White,
                        TextColor = Colors.Black,
                        Margin = new Thickness(5), // Використання Margin замість Padding
                        FontSize = 14
                    };

                    cells[row, col] = entry;

                    SpreadsheetGrid.Children.Add(entry);
                    Grid.SetColumn(entry, col + 1);
                    Grid.SetRow(entry, row + 1);

                    // Додавання обробника події
                    entry.TextChanged += OnEntryTextChanged;
                }
            }
        }

        private string ColumnNumberToName(int col)
        {
            string columnName = "";
            int dividend = col + 1;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry != null && showExpressions)
            {
                entry.Placeholder = entry.Text; // Зберігаємо вираз у Placeholder
            }
        }

        private void OnToggleModeClicked(object sender, EventArgs e)
        {
            showExpressions = !showExpressions; // Перемикання режиму
            UpdateCellDisplays();               // Оновлення відображення клітинок
        }

        public void UpdateCellDisplays()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var entry = cells[row, col];
                    if (showExpressions)
                    {
                        // Відображення виразу
                        entry.Text = entry.Placeholder;
                        entry.BackgroundColor = Colors.White; // Скидання кольору фону
                    }
                    else
                    {
                        try
                        {
                            // Обчислення виразу
                            var evaluator = new ExpressionEvaluator(this, entry.Placeholder ?? "");
                            double result = evaluator.Evaluate();
                            entry.Text = result.ToString();
                            entry.BackgroundColor = Colors.White; // Скидання кольору фону
                        }
                        catch (Exception ex)
                        {
                            // Відображення детального повідомлення про помилку
                            entry.Text = $"Помилка: {ex.Message}";
                            entry.BackgroundColor = Colors.LightPink; // Виділення клітинки з помилкою
                        }
                    }
                }
            }
        }

        public string GetCellExpression(int row, int col)
        {
            if (row >= 0 && row < rows && col >= 0 && col < columns)
            {
                return cells[row, col].Placeholder ?? "";
            }
            return "";
        }

        public bool IsEvaluating(int row, int col)
        {
            return evaluatingCells.Contains((row, col));
        }

        public void StartEvaluating(int row, int col)
        {
            evaluatingCells.Add((row, col));
        }

        public void StopEvaluating(int row, int col)
        {
            evaluatingCells.Remove((row, col));
        }

        private void OnAddRowClicked(object sender, EventArgs e)
        {
            rows++;
            BuildSpreadsheet();
        }

        private void OnAddColumnClicked(object sender, EventArgs e)
        {
            columns++;
            BuildSpreadsheet();
        }
    }
}
