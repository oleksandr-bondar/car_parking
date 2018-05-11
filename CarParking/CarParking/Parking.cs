using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;

namespace CarParking
{
    /// <summary>
    /// Представляет автомобильную парковку.
    /// </summary>
    public sealed class Parking
    {
        #region Singleton

        private static readonly Lazy<Parking> _instance = new Lazy<Parking>(() => new Parking());

        /// <summary>
        /// Представляет единственный экземпляр класса <see cref="Parking"/>.
        /// </summary>
        public static Parking Instance => _instance.Value;

        #endregion

        #region Fields

        // Использ. для блокировки кода в конструкции lock {...}
        private readonly object LockObj = new object();
        // Содержит название файла
        private readonly string TransactionsLogFileName = "Transactions.log";
        // Содержит локальную константу интервала времени в 1у минуту
        private readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);

        // Содержит список машин на парковке
        private readonly List<Car> _cars;
        // Содержит список транзакций за определенный промежуток времени
        private readonly List<Transaction> _transactions;
        // Использ. для списывания средств за парковку
        private readonly Timer _timerParkingPayment;
        // Использ. для обновления файла "Transactions.log"
        private readonly Timer _timerTransactionsLog;
        // Общее кол-во заработанных средств
        private decimal _balance;

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает общее количество заработанных средств парковкой.
        /// </summary>
        public decimal Balance
        {
            get => _balance;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Баланс не может быть меньшим нуля.");

                _balance = value;
            }
        }
        /// <summary>
        /// Возвращает количество свободных мест на парковке.
        /// </summary>
        public int CountFreeParkingSpace => Settings.ParkingSpace - _cars.Count;
        /// <summary>
        /// Возвращает количество занятых мест на парковке.
        /// </summary>
        public int CountOccupiedParkingSpace => _cars.Count;

        #endregion

        #region Constructors

        private Parking()
        {
            _cars = new List<Car>(Settings.ParkingSpace);
            _transactions = new List<Transaction>();

            _timerParkingPayment = new Timer(Settings.Timeout * 1000);
            _timerParkingPayment.AutoReset = true;
            _timerParkingPayment.Elapsed += TimerParkingPayment_Elapsed;
            //_timerParkingPayment.Start();

            _timerTransactionsLog = new Timer(60 * 1000);
            _timerTransactionsLog.AutoReset = true;
            _timerTransactionsLog.Elapsed += TimerTransactionsLog_Elapsed;
            _timerTransactionsLog.Start();
        }

        #endregion

        #region Private Methods

        private void TimerParkingPayment_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (LockObj)
            {
                foreach (Car car in _cars)
                {
                    decimal parkingPrice = Settings.GetParkingPrice(car.Type);

                    if (parkingPrice <= car.Balance)
                    {
                        car.Balance -= parkingPrice;
                        Balance += parkingPrice;
                        _transactions.Add(new Transaction(DateTime.Now, car.Id, parkingPrice));
                    }
                    else
                    {
                        // Списываем штраф за парковку
                        car.Balance -= parkingPrice * Settings.Fine;
                    }
                }
            }
        }

        private void TimerTransactionsLog_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (LockObj)
            {
                decimal debited = GetDebitedForLastMinute();

                try
                {
                    File.AppendAllText(TransactionsLogFileName,
                        $"[{DateTime.Now.ToString()}]: {debited.ToString()}\r\n",
                        Encoding.UTF8);
                }
                catch (DirectoryNotFoundException)
                { }
                catch (IOException)// не удалось записать в файл
                {}
                catch (UnauthorizedAccessException)// отсутствует необходимое разрешение для записи в файл
                { }

                // Удаляем все транзакции, которым больше 2х минут
                ClearOldTransactions();
            }
        }

        private void ClearOldTransactions()
        {
            TimeSpan TwoMinutes = TimeSpan.FromMinutes(2);
            DateTime dtNow = DateTime.Now;
            int count = 0;

            foreach (var item in _transactions)
            {
                if (dtNow - item.DateTime > TwoMinutes)
                    count++;
                else
                    break;
            }

            if (count > 0)
                _transactions.RemoveRange(0, count);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Добавляет указанную машину на парковку.
        /// </summary>
        /// <param name="car">Машина.</param>
        /// <exception cref="ArgumentNullException">Значение car равно null.</exception>
        /// <exception cref="InvalidOperationException">На парковке отсутствуют свободные места.</exception>
        public void AddCar(Car car)
        {
            lock (LockObj)
            {
                if (car == null)
                    throw new ArgumentNullException(nameof(car));

                if (CountFreeParkingSpace == 0)
                    throw new InvalidOperationException("На парковке отсутствуют свободные места.");

                _cars.Add(car);

                if (_cars.Count == 1)
                    _timerParkingPayment.Start();
            }
        }

        /// <summary>
        /// Удаляет машину с парковки по указанному ID.
        /// В случае успеха возвращает true, иначе false.
        /// </summary>
        /// <param name="id">Идентификатор машины.</param>
        /// <returns></returns>
        public bool RemoveCar(int id)
        {
            lock (LockObj)
            {
                Car car = GetCar(id);

                if (car == null)
                    return false;

                int index = _cars.IndexOf(car);

                if (index >= 0)
                {
                    if (car.Balance < 0)
                        return false;

                    _cars.RemoveAt(index);

                    if (_cars.Count == 0)
                        _timerParkingPayment.Stop();

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Пополняет баланс машины по указанному ID.
        /// В случае успеха возвращает true, иначе false.
        /// </summary>
        /// <param name="id">Идентификатор машины.</param>
        /// <param name="amount">Сумма пополнения.</param>
        /// <exception cref="ArgumentOutOfRangeException">Значение amount меньше или равно нулю.</exception>
        /// <returns></returns>
        public bool RechargeCarBalance(int id, decimal amount)
        {
            lock (LockObj)
            {
                if (amount <= 0)
                    throw new ArgumentOutOfRangeException(nameof(amount), "Значение суммы пополнения должно быть больше нуля.");

                Car car = GetCar(id);

                if (car != null)
                    car.Balance += amount;

                return car != null;
            }
        }

        /// <summary>
        /// Возвращает машину по указанному ID или null в противном случае.
        /// </summary>
        /// <param name="id">Идентификатор машины.</param>
        /// <returns></returns>
        public Car GetCar(int id) => _cars.Find(c => c.Id == id);

        /// <summary>
        /// Возвращает историю транзакций за последнюю минуту.
        /// </summary>
        /// <returns></returns>
        public string GetTransactionHistoryForLastMinute()
        {
            lock (LockObj)
            {
                DateTime dtNow = DateTime.Now;
                StringBuilder sb = new StringBuilder();
                string separator = new string('-', 50);

                sb.AppendLine(separator);
                sb.AppendLine($"|{"Время",-20}|{"ID машины",-10}|{"Списано средств",-16}|");
                sb.AppendLine(separator);

                int saveLength = sb.Length;

                foreach (var item in Enumerable.Reverse(_transactions))
                {
                    if (dtNow - item.DateTime > OneMinute)
                        break;

                    sb.AppendLine($"|{item.DateTime.ToString(),-20}|{item.CarId.ToString(),-10}|{Decimal.Round(item.Debited, 10).ToString(),-16}|");
                    sb.AppendLine(separator);
                }

                if (saveLength != sb.Length)
                    return sb.ToString();

                return String.Empty;
            }
        }

        /// <summary>
        /// Возвращает количество заработанных средств за последнюю минуту.
        /// </summary>
        /// <returns></returns>
        public decimal GetDebitedForLastMinute()
        {
            lock (LockObj)
            {
                DateTime dtNow = DateTime.Now;
                decimal debited = Decimal.Zero;

                foreach (var item in Enumerable.Reverse(_transactions))
                {
                    if (dtNow - item.DateTime > OneMinute)
                        break;

                    debited += item.Debited;
                }

                return debited;
            }
        }

        /// <summary>
        /// Возвращает содержание файла Transactions.log.
        /// </summary>
        /// <returns></returns>
        public string GetTransactionLog()
        {
            lock (LockObj)
            {
                try
                {
                    return File.ReadAllText(TransactionsLogFileName, Encoding.UTF8);
                }
                catch (DirectoryNotFoundException)
                {
                    return String.Empty;
                }
                catch (FileNotFoundException)
                {
                    return String.Empty;
                }
                catch (IOException)// не удалось прочитать файл
                {
                    return String.Empty;
                }
                catch (UnauthorizedAccessException)// отсутствует необходимое разрешение для чтения файла
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Возвращает список машин на парковке в отформатированном виде.
        /// </summary>
        /// <returns></returns>
        public string GetCarsInfo()
        {
            lock (LockObj)
            {
                StringBuilder sb = new StringBuilder();
                string separator = new string('-', 40);
                sb.AppendLine(separator);
                sb.AppendLine($"|{"ID",-10}|{"Баланс",-10}|{"Тип",-16 }|");
                sb.AppendLine(separator);

                foreach (Car car in _cars)
                {
                    sb.AppendLine($"|{car.Id.ToString(),-10}|{Decimal.Round(car.Balance, 10).ToString(),-10}|{car.Type.ToString(),-16 }|");
                    sb.AppendLine(separator);
                }

                return sb.ToString();
            }
        }

        #endregion
    }
}
