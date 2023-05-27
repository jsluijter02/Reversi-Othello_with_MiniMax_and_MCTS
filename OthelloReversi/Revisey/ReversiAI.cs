using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OthelloReversi
{
    public class ReversiAI
    {
        private Spel game;
        private int minimaxdiepte = 6;
        private int MCTSiteraties = 20000; 

        public ReversiAI(Spel game)
        {
            this.game = game;
        }

        //deze functie wordt aangeroepen door het spel, om een move te genereren voor de AI
        //je kan uit meerdere mogelijkheden kiezen, zoals minimax, montecarlo en random legal move
        public Move AI(int[,] board, string ai, int turn)
        {

            if (ai == "MiniMax")
            {
                Move output = MiniMax(board, minimaxdiepte, turn);
                if (output.x != -1 && output.y != -1)
                {
                    return output;
                }
            }

            else if (ai == "MCTS")
            {
                return MCTS(board, turn);
            }

            return RandomLegaleMove(board, turn);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //Minimax met alpha beta pruning algoritme, beschreven in Artificial Intelligence a Modern Approach//
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        private Move MiniMax(int[,] board, int diepte, int turn)
        {

            (int value, Move move) = Max(board, diepte, turn, int.MinValue, int.MaxValue);

            return move;

        }

        //Min en Max returnen de beste move en bijbehorende score, ze gaan afwisselend recursief dieper
        private (int, Move) Max(int[,] board, int diepte, int turn, int alpha, int beta) 
        {

            if(game.gameover(turn,board) || diepte == 0) 
            { 
                return (utiliteit(board), new Move(-1, -1)); 
            }

            int v = int.MinValue;
            Move a = new Move(-1, -1);

            List<Move> actions = Actions(board, turn);

            foreach(Move action in actions)
            {
                //bereken de nieuwe staat
                int[,] newboard = kopie(board);
                newboard = berekennieuwestaat(newboard, turn, action);

                (int v2, Move a2) = Min(newboard, diepte - 1, turn * -1, alpha, beta);

                if(v2 > v) //als v2 groter is dan v, dan hebben we dus een betere move gevonden
                {
                    v = v2;
                    a = action;
                    alpha = Math.Max(alpha, v);
                }
                
                if (v >= beta) //dan moeten we prunen
                {
                    return(v, a);
                }
            }

            return (v, a);
        }

		private (int, Move) Min(int[,] board, int diepte, int turn, int alpha, int beta) 
        {

            if(game.gameover(turn, board) || diepte == 0)
            {
                return (utiliteit(board), new Move(-1, -1));
            }

            int v = int.MaxValue;
            Move a = new Move(-1, -1);

            List<Move> actions = Actions(board, turn);

            foreach (Move action in actions)
            {
                int[,] newboard = kopie(board);
                newboard = berekennieuwestaat(newboard, turn, action);

                (int v2, Move a2) = Max(newboard, diepte - 1, turn * -1, alpha, beta);

                if (v2 > v)
                {
                    v = v2;
                    a = action;
                    beta = Math.Min(beta, v);
                }

                if(v <= alpha)
                {
                    return (v, a);
                }

            }

            return (v, a);  

            }
        

        //Actions vindt alle mogelijke Moves op het bord voor een bepaalde beurt en maakt er een lijst van
        private List<Move> Actions(int[,] board, int turn)
        {
            List<Move> actions = new List<Move>();

            for (int i = 0; i < game.vakjes; i++)
            {
                for (int j = 0; j < game.vakjes; j++)
                {
                    if (game.zetlegaliteit(i, j, board, turn)) //checkt of de voorgestelde zet legaal is
                    {

                        Move move = new Move(i, j);
                        actions.Add(move); //voegt hem toe aan de mogelijke zetten

                    }
                }
            }

            return actions;
        }


        //berekent hoe goed een bepaald bord is voor de AI
        private int utiliteit(int[,] board)
        {
            int witte = 0;
            int zwarte = 0;
            int waarde = 1; //standaard waarde van een steen is 1
            int mogelijkezettenwit = aantalzetten(board, -1);
            int mogelijkezettenzwart = aantalzetten(board, 1); //relatieve mobiliteit, dus ook kijken of de tegenstander benadeeld wordt

            //de hoogste score voor zwart/wit is degene met het hoogste verschil in waarde
            for (int i = 0; i < game.vakjes; i++)
            {
                for (int j = 0; j < game.vakjes; j++)
                {
                    //bereken hoeveel het vakje wat we checken waard is
                    if(hoek(i, j, game.vakjes))
                    {
                        waarde = 5;
                    }

                    else if(rand(i, j, game.vakjes))
                    {
                        waarde = 3;
                    }

                    //tel het aantal op voor de zwarte of witte stenen
                    if (board[i, j] == 1) 
                    { 
                        zwarte += waarde; 
                    }

                    else if (board[i, j] == -1) 
                    { 
                        witte += waarde;
                    }

                }
            }

            return witte - zwarte + mogelijkezettenwit - mogelijkezettenzwart; //de computer is wit, dus we spelen max voor wit
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // HEURISTIEKEN VOOR MINIMAX, zodat deze een iets betere tegenstander wordt              //
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        //heuristiek om een hoek stukje te proberen te veroveren
        private bool hoek(int x, int y, int vakjes)
        {
            if ((x == 0 && y == 0) || 
                (x == 0 && y == vakjes - 1) ||  
                (x == vakjes - 1 && y == 0) ||  
                (x == vakjes - 1 && y == vakjes - 1))
            {
                return true;
            }

            return false;
        }

        //aan de rand is meer waard, want daar kan je de steen moeilijk veroveren
        private bool rand(int x, int y, int vakjes)
        {
            //hoek is meer waard dan rand 
            if(hoek(x, y, vakjes))
            {
                return false;
            }

            if(x == 0 || y == 0 || x == vakjes - 1 || y == vakjes - 1)
            {
                return true;
            }

            return false;
        }

        //heuristiek voor mobiliteit, het hoogst aantal zetten dat de AI mag maken is beter
        private int aantalzetten(int[,] bord, int beurt)
        {
            int aantal = 0;

            for (int i = 0; i < game.vakjes; i++)
            {
                for (int j = 0; j < game.vakjes; j++)
                {
                    if (game.zetlegaliteit(i, j, bord, beurt)) //checkt of de voorgestelde zet legaal is
                    {
                        aantal++;
                    }
                }
            }

            return aantal;
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        ///// Random Legale Move - een AI modus op zichzelf maar wordt ook in MCTS gebruikt ///
        ///////////////////////////////////////////////////////////////////////////////////////
        
        private Move RandomLegaleMove(int[,] board, int turn) 
        {

            List<Move> moves = Actions(board, turn);
            int aantalmoves = moves.Count;

            if(aantalmoves == 0)
            {
                return new Move(-1, -1);
            }

            Random rand = new Random();

            int index = rand.Next(aantalmoves);

            return moves[index]; 
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        ////// Monte Carlo Tree Search, ook volgens Artificial intelligence a modern approach ///////
        /////////////////////////////////////////////////////////////////////////////////////////////
        
        //MCTS instantieert een loop voor het algoritme en roept telkens de selectie procedure aan, tot de iteratie tijd op is en een move uiteindelijk zal teruggeven
        private Move MCTS(int[,] board, int turn)
        {
            //initializeer MCTS root 
            Node s0 = new Node(board, null, 0, 0, turn, new List<Node>(), null);

            //aantal MCTS iteraties
            for(int i = 0; i < MCTSiteraties; i++)
            {
                Node select = selectie(s0);
                select.kids = expandeer(select);
                int beloning = simuleer(select);
                backprop(select, beloning);
            }

            //hierna runnen we nog 1 selectie onder de kinderen van s0, om te kijken welke actie we moeten nemen:
            return hoogsteUCB(s0).actie;
        }

        private Node selectie(Node node)
        {

            //gaat door totdat hij een leaf vindt
            while(node.visits != 0 && node.kids.Count > 0)
            {
                //terwijl hij nog geen leaf heeft gevonden, kiest hij telkens de volgende node met de hoogste UCB waarde
                node = hoogsteUCB(node);
            }

            //als het een leaf is, dan wordt deze gereturnd
            return node;   
        }

        //simuleer neemt een node en past telkens een random move toe op de staat, tot het spel afgelopen is
        private int simuleer(Node node)
        {
            int[,] bord = node.bord;
            int beurt = node.beurt;
            int[,] newboard = kopie(bord);

            //speel het spel tot het einde, met random moves voor beide spelers
            while (!game.gameover(beurt, newboard))
            {
                //bereken de nieuwe staat
                Move action = RandomLegaleMove(newboard, beurt);
                if (action.x >= 0 && action.y >= 0)
                {
                    newboard = berekennieuwestaat(newboard, beurt, action);
                }

                beurt *= -1;
            }


            //bereken de winnaar
            
            int wit = 0;
            int zwart = 0;
            for(int i = 0; i < game.vakjes; i++)
            {
                for(int j = 0; j < game.vakjes; j++)
                {
                    if (newboard[i,j] == 1)
                    {
                        zwart++;
                    }
                    else if(newboard[i,j] == -1)
                    {
                        wit++;
                    }
                }
            }

            if(zwart > wit)
            {
                return 1;
            }
            else if (wit > zwart)
            {
                return -1;
            }
            
            return 0;
        }

        private List<Node> expandeer(Node node)
        {
            int[,] bord = node.bord;
            int beurt = node.beurt;

            List<Node> newnodes = new List<Node>();

            //genereer een lijst van alle moves
            List<Move> kinderen = Actions(bord, beurt);

            foreach(Move action in kinderen)
            {
                //bereken de nieuwe staat
                int[,] newboard = kopie(bord);
                newboard = berekennieuwestaat(newboard, beurt, action);

                int newbeurt = beurt * -1;

                Node newnode = new Node(newboard, action, 0, 0, newbeurt, new List<Node>(), node);

                newnodes.Add(newnode);
            }

            return newnodes;
        }

        private void backprop(Node node, int beloning)
        {
            //backprop tot de root
            while(node != null)
            {

                node.visits += 1;

                if(node.beurt == beloning)
                {
                    node.wins += 1;
                }

                node = node.parent;
            }

        }
        
        private double UCB(Node parent, Node child)
        {
            double C = 1;
            double N = parent.visits;
            double ni = child.visits;
            double vi = child.wins / ni;

            return vi + C * Math.Sqrt(Math.Log(N / ni + 0.000000001));
        }

        //berekent welk van de kinderen de hoogste UCB score heeft
        private Node hoogsteUCB(Node ouder)
        {
            double max = double.MinValue;
            Node geselecteerd = ouder.kids[0];

            foreach (Node kid in ouder.kids)
            {
                double newmax = UCB(ouder, kid);

                if (newmax > max)
                {
                    max = newmax;
                    geselecteerd = kid;
                }

            }

            return geselecteerd;
        }

        //functie die een extensieve kopie maakt van een array,
        //zodat het originele bord onveranderd blijft,
        //dit zorgde voor veel raar gedrag van het spel, omdat een nieuwe instantie het originele bord aanpaste
        private int[,] kopie(int[,] og)
        {

            int[,] kopie = new int[game.vakjes, game.vakjes];

            for (int i = 0; i < game.vakjes; i++)
            {

                for (int j = 0; j < game.vakjes; j++)
                {
                    kopie[i, j] = og[i, j];
                }

            }

            return kopie;
        }
        private int[,] berekennieuwestaat(int[,] newboard, int beurt, Move action)
        {
            //bereken de nieuwe staat
            newboard[action.x, action.y] = beurt;
            newboard = game.flipper(action.x, action.y, newboard, beurt);
            return newboard;
        }
    }
    

    //Data structure to represent different nodes of the tree
    public class Node
    {
        public int beurt;
        public int[,] bord;
        public Move actie;
        public List<Node> kids;
        public Node parent; 

        public double visits;
        public double wins;
        public Node(int[,] board, Move action, double visits, double wins, int beurt, List<Node> kids, Node parent)
        {
            this.beurt = beurt;
            this.bord = board;
            this.actie = action;
            this.visits = visits;
            this.wins = wins;
            this.kids = kids;
            this.parent = parent;
        }
    }
}


