using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using LeagueSharp;

namespace SKOJarvanIV
{
    class Updater
    {
        private readonly string _updatelink;
        private readonly WebClient _wc = new WebClient()
        {
            Proxy = null
        };
        public bool NeedUpdate;

        public Updater(string versionlink, string updatelink, int localversion) {
            this._updatelink = updatelink;
            this.NeedUpdate = Convert.ToInt32(this._wc.DownloadString(versionlink)) > localversion;        
        }

        public bool Update() 
        {
            bool flag;
            try
            {
                string[] location = new string[] { Assembly.GetExecutingAssembly().Location };
                if (File.Exists(string.Concat(Path.Combine(location), ".bak")))
                {
                    string[] strArrays = new string[] { Assembly.GetExecutingAssembly().Location };
                    File.Delete(string.Concat(Path.Combine(strArrays), ".bak"));
                }
                string str = Assembly.GetExecutingAssembly().Location;
                string[] location1 = new string[] { Assembly.GetExecutingAssembly().Location };
                File.Move(str, string.Concat(Path.Combine(location1), ".bak"));
                WebClient webClient = this._wc;
                string str1 = this._updatelink;
                string[] strArrays1 = new string[] { Assembly.GetExecutingAssembly().Location };
                webClient.DownloadFile(str1, Path.Combine(strArrays1));
                flag = true;
            }
            catch (Exception exception)
            {
                Game.PrintChat(string.Concat("<font color='#1d87f2'>SKOJarvanIV: ", exception.Message));
                flag = false;
            }
            return flag;
        }
    }
}
