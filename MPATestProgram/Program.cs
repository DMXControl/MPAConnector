using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAConnector.Elements;

namespace MPAConnector
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWork().Wait();
        }

        private static async Task DoWork()
        {
            using (var x = new MPAGlueConnector())
            {
                x.ChainAdded += ChainAdded;

                await x.connect();

                Console.ReadLine();

                x.Disconnect();
            }
        }

        private static void ChainAdded(object sender, MPAChainEventArgs e)
        {
            var buttons = e.Chain.SelectMany(c => c).OfType<IMPARgbButton>();

            foreach (var mpaRgbButton in buttons)
            {
                mpaRgbButton.PressedChanged += MpaRgbButton_PressedChanged;
            }

            var encoder = e.Chain.SelectMany(c => c).OfType<IMPAEncoder>().Take(6).ToList();
            var fader = e.Chain.SelectMany(c => c).OfType<IMPAMotorfader>().ToList();

            for (int i = 0; i < 4; i++)
            {
                var enc = encoder[i];
                var f = fader[i];

                const int step = 2000;
                enc.TurnedLeft += (s, a) =>
                {
                    if (f.Value > step)
                        f.Value -= step;
                    else
                        f.Value = 0;
                };
                enc.TurnedRight += (s, a) =>
                {
                    if (f.Value < 0xFFFF - step)
                        f.Value += step;
                    else
                        f.Value = 0xFFFF;
                };
                enc.PressedChanged += (s, a) =>
                {
                    if (enc.Pressed)
                    {
                        if (f.Value == 0) f.Value = 0xFFFF;
                        else f.Value = 0;
                    }
                };
            }

            bool wave = true;

            var reallylast = encoder.Last();
            reallylast.PressedChanged += (s, a) =>
            {
                if (!reallylast.Pressed)
                    wave = !wave;
            };

            var last = encoder.ElementAt(4);
            var flag = false;
            last.PressedChanged += async (s, a) =>
            {
                if (last.Pressed) return;
                if (flag)
                {
                    flag = false;
                    return;
                }

                flag = true;
                while (flag)
                {
                    for (double winkel = 0; winkel < 360 && flag; winkel += 5)
                    {
                        foreach (var mpaMotorfader in fader)
                        {
                            if (!flag) break;

                            var w = (winkel / 180 * Math.PI);
                            if (wave)
                                w += (Math.PI / 2) * mpaMotorfader.Index;

                            var x = Math.Sin(w);

                            mpaMotorfader.Value = (ushort)((x + 1) * ushort.MaxValue * 0.5);
                            await Task.Delay(3);
                        }
                    }
                }
            };
        }

        private static async void MpaRgbButton_PressedChanged(object sender, EventArgs e)
        {
            IMPARgbButton b = sender as IMPARgbButton;
            if (b == null) return;

            if (b.Index < 7)
            {
                b.ButtonColor = b.Pressed ? Color.Blue : Color.Red;
            }
            else
            {
                if (b.Pressed) return;

                List<Color> colors = new List<Color>()
                {
                    Color.Blue, Color.GreenYellow, Color.Aqua, Color.Firebrick,
                    Color.DarkOrange, Color.Fuchsia, Color.DodgerBlue, Color.Yellow
                };

                var buttons = b.Parent.OfType<IMPARgbButton>();
                for (int round = 0; round < 100; round++)
                {
                    foreach (var mpaRgbButton in buttons)
                    {
                        mpaRgbButton.ButtonColor = colors[(mpaRgbButton.Index + round) % colors.Count];
                    }

                    await Task.Delay(50);
                }
            }
        }
    }
}
