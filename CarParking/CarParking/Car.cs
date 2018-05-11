using System;

namespace CarParking
{
    /// <summary>
    /// Представляет машину.
    /// </summary>
    public sealed class Car
    {
        private static int GlobalId = 1;

        /// <summary>
        /// Возвращает уникальный идентификатор.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Возвращает баланс.
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// Возвращает тип машины.
        /// </summary>
        public CarType Type { get; private set; }

        public Car(CarType carType, decimal initBalance)
        {
            Id = GlobalId++;
            Type = carType;
            Balance = initBalance;
        }

        public override string ToString()
        {
            return $"ID: {Id.ToString()} Баланс: {Balance.ToString()} Тип: {Type.ToString()}";
        }
    }
}
