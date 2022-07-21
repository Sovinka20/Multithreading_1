using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class CustomData
{
    public long CreationTime;
    public int Name;
    public int ThreadNum;
}

public class Example_0
{


    public static void Main()
    {
        //Подключение внешнего txt-файла
        const string f = "Test.txt";

        int n = 2000000;
        /*"Задание: Напишите консольное приложение на C#, которое на вход принимает большой текстовый файл." +
               " На выходе создает текстовый файл с перечислением всех уникальных слов, встречающихся в тесте, " +
               "и количеством их употреблений, отсортированный в порядке убывания количества употреблений.*/
        string line = "";

        //Символы, которые необходимо удалить, с "-" - проблема, она может быть частью слова
        char[] delimiterChars = { '-', '/', '"', '(', ')','*', '[', ']', ' ', ',',
                    '.', ':', ';', '!', '?', '\t',
                    '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
        int initialCapacity = 101;
        int numProcs = Environment.ProcessorCount;
        int concurrencyLevel = numProcs *20;
        //Создание словаря
        ConcurrentDictionary<string, int> openWith = new ConcurrentDictionary<string, int>(concurrencyLevel, initialCapacity);
        ConcurrentDictionary<string, int> openWith1 = new ConcurrentDictionary<string, int>(concurrencyLevel, initialCapacity);

        Console.WriteLine("Однопоточная реализация");
        Single_threaded_method();
        Console.WriteLine("Многопоточная реализация");
        Multithreading_method();

        void Single_threaded_method()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            using (StreamReader r = new StreamReader(f))
            {
                while ((line = r.ReadLine()) != null)
                {


                    //Перевод всех букв в строчный формат
                    line = line.ToLower();

                    //Разбиение строки на слова
                    string[] words = line.Split(delimiterChars);
                    //Заполнение словаря
                    foreach (string word in words)
                    {
                        if (word != "")
                        {
                            if (!openWith.ContainsKey(word))
                            {
                                //Запись уникального слова в словарь и установка значения 1
                                openWith.TryAdd(word, 1);
                            }
                            else
                            {
                                //Повышении значения словаря, соответствующего ключу на 1
                                openWith[word] += 1;
                            }
                        }
                    }
                }
            }

            using (var writer = new StreamWriter("dict.txt"))
            {
                //Linq реализация сортировки по значениям словаря по убыванию
                foreach (var kvp in openWith.OrderByDescending(x => x.Value))
                {
                    writer.WriteLine($"{kvp.Key}\t{kvp.Value}");
                }
            }

            //Вывод таймера
            Console.WriteLine("Time: " + stopWatch.ElapsedMilliseconds + "мс");
        }


        void Multithreading_method()
        {
            Stopwatch stopWatch1 = Stopwatch.StartNew();
            Task[] taskArray = new Task[2];
            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((Object obj) =>
                {
                    CustomData data = obj as CustomData;
                    if (data == null)
                        return;

                    data.ThreadNum = Thread.CurrentThread.ManagedThreadId;
                },
                                                      new CustomData() { Name = i, CreationTime = DateTime.Now.Ticks });
            }
            Task.WaitAll(taskArray);
            using (StreamReader r = new StreamReader(f))
            {
                foreach (var task in taskArray)
                {
                    var data = task.AsyncState as CustomData;
                    if (data != null)
                    {
                        while ((line = r.ReadLine()) != null)
                        {
                            //Перевод всех букв в строчный формат
                            line = line.ToLower();

                            //Разбиение строки на слова
                            string[] words = line.Split(delimiterChars);
                            //Заполнение словаря
                            foreach (string word in words)
                            {
                                if (word != "")
                                {
                                    if (!openWith1.ContainsKey(word))
                                    {
                                        //Запись уникального слова в словарь и установка значения 1
                                        openWith1.TryAdd(word, 1);
                                    }
                                    else
                                    {
                                        //Повышении значения словаря, соответствующего ключу на 1
                                        openWith1[word] += 1;
                                    }
                                }
                            }
                        }
                    }


                }



            }
            using (var writer = new StreamWriter("dict1.txt"))
            {
                //Linq реализация сортировки по значениям словаря по убыванию
                foreach (var kvp in openWith1.OrderByDescending(x => x.Value))
                {
                    writer.WriteLine($"{kvp.Key}\t{kvp.Value}");
                }
            }
            Console.WriteLine("Time: " + stopWatch1.ElapsedMilliseconds + "мс");

        }




    }
}
