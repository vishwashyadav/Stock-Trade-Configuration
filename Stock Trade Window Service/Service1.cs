using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Window_Service
{
    public partial class Service1 : ServiceBase
    {
        int count = 0;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartStopTimer(true);
        }

        protected override void OnStop()
        {
            StartStopTimer(false);
        }

        private void StartStopTimer(bool start)
        {
            if(start)
            {
                timer1.Interval = 5000;
                timer1.Tick += Timer1_Tick;
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("test.txt"))
            {
                writer.Write(count++.ToString());
            }
        }
    }
}
