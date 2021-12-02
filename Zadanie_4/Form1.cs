using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using ZedGraph;

namespace Zadanie_4
{
    public partial class Form1 : Form
    {
        double eps;
        int n;
        double[] b;
        double[,] A;
        public Form1()
        {
            InitializeComponent();
            OnLoad();
        }
        static double[,] Input(out int n, out double[] b)
        {
            string s;
            n = -1;
            using (StreamReader file = new StreamReader("f.txt"))
            {
                s = file.ReadToEnd();
            }
            using (StreamReader file = new StreamReader("f.txt"))
            {
                string tmp;
                do
                {
                    tmp = file.ReadLine();
                    n++;
                } while (tmp != null);
            }
            string[] stroka = s.Split('\n');
            string[] stolbec = stroka[0].Split(' ');
            double[,] a = new double[n, n];
            b = new double[n];
            int t;
            for (int i = 0; i < stroka.Length; i++)
            {
                stolbec = stroka[i].Split(' ');
                for (int j = 0; j < stolbec.Length - 1; j++)
                {
                    t = Convert.ToInt32(stolbec[j]);
                    a[i, j] = t;
                }
                t = Convert.ToInt32(stolbec[n]);
                b[i] = t;
            }
            return a;
        }

        void OnLoad()
        {
            n = 0;
            b = new double[n];
            A = Input(out n, out b);
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    label2.Text += Convert.ToString(A[i, j]) + "  ";
                label2.Text += "\n";
                label4.Text += Convert.ToString(b[i]) + "\n";
            }


            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "График зависимости нормы невязки от номера итерации";
            myPane.XAxis.Title.Text = "Номер итерации";
            myPane.YAxis.Title.Text = "Значение нормы невязки";
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;
            //myPane.YAxis.MinorGrid.IsVisible = true;
            myPane.XAxis.MinorGrid.IsVisible = true;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 1;
        }

        double residual_norm(double[] x)
        {
            double[] r = new double[n];
            for (int i = 0; i < n; i++)
            {
                double s = 0;
                for (int j = 0; j < n; j++)
                    s += A[i, j] * x[j];
                r[i] = s - b[i];
            }

            double norm = 0;
            for (int i = 0; i < n; i++)
                norm += r[i] * r[i];
            norm = Math.Sqrt(norm);
            return norm;
        }

        double[] Jacobi(out int numberOfIter)
        {
            GraphPane myPane = zedGraphControl1.GraphPane;
            PointPairList list = new PointPairList();
            numberOfIter = 0;
            double[] TempX = new double[n];
            double norm;
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
                x[i] = 1;

            do
            {
                list.Add(numberOfIter, residual_norm(x));
                for (int i = 0; i < n; i++)
                {
                    TempX[i] = b[i];
                    for (int k = 0; k < n; k++)
                    {
                        if (i != k)
                            TempX[i] -= A[i, k] * x[k];
                    }
                    TempX[i] /= A[i, i];
                }
                norm = Math.Abs(x[0] - TempX[0]);
                for (int i = 0; i < n; i++)
                {
                    if (Math.Abs(x[i] - TempX[i]) > norm)
                        norm = Math.Abs(x[i] - TempX[i]);
                    x[i] = TempX[i];
                }
                numberOfIter++;
            } while (norm > eps);

            list.Add(numberOfIter, residual_norm(x));
            LineItem myCurve = myPane.AddCurve("Jacobi", list, Color.Blue, SymbolType.Circle);
            return x;
        }

        double[] Matr_prod_Vector(double[] vector)
        {
            double[] R = new double[n];
            for (int i = 0; i < n; i++)
            {
                R[i] = 0;
                for (int j = 0; j < n; j++)
                    R[i] += A[i,j] * vector[j];
            }
            return R;
        }
        double[] MinResid(out int numberOfIter)
        {
            GraphPane myPane = zedGraphControl1.GraphPane;
            PointPairList list = new PointPairList();
            double[] x = new double[n];
            double[] TempX = new double[n];
            double[] Delta = new double[n];
            double[] R;
            double norm, Tau = 0, TempTau = 0;
            numberOfIter = 0;
            for (int i = 0; i < n; i++)
                TempX[i] = 1;

            do
            {
                list.Add(numberOfIter, residual_norm(TempX));
                R = Matr_prod_Vector(TempX);
                for (int i = 0; i < n; i++)
                {
                    Delta[i] = R[i] - b[i];
                }
                R = Matr_prod_Vector(Delta);
                Tau = 0;
                TempTau = 0;
                for (int i = 0; i < n; i++)
                {
                    Tau += R[i] * Delta[i];
                    TempTau += R[i] * R[i];
                }
                Tau /= TempTau;
                for (int i = 0; i < n; i++)
                    x[i] = TempX[i] - Tau * Delta[i];
                norm = Math.Abs(x[0] - TempX[0]);
                for (int i = 0; i < n; i++)
                {
                    if (Math.Abs(x[i] - TempX[i]) > norm)
                        norm = Math.Abs(x[i] - TempX[i]);
                    TempX[i] = x[i];
                }
                numberOfIter++;
            } while (norm >= eps);

            list.Add(numberOfIter, residual_norm(TempX));
            LineItem myCurve = myPane.AddCurve("MinResid", list, Color.Red, SymbolType.Circle);
            return x;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            eps = Convert.ToDouble(textBox1.Text);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int iter_count;
            double[] x = Jacobi(out iter_count);
            label8.Text = "";
            for (int i = 0; i < n; i++)
                label8.Text += x[i] + "  ";
            label12.Text = Convert.ToString(iter_count);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iter_count;
            double[] x = MinResid(out iter_count);
            label10.Text = "";
            for (int i = 0; i < n; i++)
                label10.Text += x[i] + "  ";
            label6.Text = Convert.ToString(iter_count);

        }
    }
}