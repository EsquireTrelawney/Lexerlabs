using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Lexer
{

    public partial class Form1 : Form
    {
        //объявление глобальных переменных
        public List<string> service;
        public List<string> operators;
        public List<string> separators;
        public List<string> identificators;
        public List<string> numbers;
        public string code; //строка, хранящая изначальный код для анализа
        public string lex; //строка хранящая конечный набор лексем
        public Form1()
        {
            //Инициализируем компоненты
            InitializeComponent();

            service = new List<string>() { "C", "CALL", "CHARACTER", "DIMENSION", "END", "FUNCTION", "GOTO", "IF", "INTEGER", "PROGRAM", "REAL", "STOP", "SUBROUTINE" };
            operators = new List<string>() { "+", "-", "*", "/", "**", ".GT.", ".LT.", ".GE.", ".LE.", ".NE.", ".EQ.", "=" };
            separators = new List<string>() { " ", "(", ")", ",", "EOL"};
            identificators = new List<string>(1);
            numbers = new List<string>(1);

            code = "";
            lex = "";
        }

        //функция вызова шага анализатора
        public string Analysis()
        {
            //bool finish = false; //логическая переменная отслеживающая завершение шага анализатора
            bool flagid = false; //логическая переменная отслеживающая отношение текущей лексемы к идентификаторам
            int iteration = 0; //счетчик иттераций работы анализатора
            int length = code.Length; //текущая длина строки с анализируемым кодом
            List <char> tokens = new List<char>(1); //список с символами для анализа
            string lexem; //полученная после анализа лексическая единица
            //MessageBox.Show(code);
            if (length <= 3) return ("EoF"); //предпроверка на конец файла

                tokens.Add(code[iteration]);

            if ((tokens[0] >= 65) && (tokens[0] <= 90)) //первый вошедший символ - буква
            {
                while ((iteration+1<=length) && (((code[iteration + 1] >= 65) && (code[iteration + 1] <= 90)) || ((code[iteration + 1] >= 48) && (code[iteration + 1] <= 57)))) //добавляем символы лексемы, пока не встретим неподходящий символ
                {    
                    if ((code[iteration + 1] >= 48) && (code[iteration + 1] <= 57)) flagid = true;
                    tokens.Add(code[++iteration]);
                }

                lexem = ListToStr(tokens); //конвертируем в строку полученный набор символов
                if (flagid) //если в лексеме есть цифры, то ищем совпадения в идентификаторах, иначе добавляем новый
                {
                    int id = identificators.IndexOf(lexem);

                    if (id == -1) //совпадений нет, добавляем новый идентификатор
                    {
                        identificators.Add(lexem);
                        dataGridView1.Rows.Add(identificators.Count - 1, lexem);
                        lexem = "I" + Convert.ToString(identificators.Count - 1);
                        code = code.Substring(iteration+1);
                        code = code.Trim();
                        return (lexem);
                    }
                    else //совпадение в идентификаторах найдено
                    {
                        lexem = "I" + Convert.ToString(id);
                        code = code.Substring(iteration+1);
                        code = code.Trim();
                        return (lexem);
                    }
                }
                else //если лексема без цифр, то проверяем все варианты
                {
                    if (lexem == separators[4]) //если лексема разделитель конца строки
                    {
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return ("S4");
                    }

                    int id = service.IndexOf(lexem);
                    if (id != -1) //если лексема служебное слово
                    {
                        lexem = "W" + Convert.ToString(id);
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return (lexem);
                    }

                    id = identificators.IndexOf(lexem); //ищем лексему среди идентификаторов
                    if (id == -1) //совпадений нет, добавляем новый идентификатор
                    {
                        identificators.Add(lexem);
                        dataGridView1.Rows.Add(identificators.Count - 1, lexem);
                        lexem = "I" + Convert.ToString(identificators.Count - 1);
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return (lexem);
                    }
                    else //совпадение в идентификаторах найдено
                    {
                        lexem = "I" + Convert.ToString(id);
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return (lexem);
                    }
                }
                
            }
            else if ((tokens[0] >= 48) && (tokens[0] <= 57)) //первый символ - цифра
            {
                while ((iteration+1 <= length) && (((code[iteration+1] >= 48) && (code[iteration + 1] <= 57)) || (code[iteration + 1] == 46))) //Добавляем символы в лексему, пока не закончится файл, либо не попадем на разделитель (исключая точку)
                    tokens.Add(code[++iteration]);

                lexem = ListToStr(tokens); //конвертируем в строку полученный набор символов

                int id = numbers.IndexOf(lexem); //ищем совпадения в числах
                if (id == -1) //совпадений нет, создаем новое число
                {
                    numbers.Add(lexem);
                    dataGridView2.Rows.Add(identificators.Count - 1, lexem);
                    lexem = "N" + Convert.ToString(numbers.Count - 1);
                    code = code.Substring(iteration + 1);
                    code = code.Trim();
                    return (lexem);
                }
                else //совпадение найдено, записываем лексему
                {
                    lexem = "N" + Convert.ToString(id);
                    code = code.Substring(iteration + 1);
                    code = code.Trim();
                    return (lexem);
                }

            }
            else if (tokens[0] == 46) //первый символ - точка
            {
                if ((code[iteration + 1] >= 48) && (code[iteration + 1] <= 57)) //следующий символ цифра
                {
                    while ((iteration + 1 <= length) && ((code[iteration + 1] >= 48) && (code[iteration + 1] <= 57))) //добавляем в лексему все цифры после точки
                        tokens.Add(code[++iteration]);

                    lexem = ListToStr(tokens); //конвертируем в строку полученный набор символов

                    int id = numbers.IndexOf(lexem); //ищем совпадения в числах
                    if (id == -1) //совпадений нет, создаем новое число
                    {
                        numbers.Add(lexem);
                        dataGridView2.Rows.Add(identificators.Count - 1, lexem);
                        lexem = "N" + Convert.ToString(numbers.Count - 1);
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return (lexem);
                    }
                    else //совпадение найдено, записываем лексему
                    {
                        lexem = "N" + Convert.ToString(id);
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return (lexem);
                    }
                }
                else if ((iteration + 4 <= length) && (code[iteration + 1] >= 65) && (code[iteration + 1] <= 90))
                {
                    for (int i = iteration; iteration < i+3; )
                        tokens.Add(code[++iteration]);

                    lexem = ListToStr(tokens); //конвертируем в строку полученный набор символов

                    int id = operators.IndexOf(lexem); //ищем совпадения в операторах
                    if (id != -1) //если найдено выводим лексему
                    {
                        lexem = "O" + Convert.ToString(id);
                        code = code.Substring(iteration + 1);
                        code = code.Trim();
                        return (lexem);
                    }
                    else return ("ERROR"); //если совпадений нет, то синтаксис программы неверен

                }
                else return ("ERROR");
            }
            else
            {
                while ((iteration + 1 <= length) && (((code[iteration + 1] >= 40) && (code[iteration + 1] <= 45)) || (code[iteration + 1] == 47) || (code[iteration+1] == 61)))
                {
                    tokens.Add(code[++iteration]);
                }

                lexem = ListToStr(tokens); //конвертируем в строку полученный набор символов

                int id = separators.IndexOf(lexem); //ищем совпадения в разделителях
                if (id != -1) //если лексема разделитель
                {
                    lexem = "S" + Convert.ToString(id);
                    code = code.Substring(iteration + 1);
                    code = code.Trim();
                    return (lexem);
                }

                id = operators.IndexOf(lexem); //ищем совпадения в операторах
                if (id != -1) //если лексема оператор
                {
                    lexem = "O" + Convert.ToString(id);
                    code = code.Substring(iteration + 1);
                    code = code.Trim();
                    return (lexem);
                }
            }
            return ("ERROR");
        }

        public bool StepAnalysis()
        {
            //вызов метода анализатора
            string step = Analysis();
            //проверка возвращаемого результата на возможное окончание кода
            if (step == "ERROR")
            {
                MessageBox.Show("Найден необрабатываемый символ!");
                return (false);
            }
            else if (step == "EoF")
            {
                MessageBox.Show("Код полностью проанализирован!");
                buttonStep.Enabled = false;
                buttonFull.Enabled = false;
                return (false);
            }
            else
            {
                lex += step + " ";
                textBoxFinal.Text += step + " ";
                return (true);
            }
        }

        public string ListToStr(List<char> list) //функция преобразования списка в строку
        {
            string str;
            char[] array = list.ToArray();
            str = new string(array);
            return (str);
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            string temp; //временная переменная хранящая строку из файла

            StreamReader f = new StreamReader("code.txt");

            //обнуление и активация элементов формы
            textBoxCode.Text = "";
            textBoxFinal.Text = "";
            buttonStep.Enabled = true;
            buttonFull.Enabled = true;
            //цикл считывания кода из файла
            while (!f.EndOfStream)
            {
                temp = f.ReadLine();
                code += temp.Trim() + " EOL "; //заполнение рабочей строки
                textBoxCode.Text += temp + Environment.NewLine; //заполнение окна вывода пользователю
            }
            code += "EOF";
            f.Close();

            //удаление пробелов в строке кода и перевод символов в верхний регистр
            //string[] partstr = code.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //code = String.Join("", partstr);
            code = code.ToUpper();
        }

        //метод пошагового анализа
        private void buttonStep_Click(object sender, EventArgs e)
        {
            StepAnalysis();
        }

        private void buttonFull_Click(object sender, EventArgs e)
        {
            bool flag = true;
            while(flag)
            {
             flag = StepAnalysis();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
