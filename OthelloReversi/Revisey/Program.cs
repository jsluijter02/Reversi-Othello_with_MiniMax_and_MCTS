using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace OthelloReversi
{ 
    public class Reversi : Form
    {
        //membervariabelen en controls
        private bool helper = false;
        private DoubleBufferedPanel panel = new DoubleBufferedPanel();
        private Button nieuwspel, help;
        private Label stopwatchlabel, aantalwit, aantalzwart;
        private Stopwatch stopwatch = new Stopwatch();
        private bool computer; //geeft aan of de speler wel of niet tegen de computer speelt

        private AIform AIchoice = new AIform();
        private string AIModes = ""; //als je op het kruisje klikt, dan gaat hij alsnog tegen random move spelen

        private Spel game;
        private ReversiAI gameAI;

        public Reversi()
        {
            game = new Spel();
            gameAI = new ReversiAI(game);
            InitializeComponent();
        }

        //de methode klik handelt de game loop af, als de speler klikt, wordt er gekeken of deze zet kan, en er wordt ook een zet voor de computer gedaan
        private async void klik(object obj, MouseEventArgs m)
        {
            panel.MouseClick -= klik; //als er geklikt wordt gaan we geen nieuwe klikevents aannemen

            int x = m.X;
            int y = m.Y;
            int a = x / (panel.Width / game.vakjes); //deze berekening zet de klikcoordinaten om in bordcoordinaten
            int b = y / (panel.Height / game.vakjes);

            //indien de geklikte positie dus legaal is,
            if (game.zetlegaliteit(a,b, game.bord, game.beurt)) 
            {

                //de speler mag een zet doen
                if (!computer || game.beurt == 1)
                {
                    game.zetdoen(a, b);
                }

                panel.Invalidate();
                spelafgelopen();

                //nu laten we de computer een zet doen, indien hij dus kon
                if (computer && game.beurt == -1)
                {

                    //we wachten even met de AI zet doen, zodat de speler zich mentaal kan voorbereiden
                    await Task.Delay(750);
                    Move AIMove = gameAI.AI(game.bord, AIModes, -1);

                    //we doen de zet, checkt ook of de zet wel legaal is
                    game.zetdoen(AIMove.x, AIMove.y);

                    panel.Invalidate();
                    spelafgelopen();

                }
            }

            panel.MouseClick += klik;
        }

        //kijkt of het spel afgelopen is en de msgbox met de winnaar getoond moet worden
        private void spelafgelopen()
        {
            if (game.gameover(game.beurt, game.bord))
            {
                game.telstenen();
                winner();
            }
        }

        //als er een winnaar is na spel afgelopen, wordt deze geshowd in een message box
        private void winner()
        {
            if (game.wit > game.zwart) { stopwatch.Stop(); MessageBox.Show("Wit heeft gewonnen!"); }
            else if (game.wit < game.zwart) { stopwatch.Stop(); MessageBox.Show("Zwart heeft gewonnen!"); }
            else if (game.wit == game.zwart) { stopwatch.Stop(); MessageBox.Show("Het is gelijkspel..."); }
        }

        //een nieuw spel wordt gestart
        private void Beginspel(object obj, EventArgs e)
        {

            if(game.vakjes >= 3)
            {

                //maak het bord leeg
                for (int i = 0; i < game.vakjes; i++)
                {
                    for (int j = 0; j < game.vakjes; j++)
                    {
                        game.bord[i, j] = 0; 
                    }
                }

                //vraagt of je tegen de pc wil spelen
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Wil je tegen de Computer spelen?", "CPU", buttons);

                //zo ja, dan vragen we tegen welke modus je wilt spelen
                if (result == DialogResult.Yes) 
                { 
                    computer = true; 
                    AIchoice.ShowDialog();
                    AIModes = AIchoice.SelectedAIMode;
                }

                else { computer = false; }

                //setup
                game.bord[game.vakjes / 2 - 1, game.vakjes / 2 - 1] = -1;
                game.bord[game.vakjes / 2 - 1, game.vakjes / 2] = 1;
                game.bord[game.vakjes / 2, game.vakjes / 2 - 1] = 1;
                game.bord[game.vakjes / 2, game.vakjes / 2] = -1;

                //zwart begint
                game.beurt = 1;

                //zet de stopwatch aan
                stopwatch.Reset();
                stopwatch.Start();
                panel.Invalidate();
            }

            else 
            { 
                MessageBox.Show("Bord moet minimaal 3x3 zijn."); 
            }

        }

        //zet de hulp aan, zodat je de legale zetten ziet
        private void klikhelper(object obj, EventArgs e)
        {
            if (helper == false) { helper = true; }
            else { helper = false; }
            panel.Invalidate();
        }

        //tekent de stenen, maar handelt zet tegelijk ook het aantal stenen op het form
        private void tekensteen(object obj, PaintEventArgs pea) 
        {
            int zwart = 0;
            int wit = 0;
            for (int i = 0; i < game.vakjes; i++)
            {

                for (int j = 0; j < game.vakjes; j++)
                {

                    int tekenx = i * (panel.Width / game.vakjes); //tekenx en tekeny bepalen in welk vakje een steen getekend moeten worden
                    int tekeny = j * (panel.Height / game.vakjes);

                    if (game.bord[i, j] == 1)
                    {
                        pea.Graphics.FillEllipse(Brushes.Black, tekenx, tekeny, panel.Width / game.vakjes, panel.Height / game.vakjes);
                        zwart++;
                    }

                    else if (game.bord[i, j] == -1)
                    {
                        pea.Graphics.FillEllipse(Brushes.White, tekenx, tekeny, panel.Width / game.vakjes, panel.Height / game.vakjes);
                        wit++;
                    }

                    else if (helper == true && game.zetlegaliteit(i, j, game.bord, game.beurt) == true) //voor de hulp functie
                    {
                        if (!computer)
                        {
                            pea.Graphics.FillEllipse(Brushes.DarkGreen, tekenx, tekeny, panel.Width / game.vakjes, panel.Height / game.vakjes);
                        }
                        else if (game.beurt == 1)
                        {
                            pea.Graphics.FillEllipse(Brushes.DarkGreen, tekenx, tekeny, panel.Width / game.vakjes, panel.Height / game.vakjes);
                        }
                    }

                }
            }

            aantalzwart.Text = "Zwart heeft " + zwart + " stenen.";
            aantalwit.Text = "Wit heeft " + wit + " stenen.";
        }

        //deze wordt aangeroepen in de constructor en handelt de opbouw van het form af.
        private void InitializeComponent()  
        {
            //form
            this.Text = "Reversi";
            this.BackColor = Color.White;
            this.ClientSize = new Size(760, 450);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Paint += tekenGUIstenen;
            //panel
            panel.Location = new Point(27, 23);
            panel.Size = new Size(400, 400);
            panel.BackColor = Color.Green;
            panel.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(panel);
            panel.Paint += this.tekenbord;
            panel.Paint += this.tekensteen;
            panel.MouseClick += this.klik;
            //nieuwspel button
            nieuwspel = new Button();
            nieuwspel.Location = new Point(507, 23);
            nieuwspel.Text = "Nieuw Spel";
            nieuwspel.Size = new Size(219, 40);
            nieuwspel.Font = new Font("Candara", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(nieuwspel);
            nieuwspel.Click += Beginspel;
            //help button
            help = new Button();
            help.Location = new Point(507, 69);
            help.Size = new Size(219, 40);
            help.Text = "Help";
            help.Font = new Font("Candara", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(help);
            help.Click += klikhelper;
            //timer
            stopwatchlabel = new Label();
            stopwatchlabel.Location = new Point(500, 385);
            stopwatchlabel.Font = new System.Drawing.Font("Calibri", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            stopwatchlabel.Size = new Size(160, 50);
            this.Controls.Add(stopwatchlabel);
            Timer tijd = new Timer();
            tijd.Enabled = true;
            tijd.Interval = 200;
            tijd.Tick += timertick;
            //time label
            Label time = new Label();
            time.Size = new Size(35, 15);
            time.Location = new Point(658, 408);
            time.Text = "Time";
            time.Font = new Font("Candara", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Controls.Add(time);
            //REVERSI LOGO LABEL
            Label reversie = new Label();
            reversie.Size = new Size(194, 49);
            reversie.Location = new Point(519, 123);
            reversie.Font = new System.Drawing.Font("Candara", 30F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            reversie.Text = "REVERSI↔";
            this.Controls.Add(reversie);
            //aantalzwart label
            aantalzwart = new Label();
            aantalzwart.Location = new Point(599, 214);
            aantalzwart.Size = new Size(200, 20);
            aantalzwart.Font = new Font("Candara", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            aantalzwart.Text = "Zwart heeft 0 stenen.";
            this.Controls.Add(aantalzwart);
            //aantalwit label
            aantalwit = new Label();
            aantalwit.Location = new Point(599, 295);
            aantalwit.Size = new Size(200, 20);
            aantalwit.Font = new Font("Candara", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            aantalwit.Text = "Wit heeft 0 stenen.";
            this.Controls.Add(aantalwit);
        }
        
        
        //tekent het aantal vakjes op het bord 
        private void tekenbord(object obj, PaintEventArgs pea) 
        {

            Pen zwart = new Pen(Color.Black);

            for (int i = 1; i <= game.vakjes; i++)
            {
                int lijnx = panel.Width / game.vakjes * i;
                pea.Graphics.DrawLine(zwart, lijnx, 0, lijnx, panel.Height);
            }

            for (int i = 1; i <= game.vakjes; i++)
            {
                int lijny = panel.Height / game.vakjes * i;
                pea.Graphics.DrawLine(zwart, 0, lijny, panel.Width, lijny);
            }

        }
        
        //deze methode tekent de twee stenen in het UI van het form, dus niet op het bord
        private void tekenGUIstenen(object obj, PaintEventArgs pea)
        {
            Pen lightgray = new Pen(Color.LightGray, 2);
            Pen darkgray = new Pen(Color.DarkGray, 2);
            pea.Graphics.FillEllipse(Brushes.Black, 511, 184, 75, 75);
            pea.Graphics.DrawEllipse(darkgray, 511, 184, 75, 75);
            pea.Graphics.FillEllipse(Brushes.White, 511, 265, 75, 75);
            pea.Graphics.DrawEllipse(lightgray, 511, 265, 75, 75);
        }

        //deze methode format de timer, zodat het label de timer weergeeft
        private void timertick(object obj, EventArgs e) 
        {
            this.stopwatchlabel.Text = String.Format("{0:hh\\:mm\\:ss}", stopwatch.Elapsed);
        }
    }

    //Deze start het form op 
    internal static class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Reversi());
        }
    }
}
public class DoubleBufferedPanel : Panel { public DoubleBufferedPanel() : base() { DoubleBuffered = true; } } //maakt de code smooth: https://stackoverflow.com/questions/818415/how-do-i-double-buffer-a-panel
