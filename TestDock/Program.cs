using System;
using System.Drawing;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        var form = new Form { Size = new Size(300, 400) };
        var pnl = new Panel { Dock = DockStyle.Fill };
        
        var btn1 = new Button { Text = "Button 1", Dock = DockStyle.Top, Height = 50 };
        var btn2 = new Button { Text = "Button 2", Dock = DockStyle.Top, Height = 50 };
        var btn3 = new Button { Text = "Button 3", Dock = DockStyle.Top, Height = 50 };
        
        pnl.Controls.Add(btn1);
        pnl.Controls.Add(btn2);
        pnl.Controls.Add(btn3);
        
        form.Controls.Add(pnl);
        
        // Let's print the Y coordinates to see which is at the top
        form.Load += (s, e) => {
            Console.WriteLine($"btn1.Y = {btn1.Location.Y}");
            Console.WriteLine($"btn2.Y = {btn2.Location.Y}");
            Console.WriteLine($"btn3.Y = {btn3.Location.Y}");
            Application.Exit();
        };
        Application.Run(form);
    }
}
