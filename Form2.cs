using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace coursework_RR_Puzzles
{
    public partial class Form2 : Form
    {
        Image image;
        public Form2()
        {
            InitializeComponent();
        }

        //Добавляем перегруженный конструктор формы, принимающий на вход изображение
        public Form2(Image t_image)
        {
            //Добавляем изображение из Form1 в Form2
            image = t_image;
            InitializeComponent();
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            try 
            {
                //Отрисовываем изрображение на всю форму
                Graphics g = e.Graphics;
                g.DrawImage(image, new Rectangle(0, 0, 600, 370));
            }
            catch { }
            
        }
    }
}
