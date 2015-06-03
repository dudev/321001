using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dz3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static List<List<int>> _robots = new List<List<int>>();
        private static List<int> _man = new List<int>();

        private static bool _gameEnd = false;
        private static bool _stepEnd = true;
        private static List<int> _manStep;

        private static List<List<int>> _listImg = new List<List<int>>();

        private void Initialize()
        {
            var rand = new Random();

            _gameEnd = false;
            _stepEnd = true;
            stepButton.Enabled = true;

            do
            {
                _man = new List<int>() { 2, rand.Next(9), rand.Next(9) };
            } while (!CheckPlace(_man[1], _man[2]));

            _robots.RemoveRange(0, _robots.Count);
            while (_robots.Count < 5)
            {
                var elem = new List<int>() { 2, rand.Next(9), rand.Next(9) };
                if (!CheckPlace(elem[1], elem[2]))
                {
                    _robots.Add(elem);
                }
            }
            
            Drawer();
        }

        private void img_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_stepEnd)
            {
                var i = tableLayoutPanel1.Controls.GetChildIndex((PictureBox) sender);
                ClickOnBoard(_listImg[i][0], _listImg[i][1]);
            }
        }

        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_stepEnd)
            {
                int[] widths = tableLayoutPanel1.GetColumnWidths();
                int[] heights = tableLayoutPanel1.GetRowHeights();

                int col = -1;
                int left = e.X;
                for (int i = 0; i < widths.Length; i++)
                {
                    if (left < widths[i])
                    {
                        col = i;

                        break;
                    }
                    else
                        left -= widths[i];
                }

                int row = -1;
                int top = e.Y;
                for (int i = 0; i < heights.Length; i++)
                {
                    if (top < heights[i])
                    {
                        row = i;
                        break;
                    }
                    else
                        top -= heights[i];
                }
                ClickOnBoard(col, row);
            }
        }

        private void ClickOnBoard(int col, int row)
        {
            if (col != -1 && row != -1 && (col != _man[1] || row != _man[2]))
            {
                var img = new PictureBox
                {
                    Image = new Bitmap(Properties.Resources.delete)
                };
                _manStep = new List<int>() { col, row };
                if (!CheckPlace(col, row) && Math.Abs(col - _man[1]) <= 1 && Math.Abs(row - _man[2]) <= 1)
                {
                    Drawer();
                    tableLayoutPanel1.Controls.Add(img, col, row);
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Game(worker);
        }

        private void Game(BackgroundWorker worker)
        {
            while (!_gameEnd)
            {
                foreach (var robot in _robots)
                {
                    RobotStep(robot);
                    worker.ReportProgress(1);
                    Thread.Sleep(150);
                }

                if (!_gameEnd)
                {
                    _stepEnd = false;

                    while (!_stepEnd)
                    {
                        Thread.Sleep(150);
                    }

                    _man[1] = _manStep[0];
                    _man[2] = _manStep[1];

                    if (_robots.Count == 0)
                    {
                        _gameEnd = true;
                    }
                }
            }
        }

        private bool CheckPlace(int col, int row)
        {
            return (_robots.Any(robot => col == robot[1] && row == robot[2]))
                || (col == _man[1] && row == _man[2]);
        }

        private void Drawer()
        {
            tableLayoutPanel1.Controls.Clear();

            PictureBox img;
            _listImg.RemoveRange(0, _listImg.Count);

            foreach (var robot in _robots)
            {
                img = new PictureBox
                {
                    Image = new Bitmap(Properties.Resources.avatar)
                };
                img.MouseClick += new MouseEventHandler(this.img_MouseClick);
                _listImg.Add(new List<int>() { robot[1], robot[2] });
                tableLayoutPanel1.Controls.Add(img, robot[1], robot[2]);
            }

            img = new PictureBox
            {
                Image = new Bitmap(Properties.Resources.avatar5)
            };
            tableLayoutPanel1.Controls.Add(img, _man[1], _man[2]);
        }

        private void RobotStep(List<int> robot)
        {
            int col = robot[1], row = robot[2];
            var rand = new Random();

            if ((col == _man[1] && Math.Abs(row - _man[2]) <= robot[0])
                || (row == _man[2] && Math.Abs(col - _man[1]) <= robot[0]))
            {
                _gameEnd = true;
                return;
            }

            if (rand.NextDouble() < 0.5)
            {
                if (col == 0)
                {
                    col = 1;
                } else if (col == 9)
                {
                    col = 8;
                }
                else
                {
                    if (rand.NextDouble() < 0.5)
                    {
                        col--;
                    }
                    else
                    {
                        col++;
                    }
                }
            }
            else
            {
                if (row == 0)
                {
                    row = 1;
                }
                else if (row == 9)
                {
                    row = 8;
                }
                else
                {
                    if (rand.NextDouble() < 0.5)
                    {
                        row--;
                    }
                    else
                    {
                        row++;
                    }
                }
            }
            if (!CheckPlace(col, row))
            {
                robot[1] = col;
                robot[2] = row;
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            Initialize();
            backgroundWorker1.RunWorkerAsync();
            startButton.Enabled = false;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Drawer();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }

            startButton.Enabled = true;
            stepButton.Enabled = false;

            MessageBox.Show(@"Игра окончена");
        }

        private void stepButton_Click(object sender, EventArgs e)
        {
            if (_manStep == null)
            {
                MessageBox.Show(@"Недопустимый ход");
                return;
            }

            int col = _manStep[0], row = _manStep[1];

            if (col == _man[1] && row == _man[2])
            {
                MessageBox.Show(@"Недопустимый ход");
                return;
            }

            if (!CheckPlace(col, row))
            {
                if (Math.Abs(col - _man[1]) <= 1 && Math.Abs(row - _man[2]) <= 1)
                {
                    _stepEnd = true;
                    return;
                }
            }
            else
            {
                if ((col == _man[1] && Math.Abs(row - _man[2]) <= _man[0])
                    || (row == _man[2] && Math.Abs(col - _man[1]) <= _man[0]))
                {
                    for (int i = 0; i < _robots.Count; i++)
                    {
                        if (_robots[i][1] == col && _robots[i][2] == row)
                        {
                            _robots.RemoveAt(i);
                            Drawer();
                            _stepEnd = true;
                            _manStep[0] = _man[1];
                            _manStep[1] = _man[2];
                            return;
                        }
                    }
                }
            }
            MessageBox.Show(@"Недопустимый ход");
        }
    }
}
