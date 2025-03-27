using System;

namespace WarehouseManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в систему управления складом!");

            string[] products = { "Ноутбук", "Смартфон", "Планшет", "Монитор", "Клавиатура" };
            int[] quantities = { 10, 25, 15, 8, 50 };
            double[] prices = { 1200.50, 800.00, 500.75, 300.00, 50.25 };
            bool[] isFragile = { true, true, true, false, false };

            // Основной цикл обработки товаров
            for (int i = 0; i < products.Length; i++)
            {
                Console.WriteLine($"\nТовар: {products[i]}");
                Console.WriteLine($"Количество: {quantities[i]}");
                Console.WriteLine($"Цена: {prices[i]:C}");
                Console.WriteLine($"Хрупкий: {(isFragile[i] ? "Да" : "Нет")}");

                // Использование switch-case для анализа категории товара
                switch (products[i])
                {
                    case "Ноутбук":
                        Console.WriteLine("Это категория электроники.");
                        if (prices[i] > 1000)
                        {
                            Console.WriteLine("Дорогой товар. Требуется дополнительная проверка.");

                            // Большая вложенность (7-8 уровней)
                            for (int j = 0; j < 2; j++)
                            {
                                Console.WriteLine($"Этап {j + 1}: Проверка состояния товара...");

                                if (j == 0)
                                {
                                    Console.WriteLine("Проверка упаковки...");

                                    for (int k = 0; k < 2; k++)
                                    {
                                        Console.WriteLine($"Подэтап {k + 1}: Анализ коробки...");

                                        if (k == 0)
                                        {
                                            Console.WriteLine("Коробка целая.");

                                            for (int l = 0; l < 2; l++)
                                            {
                                                Console.WriteLine($"Шаг {l + 1}: Проверка содержимого...");

                                                if (l == 0)
                                                {
                                                    Console.WriteLine("Комплектующие на месте.");

                                                    for (int m = 0; m < 2; m++)
                                                    {
                                                        Console.WriteLine($"Действие {m + 1}: Проверка документации...");

                                                        if (m == 0)
                                                        {
                                                            Console.WriteLine("Документы в порядке.");

                                                            for (int n = 0; n < 2; n++)
                                                            {
                                                                Console.WriteLine($"Операция {n + 1}: Проверка гарантии...");

                                                                if (n == 0)
                                                                {
                                                                    Console.WriteLine("Гарантия действительна.");

                                                                    for (int o = 0; o < 2; o++)
                                                                    {
                                                                        Console.WriteLine($"Процедура {o + 1}: Подготовка к отправке...");

                                                                        if (o == 0)
                                                                        {
                                                                            Console.WriteLine("Товар готов к отправке.");
                                                                        }
                                                                        else
                                                                        {
                                                                            Console.WriteLine("Требуется дополнительная упаковка.");
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("Гарантия истекла.");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Документы отсутствуют.");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Комплектующие повреждены.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Коробка повреждена.");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Подготовка товара к отгрузке...");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Бюджетный товар. Отправка без дополнительной проверки.");
                        }
                        break;

                    case "Смартфон":
                        Console.WriteLine("Это портативное устройство.");
                        if (isFragile[i])
                        {
                            Console.WriteLine("Требуется осторожная упаковка.");
                        }
                        else
                        {
                            Console.WriteLine("Упаковка стандартная.");
                        }
                        break;

                    case "Планшет":
                        Console.WriteLine("Это планшет.");
                        if (quantities[i] < 20)
                        {
                            Console.WriteLine("Запасы заканчиваются. Необходимо пополнить.");
                        }
                        else
                        {
                            Console.WriteLine("Запасы в норме.");
                        }
                        break;

                    default:
                        Console.WriteLine("Это товар общего назначения.");
                        break;
                }

                // Простая проверка количества товара
                if (quantities[i] > 20)
                {
                    Console.WriteLine("Товар в достаточном количестве.");
                }
                else if (quantities[i] > 0)
                {
                    Console.WriteLine("Товар заканчивается.");
                }
                else
                {
                    Console.WriteLine("Товар отсутствует на складе.");
                }
            }

            // Дополнительная логика
            Console.WriteLine("\nПроверка дополнительных условий...");

            int randomValue = new Random().Next(1, 10);

            if (randomValue > 5)
            {
                Console.WriteLine("Случайное значение больше 5.");
            }
            else
            {
                Console.WriteLine("Случайное значение меньше или равно 5.");
            }

            // Логика для проверки хрупких товаров
            Console.WriteLine("\nПроверка хрупких товаров на складе...");

            for (int i = 0; i < products.Length; i++)
            {
                if (isFragile[i])
                {
                    Console.WriteLine($"Товар '{products[i]}' хрупкий. Обращаться осторожно.");
                }
                else
                {
                    Console.WriteLine($"Товар '{products[i]}' не хрупкий.");
                }
            }

            Console.WriteLine("\nРабота системы завершена. Спасибо за использование!");
        }
    }
}
