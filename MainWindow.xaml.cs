using Petzold.Media2D;//biblioteca sageti
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro;//biblioteca ui

namespace WpfApplication4
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // get the theme from the current application
            var theme = ThemeManager.DetectAppStyle(Application.Current);

            // now set the Green accent and dark theme
            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent("Cyan"),
                                        ThemeManager.GetAppTheme("BaseDark"));

            base.OnStartup(e);
        }
    }
  

    public partial class MainWindow
    {


      
        int buton = -1;//preselectie buton
        int nnr = 1;//numarul nodurilor
        int nnm = 1;//nr muchii
        int pn1 = -1, pn2 = -1;//pozitia nodurilor de start si de sfarsit a unei noi muchii
        int ok = 1;
        Ellipse[] noduri = new Ellipse[32767];//nodurile deja 
        Label[] nr_label = new Label[32767];//textul din nodul respectiv[i]
        Point s1, s2;//inceput muchie
        Ellipse es, ef; //create pentru creerea muchiilor si indexare corespunzatoare
        Ellipse global_ellipse;//nod global
        ArrowLine global_sageata;//sageata globala

        //structura muchii
        ArrowLine[] muchii = new ArrowLine[32767];
        Ellipse[] ns = new Ellipse[32767];
        Ellipse[] nf = new Ellipse[32767];
        //structura muchii


        int[,] matrice = new int[500, 500];//matrice de adicenta
        int[] coada = new int[25000];//coada
        int[] vizitat = new int[500];//vectori de vizitati


        DispatcherTimer pauza = new DispatcherTimer();//cronometru


        public MainWindow()
        {
            InitializeComponent();
            s1.X = -1;
            s1.Y = -1;
            s2.X = -1;
            s2.Y = -1;

            es = new Ellipse();//Elipse create pentru crearea muchiilor si indexarea elipselor corespunzatoare
            ef = new Ellipse();
            global_ellipse = new Ellipse();//elipsa globala de lucru


            //eventuri
            ecran.MouseLeftButtonUp += canvas1_MouseUp;
            ecran.MouseLeftButtonDown += canvas1_MouseDown;
            ecran.MouseMove += canvas1_MouseMove;
        }



        //<----Functii Mouse---->
        Point p;


        //A-buton mouse lasat
        private void canvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == e.OriginalSource as Ellipse)
            {
                Ellipse e2;
                e2 = new Ellipse();
                e2 = (Ellipse)e.OriginalSource as Ellipse;
                e2.ReleaseMouseCapture();
            }
        }


        //B-buton mouse apasat 
        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int pozitie = 0, i;
            if (e.OriginalSource == e.OriginalSource as Ellipse)
            {
                global_ellipse = (Ellipse)e.OriginalSource as Ellipse;

                //adaugarea de noduri
                if (buton == 0 && (faramuchie(global_ellipse) == 0))
                {
                    global_ellipse.CaptureMouse();
                    p = e.GetPosition(global_ellipse);
                }


                //stergerea nodului selectat
                else if (buton == 1)
                {   //->>stergere nod
                    for (i = 1; i <= nnr; i++)
                    {
                        if (noduri[i] == global_ellipse) pozitie = i;
                    }

                    //--->daca are muchii
                    if (faramuchie(global_ellipse) == 1)
                    {
                        for (i = 1; i <= nnm; i++)
                        {
                            if (ns[i] == global_ellipse || nf[i] == global_ellipse)
                            {
                                int n1 = 0, n2 = 0;
                                ecran.Children.Remove(muchii[i]);
                                //cautare noduri pentru a putea fi scoase din matricea de adiacnta+din structura de muchii
                                for (int j = 1; j <= nnr; j++)
                                {
                                    if (noduri[j] == ns[i]) n1 = j;
                                }
                                for (int j = 1; j <= nnr; j++)
                                {
                                    if (noduri[j] == nf[i]) n2 = j;
                                }
                                muchii[i] = null;
                                ns[i] = null;
                                nf[i] = null;
                                matrice[n1, n2] = 0;
                            }
                        }
                    }
                    ecran.Children.Remove(global_ellipse);
                    ecran.Children.Remove(nr_label[pozitie]);
                }


                ////creare muchii+matrice de adiacenta
                else if (buton == 2)
                {

                    if (e.OriginalSource == e.OriginalSource as Ellipse)
                    {
                        //Gasire nod
                        Point p;
                        p = e.GetPosition(global_ellipse);
                        if (s1.X == -1 && s1.Y == -1 && s2.X == -1 && s2.Y == -1)
                        {
                            es = global_ellipse;//elipsa de start 
                            for (i = 1; i <= nnr; i++)
                            {
                                if (noduri[i] == global_ellipse) pn1 = i;//gasirea nodului de start a muchiei
                            }
                            s1.X = Canvas.GetLeft(global_ellipse) + 15;
                            s1.Y = Canvas.GetTop(global_ellipse) + 15;

                        }
                        else if (s1.X != -1 && s1.Y != -1 && s2.X == -1 && s2.Y == -1)
                        {
                            s2.X = Canvas.GetLeft(global_ellipse) + 15;
                            s2.Y = Canvas.GetTop(global_ellipse) + 15;
                            if (s1 != s2)
                            {
                                ef = global_ellipse;
                                if (muchieidentica(es, ef) == 0) desenl(s1, s2, es, ef);//trasarea muchiei intre cele 2 noduri
                                for (i = 1; i <= nnr; i++)
                                {
                                    if (noduri[i] == global_ellipse) pn2 = i;//gasirea nodului final a muchiei curenta 
                                }
                                matrice[pn1, pn2] = 1;//creare muchie in matricea de adiacenta 
                            }
                            s1.X = -1; s1.Y = -1; s2.X = -1; s2.Y = -1;
                        }
                    }
                }
                ////creare muchii+matrice de adiacenta

                //Start BFS
                if (buton == 4)
                {
                    if (ok == 1)
                    {
                        bfs(global_ellipse);
                        ok = 0;
                    }


                }

            }

            //stergere muchii
            else if (buton == 3 && e.OriginalSource == e.OriginalSource as ArrowLine)
            {
                global_sageata = e.OriginalSource as ArrowLine;
                ecran.Children.Remove(global_sageata);
                eliminare_muchie(global_sageata);
            }


        }


        //C mouse miscat
        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (buton == 0 && e.OriginalSource == e.OriginalSource as Ellipse)
            {
                int pozitie = 32768, i;//pozitia elipsei
                global_ellipse = (Ellipse)e.OriginalSource as Ellipse;
                p = e.GetPosition(global_ellipse);
                Point x = e.GetPosition(global_ellipse);
                for (i = 1; i <= nnr; i++)
                {
                    if (noduri[i] == global_ellipse) pozitie = i;
                }
                if (e.LeftButton == MouseButtonState.Pressed && faramuchie(global_ellipse) == 0)
                {
                    if (Canvas.GetLeft(global_ellipse) + (x.X - 15) > 0 && Canvas.GetLeft(global_ellipse) + (x.X + 15) < ecran.ActualWidth)
                    {
                        Canvas.SetLeft(global_ellipse, Canvas.GetLeft(global_ellipse) + (x.X - 15));
                        Canvas.SetLeft(nr_label[pozitie], Canvas.GetLeft(global_ellipse) + 5);
                    }

                    if (Canvas.GetTop(global_ellipse) + (x.Y - 15) > 0 && Canvas.GetTop(global_ellipse) + (x.Y + 15) < ecran.ActualHeight)
                    {
                        Canvas.SetTop(global_ellipse, Canvas.GetTop(global_ellipse) + (x.Y - 15));
                        Canvas.SetTop(nr_label[pozitie], Canvas.GetTop(global_ellipse));
                    }
                }
                p = x;
            }
        }
        //<----Functii Mouse---->





        //<----Functii Butoane---->
        private void adauga_Click(object sender, RoutedEventArgs e)
        {
            buton = 0;
            double x, y;
            int inaltime, lungime;
            Ellipse nod;
            Label lbl;
            nod = new Ellipse();
            lbl = new Label();
            lbl.Content = nnr;


            ///adaugarea nodului in vector
            add(nod);
            add_label(lbl);
            nnr++;


            //proprietati nod
            nod.Height = 30;
            nod.Width = 30;
            nod.Stroke = Brushes.LightSkyBlue;
            nod.StrokeThickness = 2;
            nod.Fill = Brushes.SteelBlue;
            nod.Opacity = 0.5;


            //generare nod random
            Random rand = new Random();
            inaltime = (int)ecran.ActualHeight;
            lungime = (int)ecran.ActualWidth;
            x = rand.Next(30, inaltime - 30);
            y = rand.Next(30, lungime - 30);
            //Setare pozitii a nodurilor+labelurilor
            Canvas.SetTop(nod, x);
            Canvas.SetLeft(nod, y);
            Canvas.SetTop(lbl, x);
            Canvas.SetLeft(lbl, y + 5);
            ecran.Children.Add(lbl);
            ecran.Children.Add(nod);
        }

        private void sterge_Click(object sender, RoutedEventArgs e)
        {
            buton = 1;
        }


        private void adauga_muchie_Click(object sender, RoutedEventArgs e)
        {
            buton = 2;
        }
        private void stergere_muchie_Click(object sender, RoutedEventArgs e)
        {
            buton = 3;
        }
        private void start_Click(object sender, RoutedEventArgs e)
        {
            buton = 4;
            for (int i = 1; i < nnr; i++)
            {
                noduri[i].Fill = Brushes.SteelBlue;
            }
            for (int i = 0; i < 500; i++)
            {
                vizitat[i] = 0;
            }
            MessageBox.Show("Alegeti un nod de start apasand pe acesta!");
        }
        //<----Functii butoane---->




        //<----Functii---->


        //adauga structurile in vectori
        private void add_label(Label lbl)
        {
            nr_label[nnr] = lbl;
        }
        private void add(Ellipse nod)
        {
            noduri[nnr] = nod;
        }
        //adauga structurile in vectori



        // trasare muchii
        private void desenl(Point s1, Point s2, Ellipse es, Ellipse ef)
        {
            ArrowLine sageata = new ArrowLine();
            sageata.Stroke = Brushes.FloralWhite;
            sageata.Opacity = 0.4;
            sageata.StrokeThickness = 1.6;
            sageata.ArrowAngle = 60;
            sageata.ArrowLength = 10;
            sageata.X1 = s1.X;
            sageata.Y1 = s1.Y;
            sageata.X2 = s2.X;
            sageata.Y2 = s2.Y;
            ecran.Children.Add(sageata);

            //adaugarea muchiilor si a nodurilor respective intr-o structura pentru o stergere ulterioara mai usoara
            muchii[nnm] = sageata;
            ns[nnm] = es;
            nf[nnm] = ef;
            nnm++;
            //---


        }
        // trasare muchii

        private int muchieidentica(Ellipse es, Ellipse ef)//determinarea daca exista muchii identice
        {
            for (int i = 1; i <= nnm; i++)
            {
                if (ns[i] == es && nf[i] == ef) return 1;
            }
            return 0;
        }

        private int faramuchie(Ellipse global_ellipse)//daca nodul poate fi miscat (daca nu are muchie)
        {
            for (int i = 1; i <= nnm; i++)
            {
                if (ns[i] == global_ellipse || nf[i] == global_ellipse) return 1;
            }
            return 0;
        }

        private void eliminare_muchie(ArrowLine global_sageata) //eliminarea muchiei din vectorul de muchii si din matricea de adiacenta
        {
            int n1 = 0, n2 = 0;
            for (int i = 1; i <= nnm; i++)
            {
                if (muchii[i] == global_sageata)
                {

                    for (int j = 1; j <= nnr; j++)
                    {
                        if (noduri[j] == ns[i]) n1 = j;
                    }

                    for (int j = 1; j <= nnr; j++)
                    {
                        if (noduri[j] == nf[i]) n2 = j;
                    }
                    muchii[i] = null;
                    nf[i] = null;
                    ns[i] = null;
                    matrice[n1, n2] = 0;
                }
            }
        }

        //<----Functii---->



        //<-----BFS----->
        private async void bfs(Ellipse global_ellipse)
        {
            int prim = 0, ultim = 0, nc;
            coada[0] = determinarenod(global_ellipse);
            await Task.Delay(1000);
            while (prim <= ultim)
            {
                nc = coada[prim];
                vizitat[nc] = 1;
                noduri[nc].Fill = Brushes.Green;
                await Task.Delay(1000);

                for (int i = 1; i <= nnr; i++)
                {
                    if (matrice[nc, i] == 1 && vizitat[i] == 0)
                    {
                        ultim++;
                        coada[ultim] = i;

                    }
                }
                prim++;
            }
            ok = 1;
        }



        private int determinarenod(Ellipse global_ellipse)
        {
            for (int i = 1; i <= nnr; i++)
            {
                if (noduri[i] == global_ellipse) return i;
            }
            return 0;
        }


       // Resetare completa a cozii , matricii , structurii si a ecranului
        private void Resetare_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= nnr; i++)
            {
                if (noduri[i] != null) ecran.Children.Remove(noduri[i]);
                if (nr_label[i] != null) ecran.Children.Remove(nr_label[i]);
                vizitat[i] = 0;
                noduri[i] = null;
                nr_label[i] = null;
            }

            for (int i = 0; i <25000; i++)
            {
                coada[i] = 0;
            }

            for (int i = 1; i <= nnm;i++ )
            {
                if(muchii[i]!=null)
                {
                    ecran.Children.Remove(muchii[i]);
                    ns[i] = null;
                    nf[i] = null;
                    muchii[i] = null;
                }
            }

                for (int i = 1; i <= nnr; i++)
                {
                    for (int j = 1; j <= nnr; j++)
                    {
                        matrice[i, j] = 0;
                    }
                }




            nnr = 1;
            nnm = 1;
        }

        //------------>

    }
 
}