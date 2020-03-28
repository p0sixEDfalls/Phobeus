using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;

namespace Phobeus.ViewModels
{
    public enum CurrentTab
    {
        Calculator,
        Converter,
        About
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private CurrentTab currentTab;

        private List<string> currencies = new List<string>() { "RUB", "USD", "EUR" };

        private Dictionary<string, double> converterValues = new Dictionary<string, double>()
        {
            ["USDRUB"] = 63.30,
            ["USDEUR"] = 0.92,
            ["USDUSD"] = 1.0,      

            ["RUBUSD"] = 0.016,
            ["RUBEUR"] = 0.015,
            ["RUBRUB"] = 1.0,

            ["EURUSD"] = 1.08,
            ["EURRUB"] = 68.62,
            ["EUREUR"] = 1.0,
        };

        private readonly NumberFormatInfo negativeFormat = new NumberFormatInfo() { NegativeSign = "-" };

        private string currentFromCurrency;

        private string currentToCurrency;

        private string currencyInputValue;

        private string currencyOutputValue;

        public List<string> Currencies
        {
            get => currencies;
            set => this.RaiseAndSetIfChanged(ref currencies, value);
        }

        public string CurrentFromCurrency
        {
            get => currentFromCurrency;
            set => this.RaiseAndSetIfChanged(ref currentFromCurrency, value);
        }

        public string CurrentToCurrency
        {
            get => currentToCurrency;
            set => this.RaiseAndSetIfChanged(ref currentToCurrency, value);
        }

        public string CurrencyInputValue
        {
            get => currencyInputValue;
            set => this.RaiseAndSetIfChanged(ref currencyInputValue, value);
        }

        public string CurrencyOutputValue
        {
            get => currencyOutputValue;
            set => this.RaiseAndSetIfChanged(ref currencyOutputValue, value);
        }

        private readonly double maxValue = 9999999999999.0;

        private bool isNeedToCalculate;

        private bool isNeedToUseExistOutValue;

        private string action;

        private string outputValue;

        private readonly List<Key> acceptedKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9 };

        private readonly List<Key> commandKeys = new List<Key>() { Key.Back, Key.OemPeriod, Key.Add, Key.Subtract, Key.Multiply, Key.Divide, Key.OemPlus };

        private string inputText;

        private string outputText;

        public string InputText
        {
            get => inputText;
            set => this.RaiseAndSetIfChanged(ref inputText, value);
        }

        public string OutputText
        {
            get => outputText;
            set => this.RaiseAndSetIfChanged(ref outputText, value);
        }

        public ReactiveCommand<string, Unit> AddInputDigitCommand { get; }

        public ReactiveCommand<string, Unit> ExecuteDigitCommand { get; }

        public MainWindowViewModel()
        {
            inputText = "";
            outputText = "";

            AddInputDigitCommand = ReactiveCommand.Create<string>(AddInputDigit);
            ExecuteDigitCommand = ReactiveCommand.Create<string>(ExecuteCommand);

            CurrencyInputValue = "0";
            CurrencyOutputValue = "0";

            currentFromCurrency = currencies[0];
            currentToCurrency = currencies[1];
        }

        private void ExecuteCommand(string obj)
        {
            if (obj == "=")
            {
                isNeedToUseExistOutValue = true;
                Calculate();
            }
            else if (obj == "d")
            {
                DeleTeLastInputChar();
            }
            else if (obj == ".")
            {
                AddComma();
            }
            else if (obj == "C")
            {
                Clear();
            }
            else if (obj == "+/-")
            {
                ChangeSign();
                return;
            }
            else
            {
                if (InputText == "0")
                {
                    action = obj;
                    OutputText = $"{outputValue} {action}";
                }
                else
                {
                    if (isNeedToCalculate)
                    {
                        Calculate();
                        action = obj;
                        outputValue = OutputText;
                        OutputText += $" {obj}";
                        isNeedToCalculate = true;
                    }
                    else
                    {
                        isNeedToCalculate = true;
                        action = obj;

                        if (isNeedToUseExistOutValue)
                        {
                            outputValue = OutputText;
                            OutputText += $" {action}";
                        }
                        else
                        {
                            outputValue = InputText;
                            OutputText = $"{InputText} {action}";
                        }
                    }

                    isNeedToUseExistOutValue = false;
                }
            }

            //InputText = "0";
        }

