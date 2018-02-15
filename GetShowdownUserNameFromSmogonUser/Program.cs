using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetShowdownUserNameFromSmogonUser
{
    class Program
    {

        private static WebClient client = new WebClient();

        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                StreamReader sr = new StreamReader("output.json");
                Dictionary<string, User> nameUserTranslation = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, User>>(sr.ReadToEnd());
                sr.Close();
                string userToScanFor = args[0];
                if (nameUserTranslation.ContainsKey(Regex(userToScanFor)))
                {
                    List<string> savedUsernames = new List<string>();
                    Dictionary<string, string> oppForUsername = new Dictionary<string, string>();
                    string toScanFor = Regex(userToScanFor);
                    foreach (Match match in nameUserTranslation[toScanFor].matches)
                    {
                        string opponent = (match.firstUser == toScanFor) ? match.secondUser : match.firstUser;
                        if (opponent == null) opponent = "";
                        foreach (string replay in match.replays)
                        {
                            string userOne = "";
                            string userTwo = "";
                            if(!GetUsersForReplay(replay, ref userOne, ref userTwo)) continue;

                            if (userOne == Regex(opponent))
                            {
                                if (!savedUsernames.Contains(userTwo))
                                {
                                    savedUsernames.Add(userTwo);
                                }
                            }
                            else if (userTwo == Regex(opponent))
                            {
                                if (!savedUsernames.Contains(userOne))
                                {
                                    savedUsernames.Add(userOne);
                                }
                            }
                            else
                            {
                                if (!savedUsernames.Contains(userTwo))
                                {
                                    if (!oppForUsername.ContainsKey(userOne))
                                    {
                                        oppForUsername.Add(userOne, opponent);
                                    }
                                    else
                                    {
                                        if (opponent != oppForUsername[userOne] && !savedUsernames.Contains(userOne))
                                        {
                                            savedUsernames.Add(userOne);
                                        }
                                    }
                                }
                                if (!savedUsernames.Contains(userOne))
                                {
                                    if (!oppForUsername.ContainsKey(userTwo))
                                    {
                                        oppForUsername.Add(userTwo, opponent);
                                    }
                                    else
                                    {
                                        if (opponent != oppForUsername[userTwo] && !savedUsernames.Contains(userTwo))
                                        {
                                            savedUsernames.Add(userTwo);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    FileInfo f = new FileInfo(Regex(userToScanFor) + ".txt");
                    if (f.Exists)
                    {
                        f.Delete();
                    }
                    File.WriteAllText(Regex(userToScanFor) + ".txt", String.Join("\n", savedUsernames));
                }
            }
        }

        private static bool GetUsersForReplay(string replay, ref string userOne, ref string userTwo)
        {
            try
            {
                string output = client.DownloadString(replay);
                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("<h1 "))
                    {
                        string tempLine = line.Substring(line.IndexOf("<a"));
                        tempLine = tempLine.Substring(tempLine.IndexOf("/users/") + "/users/".Length);
                        userOne = ShowdownRegex(tempLine.Substring(0, tempLine.IndexOf("\"")));

                        tempLine = tempLine.Substring(tempLine.IndexOf("<a"));
                        tempLine = tempLine.Substring(tempLine.IndexOf("/users/") + "/users/".Length);
                        userTwo = ShowdownRegex(tempLine.Substring(0, tempLine.IndexOf("\"")));

                        break;
                    }
                }
            }
            catch (WebException)
            {
                return false;
            }
            return true;
        }

        private static Regex rgx = new Regex("[, ]");
        private static string Regex(string toFilter)
        {
            toFilter = rgx.Replace(toFilter, "");
            return toFilter.ToLower();
        }


        private static Regex sdRgx = new Regex("[^a-zA-Z0-9]");
        public static string ShowdownRegex(string toFilter)
        {
            toFilter = sdRgx.Replace(toFilter, "");
            return toFilter.ToLower();
        }
    }
}
