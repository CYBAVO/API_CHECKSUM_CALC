﻿// Copyright (c) 2018-2021 The CYBAVO developers
// All Rights Reserved.
// NOTICE: All information contained herein is, and remains
// the property of CYBAVO and its suppliers,
// if any. The intellectual and technical concepts contained
// herein are proprietary to CYBAVO
// Dissemination of this information or reproduction of this materia
// is strictly forbidden unless prior written permission is obtained
// from CYBAVO.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace c__checksum
{
    class Program
    {
        static string BuildChecksum(List<string> ps, string secret, long time, string r)
        {
            ps.Add(String.Format("t={0}", time));
            ps.Add(String.Format("r={0}", r));
            ps.Sort(delegate(string x, string y){
              return String.Compare(x, y, StringComparison.Ordinal);
            });
            ps.Add(String.Format("secret={0}", secret));
            return sha256(String.Join("&", ps.ToArray()));
        }

        static string sha256(string value)
        {
            StringBuilder builder = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                foreach (Byte b in result)
                {
                    builder.Append(b.ToString("x2"));
                }
            }
            return builder.ToString();
        }

        static string CalculateCallbackCheckSum(string payload, string secret)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(payload+secret));
                return System.Convert.ToBase64String(result)
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }

        static void Example1()
        {
            // calculate the checksum for /v1/sofa/wallets/689664/notifications?from_time=1561651200&to_time=1562255999&type=2

            // ps contains all query strings and post body if any
            List<string> ps = new List<string>(new string[] { "from_time=1561651200", "to_time=1562255999", "type=2" });

            long curTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string checksum = BuildChecksum(ps, "THIS_IS_A_SECRET", curTime, "THIS_IS_A_RANDOM_STRING");

            Console.WriteLine(checksum);
        }

        static void Example2()
        {
            // calculate the checksum for /v1/sofa/wallets/689664/autofee
            // post body: {"block_num":1}

            // ps contains all query strings and post body if any
            List<string> ps = new List<string>(new string[] { "{\"block_num\":1}" });

            long curTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string checksum = BuildChecksum(ps, "THIS_IS_A_SECRET", curTime, "THIS_IS_A_RANDOM_STRING");

            Console.WriteLine(checksum);
        }

        static void CallbackChecksumExample()
        {
            // calculate the checksum for callback notification
            string payload = "{\"merchant_id\":1164830639,\"order_id\":\"N1164830639_0000001023\",\"currency\":\"ETH\",\"txid\":\"0xd4cd1899713de17214b417890ee50835915469bd3d8cae9464f50a59dfa69f2e\",\"recv_amount\":\"1000000000000000000\",\"broadcast_at\":1618199781,\"block_height\":10020669,\"from_address\":\"0x4d5DCF7e1FA43770082910107573C8fF4AD0866c\",\"to_address\":\"0x8F8B9C746c51C89Fbe83221930eC6e89C3A18918\",\"state\":0,\"addon\":{},\"currency_bip44\":60,\"token_address\":\"\",\"decimal\":18,\"fee\":\"1428000000000000\",\"fee_decimal\":18}";

            string checksum = CalculateCallbackCheckSum(payload, "23qZHkPaG24QrK6bDW9sXbEL7JkB");

            Console.WriteLine(checksum);
        }
        
        static void Main(string[] args)
        {
            Example1();
            Example2();
            CallbackChecksumExample();
        }
    }
}