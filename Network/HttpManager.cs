using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using LitJson;
using LandFightBotReborn.Bot;
using System.Net;
using LandFightBotReborn.Utils;

namespace LandFightBotReborn.Network
{
    /// <summary>
    /// By M.Fakhreddin
    /// For request that are not in form of socket conection
    /// Like Login or create account
    /// Note: Some charachters like _ or - are invalid do not use them in your request
    /// It causes to send incomplete request so you would recieve wrong answer
    /// </summary>
    public class HttpManager
    {
        private BotManager player;

        public HttpManager(BotManager player)
        {
            this.player = player;
        }

        public bool getRequest(string uri, String[] headerKeys, String[] headerValues,JsonData retBody,out WebHeaderCollection retHeaders)
        {
            string responseString;
            try
            {
                using (var client = new WebClient())
                {
                    Dictionary<String, String> headers = new Dictionary<String, String>();
                    if (player.user.getUserToken() != "")
                    {
                        client.ResponseHeaders.Add(Constants.HEADERS.COOKIE, setSession(player.user.getUserToken()));
                    }
                    if (headerKeys != null && headerValues != null && headerKeys.Length > 0 && headerValues.Length > 0)
                    {
                        for (int i = 0; i < headerKeys.Length; i++)
                        {
                            client.ResponseHeaders.Add(headerKeys[i] + "", headerValues[i] + "");
                        }
                    }
                    responseString = client.DownloadString(uri);
                    retHeaders = client.ResponseHeaders;
                    retBody=JsonMapper.ToObject(responseString);
                }
                return true;
            }
            catch (Exception e)
            {
                retHeaders = null;
                retBody = null;
            }
            return false;
        }


        /// <summary>
        /// POST request eample:
        /// var response = Http.Post("http://dork.com/service", new NameValueCollection() {
        ///   { "home", "Cosby" },
        ///   { "favorite+flavor", "flies" }
        /// });
        /// </summary>
        /// <param name="uri">reqeset uri</param>
        /// <param name="pairs">Params you want to send</param>
        /// <param name="headerValues">Header values</param>
        /// <param name="headerKeys">Header keys</param>
        /// <param name="retHeaders">Headers which are returned as server responce</param>
        /// <returns></returns>
        public bool postRequset(string uri, NameValueCollection pairs, String[] headerKeys
            , String[] headerValues,out JsonData retBody,out WebHeaderCollection retHeaders)
        {
            byte[] response = null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    if (headerKeys != null)
                    {
                        for (int j = 0; j < headerKeys.Length; j++)
                        {
                            //byte[] binaryVal = System.Text.Encoding.ASCII.GetBytes(values[j]);
                            client.ResponseHeaders.Add(headerKeys[j], headerValues[j]);
                        }
                    }
                    if (player.user.getAccessToken() != "")
                    {
                        client.ResponseHeaders[Constants.HEADERS.COOKIE] = setSession(player.user.getAccessToken());
                    }
                    response = client.UploadValues(uri, pairs);
                    retHeaders = client.ResponseHeaders;
                }
                string resString = Converter.byteToString(response);
                retBody = JsonMapper.ToObject(resString);
                return true;
            }
            catch (Exception e)
            {
                retBody = null;
                retHeaders = null;
            }
            return false;
        }


        public string getSession(WebHeaderCollection headers)
        {
            string rawSession = headers[Constants.HEADERS.SET_COOKIE];
            string session = rawSession.Replace(Constants.HEADERS.SESSION, "").Split(';')[0];
            Logger.debug("Session:" + session);
            return session;
        }

        public string setSession(string token)
        {
            string sessionPart = Constants.HEADERS.SESSION + token + ";";
            return sessionPart;
        }
    }
}