        private void AddInputDigit(string obj)
        {
            if (currentTab == CurrentTab.Calculator)
            {
                if (InputText == "0")
                {
                    InputText = obj;
                }
                else
                {
                    string[] levels = InputText.Split('.');

                    if (levels.Length == 1)
                    {
                        if (levels[0].Length < 6)
                            InputText += obj;
                    }
                    else
                    {
                        if (levels[1].Length < 6)
                            InputText += obj;
                    }
                }
            }
            else if (currentTab == CurrentTab.Converter)
            {
                if (CurrencyInputValue == "0" )
                {
                    CurrencyInputValue = obj;
                }
                else
                {
                    string[] levels = CurrencyInputValue.Split('.');

                    if (levels.Length == 1)
                    {
                        if (levels[0].Length < 6)
                            CurrencyInputValue += obj;
                    }
                    else
                    {
                        if (levels[1].Length < 3)
                            CurrencyInputValue += obj;
                    }
                }

                Convert();
            }                        
        }

        private void Calculate()
        {
            isNeedToCalculate = false;

            double input = double.Parse(InputText, negativeFormat);
            double output = double.Parse(outputValue, negativeFormat);
            double result = 0.0;

            switch (action)
            {
                case "+":
                    result = input + output;
                    break;

                case "-":
                    result = output - input;
                    break;

                case "x":
                    result = output * input;
                    break;

                case "/":
                    result = output / input;
                    break;
            }

            if (double.IsInfinity(result))
            {
                Clear();
                OutputText = "Devide by zero";
                return;
            }           

            if (result > maxValue)
            {
                Clear();
                OutputText = "Out of range";
            }
            else
            {
                OutputText = result.ToString("0.######");
            }
        }

        private void Convert()
        {
            CurrencyOutputValue = (double.Parse(CurrencyInputValue) * converterValues[CurrentToCurrency + CurrentFromCurrency]).ToString("0.######");
        }

        private void DeleTeLastInputChar()
        {
            if (InputText.Length > 0)
                InputText = InputText.Remove(InputText.Length - 1);

            if (string.IsNullOrEmpty(InputText))
                InputText = "0";
        }

        private void AddComma()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                InputText = "0.";
            }
            else
            {
                if (!InputText.Contains('.'))
                    InputText += ".";
            }
        }

        private void ChangeSign()
        {
            if (InputText.Contains("-"))
                InputText = InputText.Remove(0, 1);
            else
                InputText = $"-{InputText}";
        }

        private void Clear()
        {
            InputText = "0";
            OutputText = string.Empty;
            action = string.Empty;
            isNeedToCalculate = false;
            isNeedToUseExistOutValue = false;
        }

        public void ProcessKeyInput(object obj, KeyEventArgs args)
        {
            if (currentTab == CurrentTab.Calculator)
            {
                if (acceptedKeys.Contains(args.Key))
                    AddInputDigit(args.Key.ToString().Remove(0, 1));

                if (commandKeys.Contains(args.Key))
                {
                    switch (args.Key)
                    {
                        case Key.Back:
                            DeleTeLastInputChar();
                            break;

                        case Key.OemPeriod:
                            AddComma();
                            break;

                        case Key.Add:
                            ExecuteCommand("+");
                            break;

                        case Key.Subtract:
                            ExecuteCommand("-");
                            break;

                        case Key.Multiply:
                            ExecuteCommand("x");
                            break;

                        case Key.Divide:
                            ExecuteCommand("/");
                            break;

                        case Key.OemPlus:
                            ExecuteCommand("=");
                            break;
                    }
                }
            }
            else if (currentTab == CurrentTab.Converter)
            {
                if (acceptedKeys.Contains(args.Key))
                    AddInputDigit(args.Key.ToString().Remove(0, 1));
            }          
        }


        public void ProcessKeyInputCurrency(object obj, SelectionChangedEventArgs args)
        {
            switch ((args.AddedItems[0] as TabItem).Header)
            {
                case "Calculator":
                    currentTab = CurrentTab.Calculator;
                    break;

                case "Converter":
                    currentTab = CurrentTab.Converter;
                    break;

                case "About":
                    currentTab = CurrentTab.About;
                    break;
            }
        }
    }
}
