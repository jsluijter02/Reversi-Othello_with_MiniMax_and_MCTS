namespace OthelloReversi
{
    public class Spel
    {    
        public int[,] bord;
        public int beurt;
        public int vakjes;

        public int wit;
        public int zwart;

        public Spel()
        {
            vakjes = 6; 
            bord = new int[vakjes,vakjes];
        }

        //zet doen doet een zet in het spel, hij checkt of deze zet legaal is, en zet hem dan op het bord,
        //ook checkt hij of de tegenstander een zet kan doen, want in reversi blijft de beurt aan de huidige speler,
        //als de tegenstander geen zetten heeft
        public void zetdoen(int x, int y)
        {

            bool legalezet = zetlegaliteit(x,y,bord,beurt);

            if (legalezet)
            {
                bord[x, y] = beurt;
                bord = flipper(x, y, bord, beurt);
                if(heeftlegalezet(beurt * -1, bord))
                {
                    beurt *= -1;
                }
            }
        }


        //checkt voor een bepaalde zet of deze in een bepaalde richting legaal is. 
        //als hij een richting vindt waarin het legaal is om de zet te doen, dan returnt hij true
        public bool zetlegaliteit(int x, int y, int[,] board, int turn)
        {
            if (x < 0 || x >= vakjes || y < 0 || y >= vakjes) //als het buiten het bord is
            {
                return false;
            }

            if (board[x, y] != 0) { return false; } //als er een steen ligt is het sws fout

            foreach (var richting in richtingen)
            {

                if (richtingchecker(x, y, richting[0], richting[1], board, turn))//check alle richtingen of er iets geplaatst kan worden
                {
                    return true;
                }

            }

            return false;
        }


        //richtingchecker gaat 1 richting af en kijkt of deze zet legaal zou zijn in die richting
        public bool richtingchecker(int x, int y, int dx, int dy, int[,] board, int turn)
        {
            x += dx; //voeg de richting toe aan x en y
            y += dy;

            //als het buiten het bord is
            if (x < 0 || x >= vakjes || y < 0 || y >= vakjes) 
            {
                return false;
            }

            //als er geen steen ligt is het false, of als de steen direct ernaast van de speler is
            if (board[x, y] == 0 || board[x, y] == turn) 
            {
                return false;
            }

            bool enemy = false; //we willen in de richting een steen van de tegenstander vinden

            while (x >= 0 && x < vakjes && y >= 0 && y < vakjes && board[x, y] != 0) //terwijl we niet van het bord afgaan en het vakje is niet leeg
            {
                if (board[x, y] == turn * -1) //als het de tegenstander is, dan gaat enemy naar true
                {
                    enemy = true;
                }

                else if (enemy && board[x, y] == turn) //we zijn dus een sequentie van tegenstanders tegengekomen en nu vinden we onze eigen steen, return true
                {
                    return true;
                }

                x += dx;
                y += dy;
            }

            return false;
        }

        //flipper flipt alle stenen die er geflipt moeten worden nadat de zet is gedaan
        public int[,] flipper(int x, int y, int[,] board, int turn)
        {

            //alle richtingen checken of ze geflipt moeten worden
            foreach (var richting in richtingen)
            {

                bool check = richtingchecker(x, y, richting[0], richting[1], board, turn); //check even of we deze richting kunnen flippen
                
                if (check) //zo ja,
                {

                    int a = x + richting[0];
                    int b = y + richting[1];

                    while (board[a, b] != turn) //flip dan zolang we niet een steen van onszelf tegenkomen 
                    {

                        board[a, b] = turn;
                        a += richting[0];
                        b += richting[1];

                    }

                }

            }

            return board;
        }

        private int[][] richtingen = new[]{
        new[]{-1, -1},
        new[]{ -1, 0 },
        new[]{ -1, 1 },
        new[]{ 0, -1},
        new[]{  0, 1 },
        new[]{ 1, -1},
        new[]{  1, 0 },
        new[]{  1, 1 }
        };

        //gameover checkt of beide beurten nog wel legale zetten in hun bezit hebben
        public bool gameover(int turn, int[,] board)
        {

            if (!heeftlegalezet(turn, board)) 
            { 

                if (!heeftlegalezet(turn * - 1, board)) 
                { 
                    return true; 
                } 
            
            }

            return false;
        }

        //checkt of een bepaalde speler een zet op het bord heeft liggen
        public bool heeftlegalezet(int turn, int[,] board) 
        {
            for (int i = 0; i < vakjes; i++)
            {
                for (int j = 0; j < vakjes; j++)
                {

                    if (board[i, j] == 0)
                    {

                        if (zetlegaliteit(i, j, board, turn))
                        {
                            return true;
                        }

                    }
                }
            }

            return false;
        }

        //tel stenen telt het aantal stenen voor zwart en wit, dit wordt aan het einde van het spel gedaan, om de winnaar te bepalen
        public void telstenen()
        {
            wit = 0;
            zwart = 0;

            for(int i = 0;i < vakjes; i++)
            {
                for(int j = 0; j < vakjes; j++)
                {
                    if(bord[i, j] == 1) { zwart++; }
                    else if(bord[i, j] == -1) { wit++; }
                }
            }
        }
    }
}