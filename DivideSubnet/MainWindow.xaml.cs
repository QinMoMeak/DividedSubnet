using SubnetConsoleDemo;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DivideSubnet
{
    /// MainWindow.xaml 的交互逻辑
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCalculateSubnet_Click(object sender, RoutedEventArgs e)
        {
            string msg;
            string ipStr, prefixStr,sub_numStr,host_numStr;//IP 子网掩码 子网个数 主机数
            int prefix,sub_num,host_num;

            string Str = txtIP.Text.ToString().Trim();
            sub_numStr = txtSub_num.Text.ToString().Trim();
            host_numStr = txtHost_num.Text.ToString().Trim();

            if (!Int32.TryParse(sub_numStr, out sub_num)&&(sub_numStr.Length!=0))
            {
                MessageBox.Show("子网数应该是一个数字");
                return;
            }
            /*for (int i= 0; i < host_numStr.Length; i++)
            {
                if (host_numStr[i]=='，')
                    host_numStr[i]=',';
            }*/

            host_numStr = host_numStr.Replace('，', ',');

            int[] Num_sub = new int[1024];
            String Str_combox = combox.SelectedItem.ToString().Trim();
            int pos = Str_combox.IndexOf(":");
            string combox_Str = Str_combox.Substring(pos+2);
            // MessageBox.Show(Convert.ToString(combox_Str.Length));
            // MessageBox.Show(combox_Str);

          

          

            if (Str.IndexOf("/") == -1)
            {
                MessageBox.Show("网络地址块格式不标准 请重新输入！");
                return;
            }
            

            String[] splitStrings = new String[3];
            splitStrings = Str.Split('/');
            ipStr = splitStrings[0];
            prefixStr = splitStrings[1];
            //ipStr = txtIP.Text.ToString().Trim();
            // prefixStr = txtPrefix.Text.ToString().Trim();
            //int pos = Str.find_last_of("/");
            //string prefixStr(Str, pos+1,Str.length() - pos);
            //ipStr = Str.replace(pos - 1, Str.length() - pos, "");

            if (!Int32.TryParse(prefixStr,out prefix))
            {
                MessageBox.Show("子网掩码应该是一个数字");
                return;
            }

            if (!((prefix >= 0) && (prefix <= 32)))
            {
                MessageBox.Show("子网掩码应该是一个从0到32的数字");
                return;
            }
            /*Int32.TryParse(txt_Prefix.Text.ToString().Trim(), out txt_host);
            if (Math.Pow(2,32-prefix) <= txt_host)
            {
                MessageBox.Show("每个子网的主机个数太多啦！！！");
                return;
            }*/

            if (combox_Str.Equals("固定长度"))//固定长度 将主机数数组格式化为固定长度
            {
                // MessageBox.Show(combox_Str);
                if (host_numStr.Length == 0)
                {
                    if (sub_numStr.Length == 0)
                    {
                        Network[] subnet = Network.DivideSubnet(ipStr, prefix, out msg);

                        if (subnet == null)
                        {
                            MessageBox.Show(msg);
                            return;
                        }

                        dataGrid.ItemsSource = subnet;
                        return;
                    }
                    else
                    {

                        int temp_log = 1, temp = 2;
                        while (true)
                        {
                            if (sub_num <= temp)
                            {
                                break;
                            }
                            temp_log++;
                            temp <<= 1;
                        }
                        temp_log = 32 - prefix - temp_log;
                        if (temp_log < 2)
                        {
                            MessageBox.Show("留给主机号的位数太少啦（建议减少子网数）！！！");
                            return;
                        }
                        host_num = 1;
                        for (int i = 0; i < temp_log; i++)
                        {
                            host_num <<= 1;
                        }
                        host_num -= 2;
                    }
                }
                //MessageBox.Show(host_num.ToString());
                else if(!Int32.TryParse(host_numStr, out host_num))
                {
                    MessageBox.Show("非合法固定长度！！！（应为一合适的正整数）");
                    return;
                }
                // MessageBox.Show(Convert.ToString(host_num));
                for (int i = 0; i < sub_num; i++)
                {
                    Num_sub[i] = host_num;
                }

            }

            //  String s1 = s


            else//可变数组 依次赋值并排序
            {
                String[] splitString = null;
                // Console.WriteLine(Str);
                splitString = host_numStr.Split(',');
                //MessageBox.Show(host_numStr);

                if (sub_num != splitString.Length)
                {
                    MessageBox.Show("可变长度数据个数与子网数不符！！！");
                    return;
                }
                for (int i = 0; i < sub_num; i++)
                {
                    int temp_host;
                    Int32.TryParse(splitString[i], out temp_host);
                    Num_sub[i] = temp_host + 2;
                }

                for (int i = 0; i < sub_num; i++)
                {
                    for (int j = i; j < sub_num; j++)
                    {
                        int temp_;
                        if (Num_sub[j] > Num_sub[i])
                        {
                            temp_ = Num_sub[i];
                            Num_sub[i] = Num_sub[j];
                            Num_sub[j] = temp_;
                        }
                    }
                }
            }


            Network[] subnets = Network.DivideSubnet_CIDR(ipStr, prefix, sub_num, Num_sub, out msg);

            if(subnets == null)
            {
                MessageBox.Show(msg);
                return;
            }

            dataGrid.ItemsSource = subnets;
        }

       
    }
}
