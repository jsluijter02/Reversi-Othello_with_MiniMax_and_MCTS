using System;
using System.Windows.Forms;
using System.Drawing;

public class AIform : Form
{
    public Button minimaxButton;
    public Button mctsButton;
    public Button randomButton;

    public Label aiModeLabel;
    public string SelectedAIMode { get; private set; }

    public AIform()
    {
        this.Width = 400;
        this.Height = 150;
        this.Text = "CPU";
        this.FormBorderStyle = FormBorderStyle.FixedSingle;

        minimaxButton = new Button();
        mctsButton = new Button();
        randomButton = new Button();

        aiModeLabel = new Label();

        minimaxButton.Text = "MiniMax";
        mctsButton.Text = "MCTS";
        randomButton.Text = "Random Move";

        aiModeLabel.Text = "Kies alstublieft een AI modus:";
        aiModeLabel.Location = new Point(30, 20);
        aiModeLabel.Size = new Size(200,30);

        minimaxButton.Location = new Point(30, 50);
        mctsButton.Location = new Point(140, 50);
        randomButton.Location = new Point(250, 50);

        this.Controls.Add(minimaxButton);
        this.Controls.Add(mctsButton);
        this.Controls.Add(randomButton);
        this.Controls.Add(aiModeLabel);

        minimaxButton.Click += MinimaxButton;
        mctsButton.Click += MctsButton;
        randomButton.Click += RandomButton;
    }

    private void MinimaxButton(object sender, EventArgs e)
    {
        SelectedAIMode = "Minimax";
        this.Close();
    }

    private void MctsButton(object sender, EventArgs e)
    {
        SelectedAIMode = "MCTS";
        this.Close();
    }

    private void RandomButton(object sender, EventArgs e)
    {
        SelectedAIMode = "Random Move";
        this.Close();
    }
}