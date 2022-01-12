using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace mpeg2_demuxer
{
    class Program
    {
        static byte[] header0 = { 0x00, 0x00, 0x01, 0xb9 };
        static byte[] header1 = { 0x00, 0x00, 0x01, 0xba };

        static void Main(string[] args)
        {
            string[] psses = Directory.GetFiles("pss");

            List<string> dupes = new List<string>();
            int frame_dupe_counter = 0;
            //BinaryWriter bw = new BinaryWriter(new FileStream("demuxed.pss", FileMode.Create));

            Dictionary<string, BinaryWriter> writers = new Dictionary<string, BinaryWriter>();

            for (int pss = 0; pss < psses.Length; pss++)
            {
                using (BinaryReader pss_reader = new BinaryReader(new FileStream(psses[pss], FileMode.Open)))
                {
                    while (pss_reader.BaseStream.Position != pss_reader.BaseStream.Length)
                    {
                        byte[] frame_header = pss_reader.ReadBytes(4);

                        if (frame_header.SequenceEqual(header1))
                        {
                            pss_reader.BaseStream.Position += 10;
                            continue;
                        }
                        
                        if (frame_header.SequenceEqual(header0))
                        {
                            break;
                        }

                        byte[] _frame_size = pss_reader.ReadBytes(2);
                        Array.Reverse(_frame_size);
                        int frame_size = BitConverter.ToUInt16(_frame_size, 0);

                        byte[] frame_data = pss_reader.ReadBytes(frame_size);

                        string hash = GetMD5(frame_data);

                        if(dupes.Contains(hash))
                        {
                            frame_dupe_counter++;
                        }
                        else
                        {
                            dupes.Add(hash);
                            //bw.Write(frame_data);

                            string frame_id = BitConverter.ToString(frame_header);

                            if (writers.ContainsKey(frame_id))
                            {
                                writers[frame_id].Write(frame_data);
                            }
                            else
                            {
                                writers.Add(frame_id, new BinaryWriter(new FileStream(frame_id, FileMode.Create)));
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, BinaryWriter> writer in writers)
            {
                writer.Value.Close();
            }

            Console.WriteLine(frame_dupe_counter);
        }

        private static string GetMD5(byte[] frame_data)
        {
            string md5 = string.Empty;
            using (MD5 hash = MD5.Create())
            {
                md5 = BitConverter.ToString(hash.ComputeHash(frame_data)).Replace("-", "");
            }
            return md5;
        }

        private static byte[] Endian(byte[] temp)
        {
            //byte[] swap = 
            return null;
        }
    }
}
