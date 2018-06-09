using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CBTC
{
    public partial class AssessForm : Form
    {
        public AssessForm()
        {
            InitializeComponent();
            InitRadar();
        }                                               //两个构造函数给什么就传什么
        string[] items = { "舒适度", "能耗", "晚点" };
        double[] ATOscores = { 100, 100, 100 };
        double[] Manualscores = { 50, 30, 20 };
        double interval = 0;
        List<double> ATOEList;
        List<double> ATOdaList;
        List<double> ManualEList;
        List<double> ManualdaList;
        double ATOt, delayTime, ATOE;
        string ATOsPath = Application.StartupPath + @"\ATOs";


        public AssessForm(ATP atp, double interval, double ATOt, double delayTime, double ATOE) //传进来的形参，把ATP里面的变量传进来
        {
            
            InitializeComponent();
            this.interval = interval;   //定义一个初值
            this.ATOEList = atp.ATOEnergyList;    
            this.ATOdaList = atp.ATODaList;         //ATO自动
            this.ManualEList = atp.EnergyList;  //手动
            this.ManualdaList = atp.DaList;
            this.ATOt = ATOt;
            this.ATOE = ATOE;
            this.delayTime = delayTime;
            InitEnergyChart(ATOEList, ManualEList);
            InitdaChart(ATOdaList, ManualdaList);
            InitRadar();
        }
        void InitRadar()
        {
            CalculateRadarScores();
            radarChart.Series[0]["CircularLabelsStyle"] = "Horizontal";
            //radarChart.Series[0].Color = Color.FromArgb(128, Color.Blue);
            radarChart.Series[0].Points.DataBindXY(items, ATOscores);
            //radarChart.Series[0].Color = Color.FromArgb(128, Color.Yellow);
            radarChart.Series[1].Points.DataBindXY(items, Manualscores);
        }
        void InitEnergyChart(List<double> ATOEList, List<double> ManualEList)
        {
            for (int i = 0; i < ATOEList.Count; i++)
            {
                EnergyChart.Series[0].Points.AddXY(i * interval, ATOEList[i]);
            }
            for (int j = 0; j < ManualEList.Count; j++)
            {
                EnergyChart.Series[1].Points.AddXY(interval * j, ManualEList[j]);
            }
        }

        private void AssessForm_Load(object sender, EventArgs e)
        {

        }

        void InitdaChart(List<double> ATOdaList, List<double> ManualdaList)
        {
            for (int i = 0; i < ATOdaList.Count; i++)
            {
                DaChart.Series[0].Points.AddXY(i * interval, ATOdaList[i]);
            }
            for (int j = 0; j < ManualdaList.Count; j++)
            {
                DaChart.Series[1].Points.AddXY(j * interval, ManualdaList[j]);
            }
        }

        void CalculateRadarScores()
        {
            double sumATOda = 0, sumManda = 0;
            for (int i = 0; i < ATOdaList.Count; i++)
            {
                sumATOda += ATOdaList[i];
            }
            for (int j = 0; j < ManualdaList.Count; j++)
            {
                sumManda += ManualdaList[j];
            }
          
            if (ManualEList.Count != 0)
            {
                Manualscores[0] = (int)(2 - sumManda / ManualEList.Count / (sumATOda / ATOdaList.Count)) * 100;
                //Manualscores[1] =(int) 100 * (2 - ManualEList.Last() / ATOEList.Last());
                Manualscores[1] = (int)100 * (2 - ManualEList.Last() / ATOE);
                Manualscores[2] = (int)100 * (Math.Abs((1 - delayTime / ATOt)));
            }
            else
            {
                Manualscores[0] = 0;
                Manualscores[1] = 0;
                Manualscores[2] = 0;
            }
            if (ATOEList.Count != 0)
            {
                ATOscores[0] = (int)(2 - sumATOda / ATOEList.Count / (sumManda / ManualdaList.Count)) * 100;
                //Manualscores[1] =(int) 100 * (2 - ManualEList.Last() / ATOEList.Last());
                ATOscores[1] = (int)100 * (2 - ATOEList.Last() / ATOt);
                ATOscores[2] = (int)100 * (Math.Abs((1 - delayTime / ATOE)));
            }
            else
            {
                ATOscores[0] = 0;
                ATOscores[1] = 0;
                ATOscores[2] = 0;
            }

            
            for (int i = 0; i < Manualscores.Length; i++)
            {
                if (Manualscores[i] <= 0)
                    Manualscores[i] = 10;
            }
            for (int i = 0; i < ATOscores.Length; i++)
            {
                if (ATOscores[i] <= 0)
                    ATOscores[i] = 80;   //ATO<=0的都给80
            }
        }
    }
}

    

