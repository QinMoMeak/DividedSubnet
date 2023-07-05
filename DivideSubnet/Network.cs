using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SubnetConsoleDemo
{
    /// ByJuston & 苒
    class Network
    {
        //匹配IP地址的正则表达式
        public const String MATCH_IP_ADDRESS_REGULAR_EXPRESSION = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";

        //网络号 子网掩码
        private byte[] networkID;

        //默认前缀
        private int defaultPrefix,prefix;

        //第一个可用主机的地址 最后一个可用主机的地址 广播地址
        private byte[] firstHost, lastHost, broadcast;

        //可用主机数
        private int hostsAvailable;

        public String NetworkID { get => getDottedDecimalByByte(networkID); set => networkID = getByteByDottedDecimal(value); }
        public int DefaultPrefix { get => defaultPrefix;}
        public int Prefix { get => prefix; }
        public String FirstHost { get => getDottedDecimalByByte(firstHost);}
        public String LastHost { get => getDottedDecimalByByte(lastHost); }
        public String Broadcast { get => getDottedDecimalByByte(broadcast); }
        public int HostsAvailable { get => hostsAvailable;}
        

        /// 构造函数
     
        public Network(String ipStr, int defaultPrefix, int prefix,int availableHost,String firstHost,String lastHost,String broadcast)
        {
            NetworkID = ipStr;
            this.prefix = prefix;
            this.defaultPrefix = defaultPrefix;
            this.firstHost = getByteByDottedDecimal(firstHost);
            this.lastHost = getByteByDottedDecimal(lastHost);
            this.broadcast = getByteByDottedDecimal(broadcast);
            this.hostsAvailable = availableHost;
        }

        /// 通过二进制获取点分十进制

        /// <param name="data">ip地址的二进制形式</param>
        /// <returns>如果合法那么返回点分十进制形式的IP地址，否则返回null</returns>
        public static String getDottedDecimalByByte(byte[] data)
        {
            //如果不是一个4字节（32位）的，那么直接return
            if (data == null)
                return null;
            if (data.Length != 4)
                return null;

            //计算四个部分的十进制形式
            String first = Convert.ToString(data[0],10);
            String second = Convert.ToString(data[1], 10);
            String third = Convert.ToString(data[2], 10);
            String fourth = Convert.ToString(data[3], 10);

            //拼接成点分十进制形式
            String ipAddress = $"{first}.{second}.{third}.{fourth}";

            return ipAddress;
        }

        /// <summary>
        /// 通过点分十进制获取其二进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] getByteByDottedDecimal(String ipStr)
        {
            //如果字符串为空或者不负责点分十进制IP地址表示形式的直接return空
            if (ipStr == null)
                return null;
            if (!Regex.IsMatch(ipStr, MATCH_IP_ADDRESS_REGULAR_EXPRESSION))
                return null;

            String[] ipPartion = ipStr.Split('.');

            //计算各部分的二进制并合并为一个
            byte first = Convert.ToByte(ipPartion[0]);
            byte second = Convert.ToByte(ipPartion[1]);
            byte third = Convert.ToByte(ipPartion[2]);
            byte fourth = Convert.ToByte(ipPartion[3]);

            //合并
            byte[] ipAddress = new byte[] { first, second, third, fourth };

            return ipAddress;
        }

        /// 根据IP地址获取默认网络前缀
        /// 不合法 -1;D类地址和E类地址 0;A类地址 8;B类地址 16;C类地址 24
        public static int getDefaultNetworkPrefix(String ipStr)
        {
            byte[] ipBytes = getByteByDottedDecimal(ipStr);
            if (ipBytes == null)
                return -1;

            //Console.WriteLine("getDefaultNetworkPrefix IP " + ipStr);

            String firstByteBinaryStr = Convert.ToString(ipBytes[0],2).PadLeft(8,'0');

            
            Console.WriteLine("firstByteBinaryStr " + firstByteBinaryStr);
            Console.WriteLine("firstByteBinaryStr 0 " + firstByteBinaryStr[0]);
            Console.WriteLine("firstByteBinaryStr 1 " + firstByteBinaryStr[1]);
            Console.WriteLine("firstByteBinaryStr 2 " + firstByteBinaryStr[2]);
            

            if (firstByteBinaryStr[0] == '0')
            {
                //A类地址 8位网络前缀
                return 8;
            }
            else if (firstByteBinaryStr[0] == '1' && firstByteBinaryStr[1] == '0')
            {
                //B类地址 16位网络前缀
                return 16;
            }
            else if (firstByteBinaryStr[0] == '1' && firstByteBinaryStr[1] == '1' && firstByteBinaryStr[2] == '0')
            {
                //C类地址 24位网络前缀
                return 24;
            }
            else
            {
                //D类地址和E类地址 多播地址和保留地址
                return 0;
            }
        }

        public static Network[] DivideSubnet(String ipStr, int dividedPrefix, out String msg)
        {
            //为了方便，首先将IP地址字符串转换为二进制形式
            byte[] ipBytes = Network.getByteByDottedDecimal(ipStr);

            if (ipBytes == null)
            {
                msg = "IP地址不合法";
                return null;
            }

            //计算划分后有几个二进制位
            int defaultPrefix = Network.getDefaultNetworkPrefix(ipStr);
            if (defaultPrefix <= 0)
            {
                Console.WriteLine("defaultPrefix:" + defaultPrefix);
                msg = defaultPrefix == 0 ? "D类地址和E类地址" : "IP地址不合法";
                Console.WriteLine("无法划分子网原因:" + msg);
                return null;
            }

            msg = "划分成功！";

            //借用了几个二进制位
            int borrowedCount = dividedPrefix - defaultPrefix;
            //Console.WriteLine("borrowedCount : " + borrowedCount);

            //网络号部分
            byte[] networkID = null;
            if (defaultPrefix == 8)
            {
                networkID = new byte[] { ipBytes[0] };
            }
            else if (defaultPrefix == 16)
            {
                networkID = new byte[] { ipBytes[0], ipBytes[1] };
            }
            else
            {
                networkID = new byte[] { ipBytes[0], ipBytes[1], ipBytes[2] };
            }

            Network[] subnets = new Network[(int)Math.Pow(2, borrowedCount)];

            //计算新的子网
            //从全0到全1(2的n次方,n是借用的二进制位数)
            for (int i = 0; i < subnets.Length; i++)
            {
                //子网的二进制字符串
                String subnetNetworkIDBinaryStr = "";

                //借用的部分 
                String borrowedStr = Convert.ToString(i, 2).PadLeft(borrowedCount, '0');

                //注意，如果借用的数量为0 也就是没有子网，那么这里应该为空
                if (borrowedCount == 0)
                    borrowedStr = String.Empty;

                //网络号部分
                for (int j = 0; j < networkID.Length; j++)
                {
                    subnetNetworkIDBinaryStr += Convert.ToString(networkID[j], 2).PadLeft(8, '0');
                }

                //Console.WriteLine("网络号部分 + " + subnetNetworkIDBinaryStr);
                //Console.WriteLine("借用的网络号部分" + borrowedStr);

                //追加上借用的网络号部分
                subnetNetworkIDBinaryStr += borrowedStr;

                //首位主机 主机地址最后一位是1，其余位都是0
                String subnetFirstHost = subnetNetworkIDBinaryStr + ("1".PadLeft(32 - dividedPrefix, '0'));
                //末尾主机 主机地址最后一位是0，其余位都是1
                String subnetLastHost = subnetNetworkIDBinaryStr + ("0".PadLeft(32 - dividedPrefix, '1'));
                //广播地址 主机地址全都是1
                String subnetBroadcast = subnetNetworkIDBinaryStr + ("1".PadLeft(32 - dividedPrefix, '1'));
                //在右边补0
                subnetNetworkIDBinaryStr = subnetNetworkIDBinaryStr.PadRight(32, '0');
                //可用主机数
                int availableHost = (int)Math.Pow(2, 32 - dividedPrefix) - 2;

                /*
                Console.WriteLine("ipStr" + ipStr);
                Console.WriteLine("subnetFirstHost" + subnetFirstHost);
                Console.WriteLine("subnetLastHost " + subnetLastHost);
                Console.WriteLine("subnetBroadcast" + subnetBroadcast);
                Console.WriteLine("availableHost" + availableHost);
                */


                //计算各个部分并整合
                ipStr = getIPByByteString(subnetNetworkIDBinaryStr);
                subnetFirstHost = getIPByByteString(subnetFirstHost);
                subnetLastHost = getIPByByteString(subnetLastHost);
                subnetBroadcast = getIPByByteString(subnetBroadcast);

                Console.WriteLine("***********************************");
                Console.WriteLine("IP地址" + ipStr + "/" + dividedPrefix);
                Console.WriteLine("首位主机" + subnetFirstHost);
                Console.WriteLine("末尾主机 " + subnetLastHost);
                Console.WriteLine("广播地址" + subnetBroadcast);
                Console.WriteLine("可用主机" + availableHost);
                Console.WriteLine("***********************************");

                subnets[i] = new Network(ipStr, dividedPrefix, dividedPrefix, availableHost, subnetFirstHost, subnetLastHost, subnetBroadcast);
            }

            return subnets;
        }

   
    


    //##############################################################################
    public static int Log (int hosts)
        {
            int temp = 1;
            int k = 0;
            while (temp < hosts) { temp <<= 1; k++; }
            return k;
        }



        public static Network[] DivideSubnet_CIDR(String ipStr, int dividedPrefix, int subnet_cnt, int[] host_num, out String msg)
        {
/*
            for (int i = 0; i < subnet_cnt; i++)
                Console.WriteLine(host_num[i]);
*/
            int divi_temp = dividedPrefix;

            int Act_len = 32 - dividedPrefix;
            int sum = subnet_cnt;
            double loc_;
            int[] loc_num = new int[31];

            loc_num[subnet_cnt - sum] = Log(host_num[subnet_cnt - sum]);
            sum--;
            loc_ = Math.Pow(2.0, (double)(Act_len - loc_num[0]));

            if (loc_num[0]+Log(subnet_cnt)>Act_len)
            {
                msg = "输入数据不合法！！（子网数太多或单个子网主机数太多）";
                Console.WriteLine("无法划分子网原因:" + msg);
                return null;
            }

            int temp_log_ = 0;
            while (true)
            {

                int temp_loc = subnet_cnt - sum;
                loc_num[temp_loc] = Log(host_num[temp_loc]);
                temp_log_ += loc_num[temp_loc];
                int temp_log = loc_num[temp_loc - 1] - loc_num[temp_loc];
                loc_ = (loc_ - 1) * Math.Pow(2.0, (double)temp_log);
                sum--;
                if (sum == 0) break;
                if (loc_ == 0)
                {
                    msg = "输入数据不合法！！（子网数太多或单个子网主机数太多）";
                    Console.WriteLine("无法划分子网原因:" + msg);
                    return null;
                }
            }
           // if (temp_log_+Log())

           /* for (int i=0;i<sum;i++)
            Console.WriteLine(loc_num[i]);


            Console.WriteLine("@@@@@@@@@@@@@@@@@@@");

            Console.WriteLine(ipStr);
            Console.WriteLine(dividedPrefix);*/

           /* //为了方便，首先将IP地址字符串转换为二进制形式
            byte[] ipBytes = Network.getByteByDottedDecimal(ipStr);

            if (ipBytes == null)
            {
                msg = "IP地址不合法";
                return null;
            }

            //计算划分后有几个二进制位
            int defaultPrefix = Network.getDefaultNetworkPrefix(ipStr);
            if (defaultPrefix <= 0)
            {
                Console.WriteLine("defaultPrefix:" + defaultPrefix);
                msg = defaultPrefix == 0 ? "D类地址和E类地址" : "IP地址不合法";
                Console.WriteLine("无法划分子网原因:" + msg);
                return null;
            }

            msg = "划分成功！";

            //借用了几个二进制位
            int borrowedCount = dividedPrefix - defaultPrefix;
            //Console.WriteLine("borrowedCount : " + borrowedCount);

            //网络号部分
            byte[] networkID = null;
            if (defaultPrefix == 8)
            {
                networkID = new byte[] { ipBytes[0] };
            }
            else if (defaultPrefix == 16)
            {
                networkID = new byte[] { ipBytes[0], ipBytes[1] };
            }
            else
            {
                networkID = new byte[] { ipBytes[0], ipBytes[1], ipBytes[2] };
            }*/

            // Network[] subnets = new Network[(int)Math.Pow(2, borrowedCount)];
            Network[] subnets = new Network[subnet_cnt];
            msg = "划分成功！";
            int last = 0;

            // dividedPrefix = a - loc_num[0];
            String[] splitString = null;
            splitString = ipStr.Split('.');
            int[] tem_num = new int[4];
            Int32.TryParse(splitString[0], out tem_num[0]);
            Int32.TryParse(splitString[1], out tem_num[1]);
            Int32.TryParse(splitString[2], out tem_num[2]);
            Int32.TryParse(splitString[3], out tem_num[3]);

            string old_str = (Convert.ToString(tem_num[0], 2).PadLeft(8, '0') +
                              Convert.ToString(tem_num[1], 2).PadLeft(8, '0') +
                              Convert.ToString(tem_num[2], 2).PadLeft(8, '0') +
                              Convert.ToString(tem_num[3], 2).PadLeft(8, '0')).Substring(0,dividedPrefix);
            Console.WriteLine(old_str);

            for (int i = 0; i < subnets.Length; i++)
            {
                string temp_str = "".PadLeft(Act_len, '0');
                int availableHost = (int) Math.Pow(2.0, loc_num[i]);
                ipStr = Convert.ToString(last, 2).PadLeft(Act_len, '0');
                /*String subnetFirstHost = (last + (int)Math.Pow(2.0, loc_num[i]);*/
                String subnetFirstHost = Convert.ToString(last +1, 2).PadLeft(Act_len, '0');
                String subnetLastHost = Convert.ToString((last + availableHost-2), 2).PadLeft(Act_len, '0');
               
                last = last + (int)Math.Pow(2.0, loc_num[i]);


                String subnetBroadcast = Convert.ToString(last-1, 2).PadLeft(Act_len, '0');
               
                Console.WriteLine(subnetBroadcast);

                subnetFirstHost = getIPByByteString(old_str + subnetFirstHost);
                subnetLastHost = getIPByByteString(old_str + subnetLastHost);
                subnetBroadcast = getIPByByteString(old_str + subnetBroadcast);
                ipStr = getIPByByteString(old_str + ipStr);

                subnets[i] = new Network(ipStr, dividedPrefix, 32-loc_num[i],availableHost-2, subnetFirstHost, subnetLastHost,subnetBroadcast);
            }
            return subnets;
            /*
            //计算新的子网
            //从全0到全1(2的n次方,n是借用的二进制位数)
            for (int i = 0; i < subnets.Length; i++)
            {
                //子网的二进制字符串
                String subnetNetworkIDBinaryStr = "";

                //借用的部分 
                String borrowedStr = Convert.ToString(i, 2).PadLeft(borrowedCount, '0');

                //注意，如果借用的数量为0 也就是没有子网，那么这里应该为空
                if (borrowedCount == 0)
                    borrowedStr = String.Empty;

                //网络号部分
                for (int j = 0; j < networkID.Length; j++)
                {
                    subnetNetworkIDBinaryStr += Convert.ToString(networkID[j], 2).PadLeft(8,'0');
                }

                //Console.WriteLine("网络号部分 + " + subnetNetworkIDBinaryStr);
                //Console.WriteLine("借用的网络号部分" + borrowedStr);

                //追加上借用的网络号部分
                subnetNetworkIDBinaryStr += borrowedStr;

                //首位主机 主机地址最后一位是1，其余位都是0
                String subnetFirstHost = subnetNetworkIDBinaryStr + ("1".PadLeft(32 - dividedPrefix,'0'));
                //末尾主机 主机地址最后一位是0，其余位都是1
                String subnetLastHost = subnetNetworkIDBinaryStr + ("0".PadLeft(32 - dividedPrefix, '1'));
                //广播地址 主机地址全都是1
                String subnetBroadcast = subnetNetworkIDBinaryStr + ("1".PadLeft(32 - dividedPrefix, '1'));
                //在右边补0
                subnetNetworkIDBinaryStr = subnetNetworkIDBinaryStr.PadRight(32, '0');
                //可用主机数
                int availableHost = (int)Math.Pow(2, 32 - dividedPrefix) - 2;

                /*
                Console.WriteLine("ipStr" + ipStr);
                Console.WriteLine("subnetFirstHost" + subnetFirstHost);
                Console.WriteLine("subnetLastHost " + subnetLastHost);
                Console.WriteLine("subnetBroadcast" + subnetBroadcast);
                Console.WriteLine("availableHost" + availableHost);
                */

            /*
             * 
        //计算各个部分并整合
        ipStr = getIPByByteString(subnetNetworkIDBinaryStr);
        subnetFirstHost = getIPByByteString(subnetFirstHost);
        subnetLastHost = getIPByByteString(subnetLastHost);
        subnetBroadcast = getIPByByteString(subnetBroadcast);

        Console.WriteLine("***********************************");
        Console.WriteLine("IP地址" + ipStr + "/" + dividedPrefix);
        Console.WriteLine("首位主机" + subnetFirstHost);
        Console.WriteLine("末尾主机 " + subnetLastHost);
        Console.WriteLine("广播地址" + subnetBroadcast);
        Console.WriteLine("可用主机" + availableHost);
        Console.WriteLine("***********************************");

        subnets[i] = new Network(ipStr, dividedPrefix, host_num[i], subnetFirstHost, subnetLastHost, subnetBroadcast);
    }

    */

        }

        public static String getIPByByteString(String byteStr)
        {
            //计算各个部分
            String first = byteStr.Substring(0, 8);
            String second = byteStr.Substring(8, 8);
            String thrid = byteStr.Substring(16, 8);
            String fourth = byteStr.Substring(24, 8);
            String ipStr = $"{Convert.ToInt32(first, 2)}.{Convert.ToInt32(second, 2)}.{Convert.ToInt32(thrid, 2)}.{Convert.ToInt32(fourth, 2)}";
            return ipStr;
        }
    }
}
