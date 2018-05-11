using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarParking
{
    /// <summary>
    /// Представляет меню парковки.
    /// </summary>
    public static class Menu
    {
        private static readonly Random rnd = new Random();

        /// <summary>
        /// Отображает главное меню парковки.
        /// </summary>
        public static void Show()
        {
            bool reshowMainMenu = true;

            while (true)
            {
                if (reshowMainMenu)
                    ShowMainMenu();

                reshowMainMenu = true;
                var cki = Console.ReadKey(true);

                switch (Char.ToUpper(cki.KeyChar))
                {
                    case '0':
                        ShowCarsMenu();
                        break;
                    case '1':
                        ShowAddCarMenu();
                        break;
                    case '2':
                        ShowRemoveCarMenu();
                        break;
                    case '3':
                        ShowRechargeCarBalanceMenu();
                        break;
                    case '4':
                        ShowTransactionHistoryForLastMinuteMenu();
                        break;
                    case '5':
                        ShowParkingBalanceMenu();
                        break;
                    case '6':
                        ShowParkingDebitForLastMinute();
                        break;
                    case '7':
                        ShowCountFreeParkingSpaceMenu();
                        break;
                    case '8':
                        ShowCountOccupiedParkingSpaceMenu();
                        break;
                    case '9':
                        ShowTransactionsLogMenu();
                        break;
                    case 'Q':
                        if (ShowAskExitMenu())
                            return;
                        break;
                    default:
                        reshowMainMenu = false;
                        break;
                }
            }
        }

        #region Menu and submenu

        private static void ShowMainMenu()
        {
            ClearConsole();
            ShowTitle("Menu:");
            Console.WriteLine("0 - Просмотреть список машин на парковке");
            Console.WriteLine("1 - Добавить машину на парковку");
            Console.WriteLine("2 - Убрать машину с парковки");
            Console.WriteLine("3 - Пополнить баланс машины");
            Console.WriteLine("4 - Вывести истории транзакций за последнюю минуту");
            Console.WriteLine("5 - Вывести общий доход парковки");
            Console.WriteLine("6 - Вывести доход парковки за последнюю минуту");
            Console.WriteLine("7 - Вывести количество свободных мест на парковке");
            Console.WriteLine("8 - Вывести количество занятых мест на парковке");
            Console.WriteLine("9 - Вывести Transactions.log");
            ShowError("Q - Выйти");
        }

        private static void ShowCarsMenu()
        {
            ClearConsole();
            ShowTitle("Список машин:");

            if (Parking.Instance.CountOccupiedParkingSpace == 0)
            {
                ShowError("парковка пуста.");
                WaitForPressAnyKey();
            }
            else
            {
                Console.WriteLine(Parking.Instance.GetCarsInfo());
                WaitForPressKeyEnter();
            }
        }

        private static void ShowAddCarMenu()
        {
            ClearConsole();
            ShowTitle("[Добавить машину на парковку]");

            if (Parking.Instance.CountFreeParkingSpace == 0)
            {
                ShowError("Невозможно добавить машину. Отсутствуют свободные места.");
                WaitForPressAnyKey();
                return;
            }

            CarType carType = (CarType)rnd.Next(1, 5);
            decimal initBalance = rnd.Next(100, 1001);

            Car newCar = new Car(carType, initBalance);
            Parking.Instance.AddCar(newCar);

            ShowInfo("Машина успешно добавлена на парковку:\r\n" + newCar.ToString());
            WaitForPressAnyKey();
        }

        private static void ShowRemoveCarMenu()
        {
            ClearConsole();
            ShowTitle("[Удалить машину с парковки]");

            if (Parking.Instance.CountOccupiedParkingSpace == 0)
            {
                ShowError("На парковке отсутствуют машины.");
                WaitForPressAnyKey();
                return;
            }

            Console.WriteLine("Список существующих машин:");
            Console.WriteLine(Parking.Instance.GetCarsInfo());

            do
            {
                Console.Write("Введите ID машины: ");

                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    Car car = Parking.Instance.GetCar(id);

                    if (car != null && Parking.Instance.RemoveCar(id))
                    {
                        ShowInfo("Машина успешно удалена с парковки.");
                        WaitForPressAnyKey();
                        return;
                    }

                    if (car == null)
                        ShowError("По указанному ID не удалось найти машину.");
                    else if (car.Balance < 0)
                        ShowError("Чтобы забрать машину, сначала нужно сплатить штраф.");
                    else
                        ShowError("Машину не удалось удалить с парковки.");
                }
                else
                {
                    ShowError("Не удается преобразовать входную строку в число.");
                }

                Console.WriteLine("Для выхода нажмите Q, для повтора любую другую клавишу...");
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
        }

        private static void ShowRechargeCarBalanceMenu()
        {
            ClearConsole();
            ShowTitle("[Пополнить баланс машины]");

            if (Parking.Instance.CountOccupiedParkingSpace == 0)
            {
                ShowError("На парковке отсутствуют машины.");
                WaitForPressAnyKey();
                return;
            }

            //Пополнить баланс машины
            Console.WriteLine("Список существующих машин:");
            Console.WriteLine(Parking.Instance.GetCarsInfo());

            do
            {
                Console.Write("Введите ID машины: ");

                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    Car car = Parking.Instance.GetCar(id);

                    if (car == null)
                    {
                        ShowError("По указанному ID не удалось найти машину.");
                    }
                    else
                    {
                        ShowInfo(car.ToString());
                        Console.Write("Введите сумму пополнения (от 50 до 1000): ");

                        if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount >= 50 && amount <= 1000)
                        {
                            if (Parking.Instance.RechargeCarBalance(id, amount))
                            {
                                ShowInfo("Баланс машины успешно пополнен!");
                                WaitForPressAnyKey();
                                return;
                            }
                            else
                            {
                                ShowError("Баланс машины пополнить не удалось.");
                            }
                        }
                        else
                        {
                            ShowError("Неверное значение суммы.");
                        }
                    }
                }
                else
                {
                    ShowError("Не удается преобразовать входную строку в число.");
                }

                Console.WriteLine("Для выхода нажмите Q, для повтора любую другую клавишу...");
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
        }

        private static void ShowTransactionHistoryForLastMinuteMenu()
        {
            ClearConsole();

            ShowTitle("История транзакций за последнюю минуту:");
            string history = Parking.Instance.GetTransactionHistoryForLastMinute();

            if (history == String.Empty)
            {
                ShowError("История транзакций отсутствует.");
                WaitForPressAnyKey();
            }
            else
            {
                Console.WriteLine(history);
                WaitForPressKeyEnter();
            }
        }

        private static void ShowParkingBalanceMenu()
        {
            ClearConsole();
            ShowTitle("Общий доход парковки составляет: " + Parking.Instance.Balance.ToString());

            WaitForPressAnyKey();
        }

        private static void ShowParkingDebitForLastMinute()
        {
            ClearConsole();
            ShowTitle("Доход парковки за последнюю минуту составляет: " + Parking.Instance.GetDebitedForLastMinute().ToString());

            WaitForPressAnyKey();
        }

        private static void ShowCountFreeParkingSpaceMenu()
        {
            ClearConsole();
            ShowTitle($"Количество свободных мест на парковке: {Parking.Instance.CountFreeParkingSpace.ToString()}/{Settings.ParkingSpace.ToString()}.");

            WaitForPressAnyKey();
        }

        private static void ShowCountOccupiedParkingSpaceMenu()
        {
            ClearConsole();
            ShowTitle($"Количество занятых мест на парковке: {Parking.Instance.CountOccupiedParkingSpace.ToString()}/{Settings.ParkingSpace.ToString()}.");

            WaitForPressAnyKey();
        }

        private static void ShowTransactionsLogMenu()
        {
            ClearConsole();

            ShowTitle("Содержание файла транзакций:");
            string log = Parking.Instance.GetTransactionLog();

            if (log == String.Empty)
            {
                ShowError("Файл пуст или его не удалось прочесть.");
                WaitForPressAnyKey();
            }
            else
            {
                Console.WriteLine(log);
                WaitForPressKeyEnter();
            }
        }

        /// <summary>
        /// Запрашивает у пользователя подтверждение выхода. 
        /// Возвращает true, если выход подтвержден, инача возвращает false. 
        /// </summary>
        /// <returns></returns>
        private static bool ShowAskExitMenu()
        {
            ClearConsole();
            Console.WriteLine("Подтвердите выход: y/n");

            return Console.ReadKey().Key == ConsoleKey.Y;
        }

        #endregion

        #region Helper methods

        private static void ClearConsole()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }

        private static void WaitForPressAnyKey()
        {
            Console.WriteLine();
            Console.WriteLine("Для возврата в главное меню нажмите любую клавишу...");
            Console.ReadKey(true);
        }

        private static void WaitForPressKeyEnter()
        {
            Console.WriteLine();
            Console.WriteLine("Для возврата в главное меню нажмите клавишу Enter...");

            do { } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
        }

        private static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ShowInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ShowTitle(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        #endregion
    }
}
