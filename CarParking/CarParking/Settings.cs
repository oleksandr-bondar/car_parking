using System;
using System.Collections.Generic;

namespace CarParking
{
    /// <summary>
    /// Представляет класс для содержания некоторых настроек, которые использует экземпляр класса <see cref="Parking"/>.
    /// </summary>
    public static class Settings
    {
        private static readonly int _timeout = 3;// in seconds
        private static readonly int _parkingSpace = 50;// max places
        private static readonly decimal _fine = 1.05M;

        private static readonly Dictionary<CarType, decimal> _prices 
            = new Dictionary<CarType, decimal>()
        {
            { CarType.Truck, 5 },
            { CarType.Passenger, 3 },
            { CarType.Bus, 2 },
            { CarType.Motorcycle, 1 }
        };

        /// <summary>
        /// Возвращает время в секундах, через которое происходит списывание средств за парковочное место.
        /// </summary>
        public static int Timeout => _timeout;
        /// <summary>
        /// Возвращает вместимость парковки (общее кол-во мест).
        /// </summary>
        public static int ParkingSpace => _parkingSpace;
        /// <summary>
        /// Возвращает коэффициент штрафа.
        /// </summary>
        public static decimal Fine => _fine;

        /// <summary>
        /// Возвращает цену за парковку для определенного типа машин.
        /// </summary>
        /// <param name="carType">Тип машины.</param>
        /// <returns></returns>
        public static decimal GetParkingPrice(CarType carType)
        {
            if (_prices.TryGetValue(carType, out decimal price))
                return price;

            throw new InvalidOperationException("Для указанного типа машин не задана цена.");
        }

        static Settings()
        {

        }
    }
}
