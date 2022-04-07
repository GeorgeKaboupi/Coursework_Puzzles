using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace coursework_RR_Puzzles
{
    public partial class Form1 : Form
    {
        //Объявление глобальных переменных
        public Image image;
        public Form2 helpPicture = new Form2();
        //Список с изображениями-пазлами
        List<Image> imageList = new List<Image>();
        public bool imageNew = true, rectSelected = false;
        public Rectangle selectRect;
        //Переменная кол-ва пазлов на стороне, строка и столбец для перестановки тайлов
        public int size, selectedX = 0, selectedY = 0, movesCount = 0;
        public int[] orderArray = new int[49];
        public Pen selectPen = new Pen(Color.FromArgb(0, 200, 255, 0), 10);
        //Глобальный путь к папке с рекордами, используется пространство имён System.Reflection
        public string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\data.txt";
        //Массив для хранения рекордов
        public string[] bestResult = new string[6];

        public Form1()
        {
            InitializeComponent();
            string[] difficulties = { "4", "9", "16", "25", "36", "49" };
            for (int i = 0; i < 49; i++) orderArray[i] = i + 1;
            comboBox1.Items.AddRange(difficulties);
            comboBox1.SelectedIndex = 0;
            LabelMovesCount.Text = $"Moves made: 0\nBest result: {GetBestResult(comboBox1.SelectedIndex)}";
        }
        private void LoadImage_Click(object sender, EventArgs e)
        {
            //Открытие диалогового окна для выбора картинки
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Файлы изображений|*.bmp;*.png;*.jpg";
            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            try { image = Image.FromFile(openDialog.FileName); }
            catch (OutOfMemoryException ex)
            {
                MessageBox.Show("Ошибка чтения картинки");
                return;
            }
            
            //Масштабирование картинки во избежание проблем с используемой памятью
            image = image.GetThumbnailImage(700, 700, null, IntPtr.Zero);
            imageList.Clear();
            //Булевская переменная, означающая, что картинка была загружена и игра ещё не началась
            imageNew = true;
            //Обновление отрисовки формы
            Invalidate();
        }
        //Метод для выбора пазла, который выбрали
        public void DoSelect(int i, int j)
        {
            //227 и 17 - отступы от краёв, левый верхний угол изначальной картинки
            selectRect = new Rectangle(227 + j * 550 / size, 17 + i * 350 / size, 550 / size, 350 / size);
            //Прозрачность ручки 255/255
            selectPen = new Pen(Color.FromArgb(255, 200, 255, 0), 5);
        }
        //Метод отрисовки
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int count = 0;

            try 
            {
                //Если игра не началась, то отрисовываем саму картинку
                if(imageNew) g.DrawImage(image, new Rectangle(227, 17, 550, 350));
                else
                {
                    int k = 0;
                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            //Отрисовываем каждую картинку, сдвигая левый верхний угол, согласно формуле
                            g.DrawImage(imageList[k], new Rectangle(227 + j * 550 / size, 17 + i * 350 / size,
                                550 / size, 350 / size));
                            //Отрисовываем обводку выбора
                            g.DrawRectangle(selectPen, selectRect);
                            k++;
                        }
                    }
                    //Проверка на победу
                    for(int i = 1; i < size * size; i++)
                    {
                        if (orderArray[i] - 1 == orderArray[i - 1]) count++;
                    }
                    if (count == size * size - 1) Win(sender, e);
                }
            }
            catch { ArgumentNullException ex; }
        }
        //StreamReader, считывание файла с рекордами
        public int GetBestResult(int i)
        {
            try
            { 
                using (StreamReader sr = new StreamReader(path)) 
                {
                    //Заполняем массив рекордов
                    bestResult = sr.ReadToEnd().Split('\n');
                    //Возвращаем рекорд, соответствующий выбранной сложности
                    return Convert.ToInt32(bestResult[i]);
                } 
            }
            catch (Exception e) { return 0; }
        }
        //StreamWriter, изменение лучшего результата
        public void SetBestResult(int result, int id)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    for(int i = 0; i < 6; i++)
                    {
                        //Если сложность соответствует строчке, то заносим новый рекорд
                        if(i == id) sw.WriteLine($"{result}");
                        else sw.WriteLine(GetBestResult(i));
                    }
                }
            }
            catch (Exception e) { }
        }
        //Метод, вызывающийся при победе (Картинка собрана)
        public void Win(object sender, EventArgs e)
        {
            //Задаём значение новой картинки для новой игры
            imageNew = true;
            //Стринговая переменная. Будет использована, если игрок поставил новый рекорд
            string messageCongrats = ", new moves record!";
            //Если игрок не поставил новый рекорд, то переменная "зануляется"
            if (movesCount > GetBestResult(comboBox1.SelectedIndex)) { messageCongrats = ""; }
            else { SetBestResult(movesCount, comboBox1.SelectedIndex); }
            //Используем MessageBox для вывода сообщения о победе с выбором о продолжении новой игры
            var result = MessageBox.Show($"You won!\nMoves made: {movesCount}{messageCongrats}\n" +
                $"Would you like to start a new game?", 
                "Congratulations!", MessageBoxButtons.YesNo);
            //При "Да" начинается новая игра, при "Нет" картинка удаляется и игра не начинается автоматически
            if(result == DialogResult.Yes) StartButton_Click(sender, e);
            else image.Dispose();
            //Закрытие формы-помощника с изображением
            LabelMovesCount.Text = $"Moves made: { movesCount = 0 }\nBest result: { GetBestResult(comboBox1.SelectedIndex) }";
            helpPicture.Close();
            Invalidate();
        }
        //Метод вырезания пазла из картинки (Принимает изначальное изображение и область вырезки)
        public Image cropImage(Image img, Rectangle cropArea)
        {
            //Используя битмапы, "копируем" маленькое изображение и возвращаем его (Bitmap = Image)
            Bitmap bpmImage = new Bitmap(img);
            return bpmImage.Clone(cropArea, bpmImage.PixelFormat);
        }
        //Метод перестановки пазлов
        public void SwapImages(List<Image> list, int id1, int id2)
        {
            Image tempImage = list[id1];
            list[id1] = list[id2];
            list[id2] = tempImage;
        }
        //Метод перестановки их id
        public void SwapIds(int id1, int id2)
        {
            int temp = orderArray[id1];
            orderArray[id1] = orderArray[id2];
            orderArray[id2] = temp;
        }
        //Кнопка для вызова вспомогательного окошка с изначальным изображением
        private void HelpButton_Click(object sender, EventArgs e)
        {
            //Используем перегруженный конструктор Form2 (см. Form2.cs)
            helpPicture = new Form2(image);
            helpPicture.Show();
        }
        //Метод, при вызове которого начинается игра
        private void StartButton_Click(object sender, EventArgs e)
        {
            try
            {
                Random rand = new Random();
                //Обозначаем начало игры
                imageNew = false;
                imageList.Clear();
                LabelMovesCount.Text = $"Moves made: 0\nBest result: {GetBestResult(comboBox1.SelectedIndex)}";

                //В список изображений заносим пазлы, используя вышеуказанный метод cropImage
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        imageList.Add(cropImage(image, new Rectangle(j * image.Height / size, i * image.Height / size, image.Width / size, image.Height / size)));
                    }
                }

                //Случайным образом расставляем тайлы на форме
                int id1, id2;
                for (int i = 0; i < size * size - 1; i++)
                {
                    id1 = rand.Next(0, size * size);
                    id2 = rand.Next(0, size * size);
                    SwapImages(imageList, id1, id2);
                    SwapIds(id1, id2);
                }

                //Обновляем форму
                Invalidate();
            }
            catch { }
        }
        //Метод нажатия на форму
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            //Работает только если началась игра, в противном случае работают лишь нажатия на кнопки
            if(!imageNew)
            {
                //Считываем координаты нажатия курсора мыши, переводя их в значения от [0; size]
                //rectSelected == false нужен для отмены выделения пазла при повторном нажатии
                if (e.X >= 227 && e.X <= 777 &&
               e.Y >= 17 && e.Y <= 367 && rectSelected == false)
                {
                    int tileY = (e.X - 227) / (550 / size),
                        tileX = (e.Y - 17) / (350 / size);
                    //Выделяем выбранный тайл вышеописанным методом DoSelect()
                    DoSelect(tileX, tileY);
                    selectedX = tileX;
                    selectedY = tileY;
                    //rectSelected = true для обозначения ранее выбранного тайла
                    rectSelected = true;
                }
                //Считываем координаты нажатия курсора мыши, если уже был выбран один пазл
                else if (e.X >= 227 && e.X <= 777 &&
                   e.Y >= 17 && e.Y <= 367 && rectSelected == true)
                {
                    //Так же считываем и переводим координаты в значения от [0; size]
                    int tileY = (e.X - 227) / (550 / size),
                        tileX = (e.Y - 17) / (350 / size);
                    //Меняем местами картинки и их идентификаторы
                    SwapImages(imageList, selectedX * size + selectedY, tileX * size + tileY);
                    SwapIds(selectedX * size + selectedY, tileX * size + tileY);
                    //Убираем прозрачность ручки, делая обводку невидимой (Убираем обводку)
                    selectPen = new Pen(Color.FromArgb(0, 200, 255, 0), 5);
                    rectSelected = false;
                    //Был совершён ход -> увеличиваем кол-во ходов
                    LabelMovesCount.Text = $"Moves made: {++movesCount}\nBest result: {GetBestResult(comboBox1.SelectedIndex)}";
                }
                //В любом другом случае (Когда нажимаем не на пазл)
                else
                {
                    //Убираем прозрачность ручки, делая обводку невидимой (Убираем обводку)
                    selectPen = new Pen(Color.FromArgb(0, 200, 255, 0), 5);
                    rectSelected = false;
                }
                Invalidate();
            }
        }
        //Метод изменения значения в comboBox1
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Меняем значение кол-ва пазлов на каждой стороне
            size = Convert.ToInt32(Math.Sqrt(Convert.ToInt32(comboBox1.SelectedItem.ToString())));
            //Зануляем кол-во ходов
            LabelMovesCount.Text = $"Moves made: {movesCount = 0}\nBest result: {GetBestResult(comboBox1.SelectedIndex)}";
            //Если смена сложности была проведена после начала игры
            if (!imageNew)
            {
                //Заново заполняем массив идентификаторов
                for (int i = 0; i < 49; i++) orderArray[i] = i + 1;
                //Начинаем новую игру
                StartButton_Click(sender, e);
            }
        }
    }
}
